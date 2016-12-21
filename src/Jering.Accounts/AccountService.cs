using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.Accounts.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Data.SqlClient;
using MimeKit;
using Jering.Mail;
using System.Net;
using Jering.Security;
using Jering.Utilities;
using System.Threading;

namespace Jering.Accounts
{
    /// <summary>
    /// Provides an API for managing Account security.
    /// </summary>
    public class AccountService<TAccount> : IAccountService<TAccount>
        where TAccount : IAccount, new()
    {
        private IClaimsPrincipalService<TAccount> _claimsPrincipalService { get; }
        private IAccountRepository<TAccount> _accountRepository { get; }
        private HttpContext _httpContext { get; }
        private AccountServiceOptions _options { get; }
        private IEmailService _emailService { get; }
        private IPasswordService _passwordService { get; }
        private ITimeService _timeService { get; }
        private CancellationToken _cancellationToken { get; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, ITokenService<TAccount>> TokenServices { get; } = new Dictionary<string, ITokenService<TAccount>>();

        /// <summary>
        /// Constructs a new instance of <see cref="AccountService{TAccount}"/>.
        /// </summary>
        /// <param name="claimsPrincipalService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="optionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="emailService"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="passwordService"></param>
        /// <param name="timeService"></param>
        public AccountService(IClaimsPrincipalService<TAccount> claimsPrincipalService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountServiceOptions> optionsAccessor,
            IAccountRepository<TAccount> accountRepository,
            IEmailService emailService,
            IServiceProvider serviceProvider,
            IPasswordService passwordService)
        {
            _emailService = emailService;
            _claimsPrincipalService = claimsPrincipalService;
            _httpContext = httpContextAccessor?.HttpContext;
            _cancellationToken = _httpContext?.RequestAborted ?? CancellationToken.None;
            _options = optionsAccessor?.Value;
            _accountRepository = accountRepository;
            _passwordService = passwordService;

            if (serviceProvider != null)
            {
                foreach (string tokenServiceName in _options.TokenServiceOptions.TokenServiceMap.Keys)
                {
                    ITokenService<TAccount> tokenService = (ITokenService<TAccount>)serviceProvider.
                        GetRequiredService(_options.TokenServiceOptions.TokenServiceMap[tokenServiceName]);
                    RegisterTokenProvider(tokenServiceName, tokenService);
                }
            }
        }

        #region Session
        /// <summary>
        /// Logs in to account with email <paramref name="email"/> and password <paramref name="password"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="isPersistent"></param>
        /// <returns>
        /// <see cref="LogInActionResult.InvalidEmail"/> if email is invalid.
        /// <see cref="LogInActionResult.InvalidPassword"/> if password is invalid.
        /// <see cref="LogInActionResult.TwoFactorRequired"/> if two factor is required.
        /// <see cref="LogInActionResult.Success"/> if log in is successful. 
        /// </returns>
        public virtual async Task<LogInActionResult> LogInActionAsync(string email, string password, bool isPersistent)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException(nameof(email));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(nameof(password));
            }

            TAccount account = await _accountRepository.GetAsync(email, _cancellationToken);

            if (account == null)
            {
                return LogInActionResult.InvalidEmail;
            }

            if (!_passwordService.ValidatePassword(account.PasswordHash, password))
            {
                return LogInActionResult.InvalidPassword;
            }

            if (account.TwoFactorEnabled)
            {
                await TwoFactorLogInAsync(account);
                await SendTwoFactorCodeEmailAsync(account);

                return LogInActionResult.TwoFactorRequired;
            }

            await ApplicationLogInAsync(account, new AuthenticationProperties { IsPersistent = isPersistent });

            return LogInActionResult.Success;
        }

        /// <summary>
        /// Logs off logged in account.
        /// </summary>
        public virtual async Task LogOffActionAsync()
        {
            await ApplicationLogOffAsync();
        }

        /// <summary>
        /// Logs in to account if <paramref name="code"/> is valid.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="isPersistent"></param>
        /// <returns>
        /// <see cref="TwoFactorLogInActionResult.InvalidCredentials"/> if two factor credentials are invalid.
        /// <see cref="TwoFactorLogInActionResult.InvalidCode"/> if code is invalid.
        /// <see cref="TwoFactorLogInActionResult.Success"/> if log in is successful. 
        /// </returns>
        public virtual async Task<TwoFactorLogInActionResult> TwoFactorLogInActionAsync(string code, bool isPersistent)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException(nameof(code));
            }

            TAccount account = await GetTwoFactorAccountAsync();

            if (account == null)
            {
                return TwoFactorLogInActionResult.InvalidCredentials;
            }

            ValidateTokenResult validateTokenResult = ValidateToken(TokenServiceOptions.TotpTokenService, 
                _options.TwoFactorTokenPurpose, 
                account, 
                code);

            if(validateTokenResult == ValidateTokenResult.Valid)
            {
                await TwoFactorLogOffAsync();
                await ApplicationLogInAsync(account, new AuthenticationProperties() { IsPersistent = isPersistent });

                return TwoFactorLogInActionResult.Success;
            }

            return TwoFactorLogInActionResult.InvalidCode;
        }
        #endregion

        #region Account Management
        /// <summary>
        /// Creates an account with email <paramref name="email"/> and password <paramref name="password"/>, 
        /// logs in to account and sends email verification email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// <see cref="SignUpActionResult.EmailInUse"/> if account email is in use.
        /// <see cref="SignUpActionResult.Success"/> if sign up is successful. 
        /// </returns>
        public virtual async Task<SignUpActionResult> SignUpActionAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException(nameof(email));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(nameof(password));
            }

            TAccount account = await _accountRepository.CreateAsync(email,
                _passwordService.HashPassword(password),
                _cancellationToken);

            if (account == null)
            {
                return SignUpActionResult.EmailInUse;
            }

            await SendEmailVerificationEmailAsync(account);

            await ApplicationLogInAsync(account, new AuthenticationProperties { IsPersistent = true });

            return SignUpActionResult.Success;
        }

        /// <summary>
        /// Sends reset password email to <paramref name="email"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// <see cref="SendResetPasswordEmailActionResult.InvalidEmail"/> if <paramref name="email"/> is invalid.
        /// <see cref="SendResetPasswordEmailActionResult.Success"/> if email sends successfully.
        /// </returns>
        public virtual async Task<SendResetPasswordEmailActionResult> SendResetPasswordEmailActionAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException(nameof(email));
            }

            TAccount account = await _accountRepository.GetAsync(email, _cancellationToken);
            if (account != null)
            {
                await SendResetPasswordEmailAsync(account);

                return SendResetPasswordEmailActionResult.Success;
            }

            return SendResetPasswordEmailActionResult.InvalidEmail;
        }

        /// <summary>
        /// Resets password for account with email <paramref name="email"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <param name="newPassword"></param>
        /// <returns>
        /// <see cref="ResetPasswordActionResult.InvalidEmail"/> if email is invalid.
        /// <see cref="ResetPasswordActionResult.InvalidNewPassword"/> if new password is same as old password.
        /// <see cref="ResetPasswordActionResult.InvalidToken"/> if token is invalid.
        /// <see cref="ResetPasswordActionResult.Success"/> if password reset is successful.
        /// </returns>
        /// <exception cref="Exception">Thrown if a database error occurs.</exception>
        public virtual async Task<ResetPasswordActionResult> ResetPasswordActionAsync(string email, string token,
            string newPassword)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException(nameof(email));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException(nameof(token));
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException(nameof(newPassword));
            }

            TAccount account = await _accountRepository.GetAsync(email, _cancellationToken);

            if (account == null)
            {
                return ResetPasswordActionResult.InvalidEmail;
            }

            if (_passwordService.ValidatePassword(account.PasswordHash, newPassword))
            {
                return ResetPasswordActionResult.InvalidNewPassword;
            }

            ValidateTokenResult validateTokenResult = ValidateToken(TokenServiceOptions.DataProtectionTokenService,
                _options.ResetPasswordTokenPurpose,
                account,
                token);

            if (validateTokenResult != ValidateTokenResult.Valid)
            {
                return ResetPasswordActionResult.InvalidToken;
            }

            SaveChangesResult result = await _accountRepository.UpdatePasswordHashAsync(account,
                _passwordService.HashPassword(newPassword),
                _cancellationToken);

            if (result == SaveChangesResult.ConcurrencyError)
            {
                // Unrecoverable by calling function.
                throw new Exception();
            }

            return ResetPasswordActionResult.Success;
        }

        /// <summary>
        /// Gets logged in account
        /// </summary>
        /// <returns>
        /// Logged in account if it exists.
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetAccountDetailsActionAsync()
        {
            return await GetApplicationAccountAsync();
        }

        /// <summary>
        /// Sets password of logged in account to <paramref name="newPassword"/>.
        /// </summary>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns>
        /// <see cref="SetPasswordActionResult.Success"/> if password change succeeds.
        /// <see cref="SetPasswordActionResult.AlreadySet"/> if account password is already equal to 
        /// <paramref name="newPassword"/>.
        /// <see cref="SetPasswordActionResult.InvalidCurrentPassword"/> if <paramref name="currentPassword"/> is invalid.
        /// <see cref="SetPasswordActionResult.NoLoggedInAccount"/> if unabled to retrieve logged in account.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<SetPasswordActionResult> SetPasswordActionAsync(string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(currentPassword))
            {
                throw new ArgumentException(nameof(currentPassword));
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException(nameof(newPassword));
            }

            if (currentPassword == newPassword)
            {
                return SetPasswordActionResult.AlreadySet;
            }

            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SetPasswordActionResult.NoLoggedInAccount;
            }

            if (_passwordService.ValidatePassword(account.PasswordHash, currentPassword))
            {
                SaveChangesResult result = await _accountRepository.UpdatePasswordHashAsync(account,
                    _passwordService.HashPassword(newPassword),
                    _cancellationToken);

                if (result == SaveChangesResult.ConcurrencyError)
                {
                    // Unrecoverable
                    throw new Exception();
                }

                // Security stamp has changed
                await RefreshApplicationLogInAsync(account);

                return SetPasswordActionResult.Success;
            }
            else
            {
                return SetPasswordActionResult.InvalidCurrentPassword;
            }
        }

        /// <summary>
        /// Sets email of logged in account to <paramref name="newEmail"/>.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newEmail"></param>
        /// <returns>
        /// <see cref="SetEmailActionResult.Success"/> if email change succeeds.
        /// <see cref="SetEmailActionResult.AlreadySet"/> if account email is already equal to 
        /// <paramref name="newEmail"/>.
        /// <see cref="SetEmailActionResult.InvalidPassword"/> if <paramref name="password"/> is invalid.
        /// <see cref="SetEmailActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="SetEmailActionResult.EmailInUse"/> if <paramref name="newEmail"/> is in use.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<SetEmailActionResult> SetEmailActionAsync(string password, string newEmail)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(nameof(password));
            }
            if (string.IsNullOrEmpty(newEmail))
            {
                throw new ArgumentException(nameof(newEmail));
            }

            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SetEmailActionResult.NoLoggedInAccount;
            }
            if (account.Email == newEmail)
            {
                return SetEmailActionResult.AlreadySet;
            }
            if (_passwordService.ValidatePassword(account.PasswordHash, password))
            {
                SaveChangesResult result = await _accountRepository.UpdateEmailAsync(account,
                    newEmail,
                    _cancellationToken);

                if (result == SaveChangesResult.ConcurrencyError)
                {
                    // Unrecoverable
                    throw new Exception();
                }
                if (result == SaveChangesResult.UniqueIndexViolation)
                {
                    return SetEmailActionResult.EmailInUse;
                }

                // Security stamp has changed
                await RefreshApplicationLogInAsync(account);

                return SetEmailActionResult.Success;
            }
            else
            {
                return SetEmailActionResult.InvalidPassword;
            }
        }

        /// <summary>
        /// Sets alt email of logged in account to <paramref name="newAltEmail"/>.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newAltEmail"></param>
        /// <returns>
        /// <see cref="SetAltEmailActionResult.Success"/> if alt email change succeeds.
        /// <see cref="SetAltEmailActionResult.AlreadySet"/> if account alt email is already equal to 
        /// <paramref name="newAltEmail"/>.
        /// <see cref="SetAltEmailActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="SetAltEmailActionResult.InvalidPassword"/> if <paramref name="password"/> is invalid.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<SetAltEmailActionResult> SetAltEmailActionAsync(string password, string newAltEmail)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(nameof(password));
            }
            if (string.IsNullOrEmpty(newAltEmail))
            {
                throw new ArgumentException(nameof(newAltEmail));
            }

            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SetAltEmailActionResult.NoLoggedInAccount;
            }
            if (account.AltEmail == newAltEmail)
            {
                return SetAltEmailActionResult.AlreadySet;
            }
            if (_passwordService.ValidatePassword(account.PasswordHash, password))
            {
                SaveChangesResult result = await _accountRepository.UpdateAltEmailAsync(account,
                    newAltEmail,
                    _cancellationToken);

                if (result == SaveChangesResult.ConcurrencyError)
                {
                    // Unrecoverable
                    throw new Exception();
                }

                return SetAltEmailActionResult.Success;
            }
            else
            {
                return SetAltEmailActionResult.InvalidPassword;
            }
        }

        /// <summary>
        /// Sets email of logged in account to <paramref name="newDisplayName"/>.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newDisplayName"></param>
        /// <returns>
        /// <see cref="SetDisplayNameActionResult.Success"/> if display name change succeeds.
        /// <see cref="SetDisplayNameActionResult.AlreadySet"/> if account display name is already equal to 
        /// <paramref name="newDisplayName"/>.
        /// <see cref="SetDisplayNameActionResult.InvalidPassword"/> if <paramref name="password"/> is invalid.
        /// <see cref="SetDisplayNameActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="SetDisplayNameActionResult.DisplayNameInUse"/> if <paramref name="newDisplayName"/> is in use.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<SetDisplayNameActionResult> SetDisplayNameActionAsync(string password, string newDisplayName)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(nameof(password));
            }
            if (string.IsNullOrEmpty(newDisplayName))
            {
                throw new ArgumentException(nameof(newDisplayName));
            }

            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SetDisplayNameActionResult.NoLoggedInAccount;
            }
            if (account.DisplayName == newDisplayName)
            {
                return SetDisplayNameActionResult.AlreadySet;
            }
            if (_passwordService.ValidatePassword(account.PasswordHash, password))
            {
                SaveChangesResult result = await _accountRepository.UpdateDisplayNameAsync(account,
                    newDisplayName,
                    _cancellationToken);

                if (result == SaveChangesResult.ConcurrencyError)
                {
                    // Unrecoverable
                    throw new Exception();
                }
                if (result == SaveChangesResult.UniqueIndexViolation)
                {
                    return SetDisplayNameActionResult.DisplayNameInUse;
                }

                return SetDisplayNameActionResult.Success;
            }
            else
            {
                return SetDisplayNameActionResult.InvalidPassword;
            }
        }

        /// <summary>
        /// Sets two factor enabled of logged in account to <paramref name="enabled"/>.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns>
        /// <see cref="SetTwoFactorEnabledActionResult.Success"/> if two factor enabled change succeeds.
        /// <see cref="SetTwoFactorEnabledActionResult.AlreadySet"/> if account two factor enabled is already equal to
        /// <paramref name="enabled"/>.
        /// <see cref="SetTwoFactorEnabledActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="SetTwoFactorEnabledActionResult.EmailUnverified"/> if account email is unverified.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<SetTwoFactorEnabledActionResult> SetTwoFactorEnabledActionAsync(bool enabled)
        {
            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SetTwoFactorEnabledActionResult.NoLoggedInAccount;
            }
            if (!account.EmailVerified)
            {
                await SendTwoFactorCodeEmailAsync(account);

                return SetTwoFactorEnabledActionResult.EmailUnverified;
            }
            if (account.TwoFactorEnabled == enabled)
            {
                return SetTwoFactorEnabledActionResult.AlreadySet;
            }

            SaveChangesResult result = await _accountRepository.UpdateTwoFactorEnabledAsync(account,
                enabled,
                _cancellationToken);

            if (result == SaveChangesResult.ConcurrencyError)
            {
                // Unrecoverable
                throw new Exception();
            }

            return SetTwoFactorEnabledActionResult.Success;
        }

        /// <summary>
        /// Sets email verified of logged in account to true if <paramref name="token"/> is valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <see cref="SetEmailVerifiedActionResult.Success"/> if email verified change succeeds.
        /// <see cref="SetEmailVerifiedActionResult.AlreadySet"/> if account email verified is already true.
        /// <see cref="SetEmailVerifiedActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="SetEmailVerifiedActionResult.InvalidToken"/> if token is invalid.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<SetEmailVerifiedActionResult> SetEmailVerifiedActionAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException(nameof(token));
            }

            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SetEmailVerifiedActionResult.NoLoggedInAccount;
            }
            if (account.EmailVerified == true)
            {
                return SetEmailVerifiedActionResult.AlreadySet;
            }

            ValidateTokenResult validateTokenResult = ValidateToken(TokenServiceOptions.DataProtectionTokenService,
                _options.ConfirmEmailTokenPurpose,
                account,
                token);

            if (validateTokenResult != ValidateTokenResult.Valid)
            {
                return SetEmailVerifiedActionResult.InvalidToken;
            }

            SaveChangesResult result = await _accountRepository.UpdateEmailVerifiedAsync(account,
                true,
                _cancellationToken);

            if (result == SaveChangesResult.ConcurrencyError)
            {
                // Unrecoverable
                throw new Exception();
            }

            return SetEmailVerifiedActionResult.Success;
        }

        /// <summary>
        /// Sets alt email verified of logged in account to true if <paramref name="token"/> is valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <see cref="SetAltEmailVerifiedActionResult.Success"/> if alt email verified change succeeds.
        /// <see cref="SetAltEmailVerifiedActionResult.AlreadySet"/> if account alt email verified is already true.
        /// <see cref="SetAltEmailVerifiedActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="SetAltEmailVerifiedActionResult.InvalidToken"/> if token is invalid.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<SetAltEmailVerifiedActionResult> SetAltEmailVerifiedActionAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException(nameof(token));
            }

            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SetAltEmailVerifiedActionResult.NoLoggedInAccount;
            }
            if (account.AltEmailVerified == true)
            {
                return SetAltEmailVerifiedActionResult.AlreadySet;
            }

            ValidateTokenResult validateTokenResult = ValidateToken(TokenServiceOptions.DataProtectionTokenService,
                _options.ConfirmAltEmailTokenPurpose,
                account,
                token);

            if (validateTokenResult != ValidateTokenResult.Valid)
            {
                return SetAltEmailVerifiedActionResult.InvalidToken;
            }

            SaveChangesResult result = await _accountRepository.UpdateAltEmailVerifiedAsync(account,
                true,
                _cancellationToken);

            if (result == SaveChangesResult.ConcurrencyError)
            {
                // Unrecoverable
                throw new Exception();
            }

            return SetAltEmailVerifiedActionResult.Success;
        }

        /// <summary>
        /// Sends email verification email to account email.
        /// </summary>
        /// <returns>
        /// <see cref="SendEmailVerificationEmailActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// </returns>
        public virtual async Task<SendEmailVerificationEmailActionResult> SendEmailVerificationEmailActionAsync()
        {
            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SendEmailVerificationEmailActionResult.NoLoggedInAccount;
            }

            await SendEmailVerificationEmailAsync(account);

            return SendEmailVerificationEmailActionResult.Success;
        }

        /// <summary>
        /// Sends alt email verification email to account alt email.
        /// </summary>
        /// <returns>
        /// <see cref="SendAltEmailVerificationEmailActionResult.Success"/> if alt email verified change succeeds.
        /// <see cref="SendAltEmailVerificationEmailActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="SendAltEmailVerificationEmailActionResult.NoAltEmail"/> if account alt email is null.
        /// </returns>
        public virtual async Task<SendAltEmailVerificationEmailActionResult> SendAltEmailVerificationEmailActionAsync()
        {
            TAccount account = await GetApplicationAccountAsync();
            if (account == null)
            {
                return SendAltEmailVerificationEmailActionResult.NoLoggedInAccount;
            }

            if (string.IsNullOrEmpty(account.AltEmail))
            {
                return SendAltEmailVerificationEmailActionResult.NoAltEmail;
            }

            await SendAltEmailVerificationEmailAsync(account);

            return SendAltEmailVerificationEmailActionResult.Success;
        }

        /// <summary>
        /// Sets email verified of logged in account to true if <paramref name="code"/> is valid. 
        /// </summary>
        /// <param name="code"></param>
        /// <returns>
        /// <see cref="TwoFactorVerifyEmailActionResult.Success"/> if email verified change succeeds.
        /// <see cref="TwoFactorVerifyEmailActionResult.AlreadySet"/> if account email verified is already true.
        /// <see cref="TwoFactorVerifyEmailActionResult.NoLoggedInAccount"/> if unable to retrieve logged in account.
        /// <see cref="TwoFactorVerifyEmailActionResult.InvalidCode"/> if code is invalid.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly.</exception>
        public virtual async Task<TwoFactorVerifyEmailActionResult> TwoFactorVerifyEmailActionAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException(nameof(code));
            }

            TAccount account = await GetApplicationAccountAsync();

            if (account == null)
            {
                return TwoFactorVerifyEmailActionResult.NoLoggedInAccount;
            }

            if (account.EmailVerified == true)
            {
                return TwoFactorVerifyEmailActionResult.AlreadySet;
            }

            ValidateTokenResult validateTokenResult = ValidateToken(TokenServiceOptions.TotpTokenService,
                _options.TwoFactorTokenPurpose,
                account,
                code);

            if (validateTokenResult == ValidateTokenResult.Invalid)
            {
                return TwoFactorVerifyEmailActionResult.InvalidCode;
            }

            SaveChangesResult result = await _accountRepository.UpdateEmailVerifiedAsync(account,
                true,
                _cancellationToken);

            if (result == SaveChangesResult.ConcurrencyError)
            {
                // Unrecoverable
                throw new Exception();
            }

            return TwoFactorVerifyEmailActionResult.Success;
        }
        #endregion

        #region Helpers
        #region Crypto
        /// <summary>
        /// Adds <paramref name="tokenService"/> to <see cref="TokenServices"/> 
        /// </summary>
        /// <param name="tokenServiceName"></param>
        /// <param name="tokenService"></param>
        public virtual void RegisterTokenProvider(string tokenServiceName, ITokenService<TAccount> tokenService)
        {
            if (string.IsNullOrEmpty(tokenServiceName))
            {
                throw new ArgumentException(nameof(tokenServiceName));
            }
            if (tokenService == null)
            {
                throw new ArgumentNullException(nameof(tokenService));
            }

            TokenServices[tokenServiceName] = tokenService;
        }

        /// <summary>
        /// Generates a token using specified <paramref name="tokenService"/>
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns>
        /// Token
        /// </returns>
        public virtual string GetToken(string tokenService, string purpose, TAccount account)
        {
            if (string.IsNullOrEmpty(tokenService))
            {
                throw new ArgumentException(nameof(tokenService));
            }
            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException(nameof(purpose));
            }
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return TokenServices[tokenService].GenerateToken(purpose, account);
        }

        /// <summary>
        /// Validates <paramref name="token"/> 
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <param name="token"></param>
        /// <returns>
        /// <see cref="ValidateTokenResult"/> returned by <see cref="ITokenService{TAccount}.ValidateToken(string, string, TAccount)"/>.
        /// </returns>
        public virtual ValidateTokenResult ValidateToken(string tokenService, string purpose, TAccount account,
            string token)
        {
            if (string.IsNullOrEmpty(tokenService))
            {
                throw new ArgumentException(nameof(tokenService));
            }
            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException(nameof(purpose));
            }
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException(nameof(token));
            }

            return TokenServices[tokenService].ValidateToken(purpose, token, account);
        }
        #endregion

        #region Application Session
        /// <summary>
        /// Logs in <paramref name="account"/> using specified <paramref name="authProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authProperties"></param>
        public virtual async Task ApplicationLogInAsync(TAccount account, AuthenticationProperties authProperties)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalService.
                CreateClaimsPrincipalAsync(account,
                _options.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                authProperties);
            authProperties.AllowRefresh = true;

            _httpContext.User = claimsPrincipal;

            await _httpContext.Authentication.SignInAsync(
                    _options.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);
        }

        /// <summary>
        /// Logs off logged in account.
        /// </summary>
        public virtual async Task ApplicationLogOffAsync()
        {
            await _httpContext.Authentication.SignOutAsync(_options.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);
        }

        /// <summary>
        /// Refreshes log in for <paramref name="account"/>.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task RefreshApplicationLogInAsync(TAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            _claimsPrincipalService.UpdateClaimsPrincipal(account, _httpContext.User);

            bool isPersistent = Convert.ToBoolean(_httpContext.User.FindFirst(_options.ClaimsOptions.IsPersistenClaimType).Value);

            await _httpContext.Authentication.SignInAsync(
                    _options.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    _httpContext.User,
                    new AuthenticationProperties { IsPersistent = isPersistent, AllowRefresh = true });
        }

        /// <summary>
        /// Gets email account of logged in account using <see cref="HttpContext.User"/> .
        /// </summary>
        /// <returns>
        /// Email if it exists. 
        /// Null otherwise.
        /// </returns>
        public virtual string GetApplicationAccountEmail()
        {
            return _httpContext.User?.FindFirst(_options.ClaimsOptions.UsernameClaimType)?.Value;
        }

        /// <summary>
        /// Gets logged in account for <see cref="HttpContext.User"/>.
        /// </summary>
        /// <returns>
        /// An account if there is a logged in account.
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetApplicationAccountAsync()
        {
            if (_httpContext.User == null)
            {
                return default(TAccount);
            }

            if (_httpContext.User.Identity?.AuthenticationType == _options.CookieOptions.ApplicationCookieOptions.AuthenticationScheme)
            {
                System.Security.Claims.Claim accountIdClaim = _httpContext.User.FindFirst(_options.ClaimsOptions.AccountIdClaimType);
                if (accountIdClaim != null)
                {
                    return await _accountRepository.GetAsync(Convert.ToInt32(accountIdClaim.Value), _cancellationToken);
                }
            }

            return default(TAccount);
        }
        #endregion

        #region Two Factor Session
        /// <summary>
        /// Instructs cookie auth middleware to add two factor cookie to <see cref="HttpResponse"/>.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task TwoFactorLogInAsync(TAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            ClaimsPrincipal claimsPrincipal = _claimsPrincipalService.
                CreateClaimsPrincipal(account.AccountId, _options.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            await _httpContext.Authentication.SignInAsync(
                _options.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme,
                claimsPrincipal);
        }

        /// <summary>
        /// Gets account using two factor cookie's account id value. 
        /// </summary>
        /// <returns>
        /// Null if two factor cookie is invalid.
        /// Null if two factor cookie does not have an account id value.
        /// An account if two factor cookie exists and has account id claim.
        /// </returns>
        public virtual async Task<TAccount> GetTwoFactorAccountAsync()
        {
            ClaimsPrincipal claimsPrincipal = await _httpContext.
                Authentication.
                AuthenticateAsync(_options.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            if (claimsPrincipal == null)
            {
                return default(TAccount);
            }

            Claim accountIdClaim = claimsPrincipal.
                FindFirst(_options.ClaimsOptions.AccountIdClaimType);

            if (accountIdClaim == null)
            {
                return default(TAccount);
            }

            int accountId = Convert.ToInt32(accountIdClaim.Value);

            return await _accountRepository.GetAsync(accountId, _cancellationToken);
        }

        /// <summary>
        /// Logs off two factor account
        /// </summary>
        public virtual async Task TwoFactorLogOffAsync()
        {
            await _httpContext.Authentication.SignOutAsync(_options.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
        }
        #endregion

        #region Emails
        /// <summary>
        /// Sends alt email verification email to account email.
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task SendAltEmailVerificationEmailAsync(TAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, 
                _options.ConfirmAltEmailTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.AltEmail,
                _options.EmailVerificationEmailSubject,
                string.Format(_options.EmailVerificationEmailMessage, 
                    _options.EmailVerificationLinkDomain,
                    WebUtility.UrlEncode(token)));

            await _emailService.SendEmailAsync(mimeMessage);
        }

        /// <summary>
        /// Sends email verification email to account email.
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task SendEmailVerificationEmailAsync(TAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, _options.ConfirmEmailTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                _options.EmailVerificationEmailSubject,
                string.Format(_options.EmailVerificationEmailMessage, 
                    _options.EmailVerificationLinkDomain,
                    WebUtility.UrlEncode(token)));

            await _emailService.SendEmailAsync(mimeMessage);
        }

        /// <summary>
        /// Sends reset password email to account email.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task SendResetPasswordEmailAsync(TAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, _options.ResetPasswordTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                _options.ResetPasswordEmailSubject,
                string.Format(_options.ResetPasswordEmailMessage,
                    _options.ResetPasswordLinkDomain,
                    WebUtility.UrlEncode(token),
                    WebUtility.UrlEncode(account.Email)));

            await _emailService.SendEmailAsync(mimeMessage);
        }

        /// <summary>
        /// Sends two factor code email. 
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task SendTwoFactorCodeEmailAsync(TAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            string token = GetToken(TokenServiceOptions.TotpTokenService, _options.TwoFactorTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                _options.TwoFactorCodeEmailSubject,
                string.Format(_options.TwoFactorCodeEmailMessage, 
                    token));

            await _emailService.SendEmailAsync(mimeMessage);
        }
        #endregion
        #endregion
    }
}

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

namespace Jering.Accounts
{
    // TODO there are some repository methods that return true/false (such as UpdateAccountEmailAsync) 
    // the underlying stored procedures should throw exceptions directly instead (right at the source of the issue)
    // to enable handling/debugging at repository and stored procedure levels.

    /// <summary>
    /// Provides an API for managing Account security.
    /// </summary>
    public class AccountsService<TAccount> : IAccountsService<TAccount> where TAccount : IAccount
    {
        private IClaimsPrincipalService<TAccount> _claimsPrincipalService { get; }
        private IAccountRepository<TAccount> _accountRepository { get; }
        private HttpContext _httpContext { get; }
        private AccountsServiceOptions _securityOptions { get; }
        private IEmailService _emailService { get; }
        private IPasswordService _passwordService { get; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, ITokenService<TAccount>> TokenServices { get; } = new Dictionary<string, ITokenService<TAccount>>();

        /// <summary>
        /// 
        /// </summary>
        public string ConfirmEmailTokenPurpose { get; } = "EmailConfirmation";

        /// <summary>
        /// 
        /// </summary>
        public string TwoFactorTokenPurpose { get; } = "TwoFactor";

        /// <summary>
        /// 
        /// </summary>
        public string ResetPasswordTokenPurpose { get; } = "ResetPassword";

        /// <summary>
        /// 
        /// </summary>
        public string ConfirmAltEmailTokenPurpose { get; } = "ConfirmAltEmail";

        /// <summary>
        /// Constructs a new instance of <see cref="AccountsService{TAccount}"/>.
        /// </summary>
        /// <param name="claimsPrincipalService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="emailService"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="passwordService"></param>
        public AccountsService(IClaimsPrincipalService<TAccount> claimsPrincipalService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountsServiceOptions> securityOptionsAccessor,
            IAccountRepository<TAccount> accountRepository,
            IEmailService emailService,
            IServiceProvider serviceProvider,
            IPasswordService passwordService)
        {
            _emailService = emailService;
            _claimsPrincipalService = claimsPrincipalService;
            _httpContext = httpContextAccessor?.HttpContext;
            _securityOptions = securityOptionsAccessor?.Value;
            _accountRepository = accountRepository;
            _passwordService = passwordService;

            if (serviceProvider != null)
            {
                foreach (string tokenServiceName in _securityOptions.TokenServiceOptions.TokenServiceMap.Keys)
                {
                    ITokenService<TAccount> tokenService = (ITokenService<TAccount>)serviceProvider.
                        GetRequiredService(_securityOptions.TokenServiceOptions.TokenServiceMap[tokenServiceName]);
                    RegisterTokenProvider(tokenServiceName, tokenService);
                }
            }
        }

        #region Crypto
        /// <summary>
        /// Adds <paramref name="tokenService"/> to <see cref="TokenServices"/> 
        /// </summary>
        /// <param name="tokenServiceName"></param>
        /// <param name="tokenService"></param>
        public virtual void RegisterTokenProvider(string tokenServiceName, ITokenService<TAccount> tokenService)
        {
            if (tokenServiceName == null)
            {
                throw new ArgumentNullException(nameof(tokenServiceName));
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
            if (tokenService == null)
            {
                throw new ArgumentNullException(nameof(tokenService));
            }
            if (purpose == null)
            {
                throw new ArgumentNullException(nameof(purpose));
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
            if (tokenService == null)
            {
                throw new ArgumentNullException(nameof(tokenService));
            }
            if (purpose == null)
            {
                throw new ArgumentNullException(nameof(purpose));
            }
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return TokenServices[tokenService].ValidateToken(purpose, token, account);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public virtual bool ValidatePassword(TAccount account, string password)
        {
            return _passwordService.ValidatePassword(account.PasswordHash, password);
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
                _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme, 
                authProperties);
            authProperties.AllowRefresh = true;

            _httpContext.User = claimsPrincipal;

            await _httpContext.Authentication.SignInAsync(
                    _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);
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

            bool isPersistent = Convert.ToBoolean(_httpContext.User.FindFirst(_securityOptions.ClaimsOptions.IsPersistenClaimType).Value);

            await _httpContext.Authentication.SignInAsync(
                    _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    _httpContext.User,
                    new AuthenticationProperties { IsPersistent = isPersistent, AllowRefresh = true });
        }

        /// <summary>
        /// Logs off account that sent request. 
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task ApplicationLogOffAsync()
        {
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
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
            return _httpContext.User?.FindFirst(_securityOptions.ClaimsOptions.UsernameClaimType)?.Value;
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

            if (_httpContext.User.Identity?.AuthenticationType == _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme)
            {
                System.Security.Claims.Claim accountIdClaim = _httpContext.User.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);
                if (accountIdClaim != null)
                {
                    return await _accountRepository.GetAccountAsync(Convert.ToInt32(accountIdClaim.Value));
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
                CreateClaimsPrincipal(account.AccountId, _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            await _httpContext.Authentication.SignInAsync(
                _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme,
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
                AuthenticateAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            if (claimsPrincipal == null)
            {
                return default(TAccount);
            }

            System.Security.Claims.Claim accountIdClaim = claimsPrincipal.
                FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);

            if (accountIdClaim == null)
            {
                return default(TAccount);
            }

            int accountId = Convert.ToInt32(accountIdClaim.Value);

            return await _accountRepository.GetAccountAsync(accountId);
        }

        /// <summary>
        /// Logs off two factor account
        /// </summary>
        public virtual async Task TwoFactorLogOffAsync()
        {
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
        }
        #endregion

        #region Account Management
        // TODO add tests
        /// <summary>
        /// Creates an account with the credentials <paramref name="email"/> and <paramref name="password"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// <see cref="CreateAccountResult{TAccount}"/> with <see cref="CreateAccountResult{TAccount}.InvalidEmail"/> set to true if <paramref name="email"/> is in use.
        /// <see cref="CreateAccountResult{TAccount}"/> with <see cref="CreateAccountResult{TAccount}.Succeeded"/> set to true if accountis created successfully. 
        /// </returns>
        /// <exception cref="NullReferenceException">Throws exception if account creation fails unexpectedly</exception>
        public virtual async Task<CreateAccountResult<TAccount>> CreateAccountAsync(string email, string password)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            try
            {
                TAccount account = await _accountRepository.CreateAccountAsync(email, _passwordService.HashPassword(password));

                if (account == null)
                {
                    throw new NullReferenceException(nameof(account));
                }

                return CreateAccountResult<TAccount>.GetSucceededResult(account);
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return CreateAccountResult<TAccount>.GetInvalidEmailResult();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Sets password of <paramref name="account"/> to <paramref name="newPassword"/>.
        /// </summary>
        /// <param name="newPassword"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetPasswordResult"/> with <see cref="SetPasswordResult.Succeeded"/> set to true if 
        /// password change succeeds.
        /// <see cref="SetPasswordResult"/> with <see cref="SetPasswordResult.AlreadySet"/> set to true if
        /// account password is already equal to <paramref name="newPassword"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<SetPasswordResult> SetPasswordAsync(TAccount account, string newPassword)
        {
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (newPassword == null)
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            if(_passwordService.ValidatePassword(account.PasswordHash, newPassword))
            {
                return SetPasswordResult.GetAlreadySetResult();
            }

            if (!await _accountRepository.
                UpdateAccountPasswordHashAsync(account.AccountId, 
                _passwordService.HashPassword(newPassword)))
            {
                throw new Exception();
            }

            return SetPasswordResult.GetSucceededResult();
        }

        /// <summary>
        /// Sets email of <paramref name="account"/> to <paramref name="newEmail"/>.
        /// </summary>
        /// <param name="newEmail"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetEmailResult"/> with <see cref="SetEmailResult.Succeeded"/> set to true if 
        /// email change succeeds.
        /// <see cref="SetEmailResult"/> with <see cref="SetEmailResult.InvalidNewEmail"/> set to true if
        /// <paramref name="newEmail"/> is in use.
        /// <see cref="SetEmailResult"/> with <see cref="SetEmailResult.GetAlreadySetResult"/> set to true if
        /// account email is already equal to <paramref name="newEmail"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<SetEmailResult> SetEmailAsync(TAccount account, string newEmail)
        {
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (newEmail == null)
            {
                throw new ArgumentNullException(nameof(newEmail));
            }

            if(account.Email == newEmail)
            {
                return SetEmailResult.GetAlreadySetResult();
            }

            try
            {
                if (!await _accountRepository.UpdateAccountEmailAsync(account.AccountId, newEmail))
                {// TODO the underlying stored procedure should throw a sql exception if update fails
                    throw new Exception();
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return SetEmailResult.GetInvalidNewEmailResult();
                }

                throw;
            }

            return SetEmailResult.GetSucceededResult();
        }

        /// <summary>
        /// Sets alternative email of <paramref name="account"/> to <paramref name="newAltEmail"/>.
        /// </summary>
        /// <param name="newAltEmail"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetAltEmailResult"/> with <see cref="SetAltEmailResult.Succeeded"/> set to true if 
        /// alternative email change succeeds.
        /// <see cref="SetAltEmailResult"/> with <see cref="SetAltEmailResult.InvalidNewAltEmail"/> set to true if
        /// <paramref name="newAltEmail"/> is in use.
        /// <see cref="SetAltEmailResult"/> with <see cref="SetAltEmailResult.GetAlreadySetResult"/> set to true if
        /// account alternative email is already equal to <paramref name="newAltEmail"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<SetAltEmailResult> SetAltEmailAsync(TAccount account, string newAltEmail)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (newAltEmail == null)
            {
                throw new ArgumentNullException(nameof(newAltEmail));
            }

            if(account.AltEmail == newAltEmail)
            {
                return SetAltEmailResult.GetAlreadySetResult();
            }

            try
            {
                if (!await _accountRepository.UpdateAccountAltEmailAsync(account.AccountId, newAltEmail))
                {// TODO the underlying stored procedure should throw a sql exception if update fails
                    throw new Exception();
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return SetAltEmailResult.GetInvalidNewAltEmailResult();
                }

                throw;
            }

            return SetAltEmailResult.GetSucceededResult();
        }

        /// <summary>
        /// Sets display name of <paramref name="account"/> to <paramref name="newDisplayName"/>.
        /// </summary>
        /// <param name="newDisplayName"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetDisplayNameResult"/> with <see cref="SetDisplayNameResult.Succeeded"/> set to true if 
        /// display name change succeeds.
        /// <see cref="SetDisplayNameResult"/> with <see cref="SetDisplayNameResult.InvalidNewDisplayName"/> set to true if
        /// <paramref name="newDisplayName"/> is in use.
        /// <see cref="SetDisplayNameResult"/> with <see cref="SetDisplayNameResult.GetAlreadySetResult"/> set to true if
        /// account display name is already equal to <paramref name="newDisplayName"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<SetDisplayNameResult> SetDisplayNameAsync(TAccount account, string newDisplayName)
        {
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (newDisplayName == null)
            {
                throw new ArgumentNullException(nameof(newDisplayName));
            }

            if (account.DisplayName == newDisplayName)
            {
                return SetDisplayNameResult.GetAlreadySetResult();
            }

            try
            {
                if (!await _accountRepository.UpdateAccountDisplayNameAsync(account.AccountId, newDisplayName))
                {// TODO the underlying stored procedure should throw a sql exception if update fails
                    throw new Exception();
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return SetDisplayNameResult.GetInvalidNewDisplayNameResult();
                }

                throw;
            }

            return SetDisplayNameResult.GetSucceededResult();
        }

        /// <summary>
        /// Sets two factor enabled of <paramref name="account"/> to <paramref name="enabled"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="enabled"></param>
        /// <returns>
        /// <see cref="SetTwoFactorEnabledResult"/> with <see cref="SetTwoFactorEnabledResult.Succeeded"/> set to true if 
        /// operation succeeds.
        /// <see cref="SetTwoFactorEnabledResult"/> with <see cref="SetTwoFactorEnabledResult.GetEmailUnverifiedResult"/> set to true if
        /// account email is unverified.
        /// <see cref="SetTwoFactorEnabledResult"/> with <see cref="SetTwoFactorEnabledResult.GetAlreadySetResult"/> set to true if
        /// account two factor enabled is already equal to <paramref name="enabled"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<SetTwoFactorEnabledResult> SetTwoFactorEnabledAsync(TAccount account, bool enabled)
        {
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if(account.TwoFactorEnabled == enabled)
            {
                 return SetTwoFactorEnabledResult.GetAlreadySetResult();
            }

            if (enabled && !account.EmailVerified)
            {
                return SetTwoFactorEnabledResult.GetEmailUnverifiedResult();
            }

            if (!await _accountRepository.UpdateAccountTwoFactorEnabledAsync(account.AccountId, enabled))
            {// TODO the underlying stored procedure should throw a sql exception if update fails
                throw new Exception();
            }

            return SetTwoFactorEnabledResult.GetSucceededResult();
        }

        /// <summary>
        /// Sets email verified of <paramref name="account"/> to <paramref name="verified"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="verified"></param>
        /// <returns>
        /// <see cref="SetEmailVerifiedResult"/> with <see cref="SetEmailVerifiedResult.Succeeded"/> set to true if 
        /// operation succeeds.
        /// <see cref="SetEmailVerifiedResult"/> with <see cref="SetEmailVerifiedResult.GetAlreadySetResult"/> set to true if
        /// account email verified is already equal to <paramref name="verified"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<SetEmailVerifiedResult> SetEmailVerifiedAsync(TAccount account, bool verified)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (account.EmailVerified == verified)
            {
                return SetEmailVerifiedResult.GetAlreadySetResult();
            }

            if (!await _accountRepository.UpdateAccountEmailVerifiedAsync(account.AccountId, verified))
            {// TODO the underlying stored procedure should throw a sql exception if update fails
                throw new Exception();
            }

            return SetEmailVerifiedResult.GetSucceededResult();
        }

        /// <summary>
        /// Sets alternative email verified of <paramref name="account"/> to <paramref name="verified"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="verified"></param>
        /// <returns>
        /// <see cref="SetAltEmailVerifiedResult"/> with <see cref="SetAltEmailVerifiedResult.Succeeded"/> set to true if 
        /// operation succeeds.
        /// <see cref="SetAltEmailVerifiedResult"/> with <see cref="SetAltEmailVerifiedResult.GetAlreadySetResult"/> set to true if
        /// account alternative email verified is already equal to <paramref name="verified"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<SetAltEmailVerifiedResult> SetAltEmailVerifiedAsync(TAccount account, 
            bool verified)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (account.AltEmailVerified == verified)
            {
                return SetAltEmailVerifiedResult.GetAlreadySetResult();
            }

            if (!await _accountRepository.UpdateAccountAltEmailVerifiedAsync(account.AccountId, verified))
            {// TODO the underlying stored procedure should throw a sql exception if update fails
                throw new Exception();
            }

            return SetAltEmailVerifiedResult.GetSucceededResult();
        }

        /// <summary>
        /// Sends reset password email. Inserts <paramref name="linkDomain"/>, token and account email into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        /// <param name="email"></param>
        public virtual async Task SendResetPasswordEmailAsync(TAccount account, string email, string subject,
            string messageFormat, string linkDomain)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (messageFormat == null)
            {
                throw new ArgumentNullException(nameof(messageFormat));
            }
            if (linkDomain == null)
            {
                throw new ArgumentNullException(nameof(linkDomain));
            }
            if(email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, ResetPasswordTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(email,
                subject,
                string.Format(messageFormat,
                linkDomain,
                WebUtility.UrlEncode(token),
                WebUtility.UrlEncode(email)));

            await _emailService.SendEmailAsync(mimeMessage);
        }

        /// <summary>
        /// Sends email verification email. Inserts <paramref name="linkDomain"/>, token and account id into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        public virtual async Task SendEmailVerificationEmailAsync(TAccount account, string subject,
            string messageFormat, string linkDomain)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (messageFormat == null)
            {
                throw new ArgumentNullException(nameof(messageFormat));
            }
            if (linkDomain == null)
            {
                throw new ArgumentNullException(nameof(linkDomain));
            }

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, ConfirmEmailTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                subject,
                string.Format(messageFormat, linkDomain, token, account.AccountId));

            await _emailService.SendEmailAsync(mimeMessage);
        }

        /// <summary>
        /// Sends alternative email verification email. Inserts <paramref name="linkDomain"/>, token and account id into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        /// <returns>
        /// <see cref="SendAltEmailVerificationEmailResult"/> with <see cref="SendAltEmailVerificationEmailResult.Succeeded"/> set to true if 
        /// email is sent successfully.
        /// <see cref="SendAltEmailVerificationEmailResult"/> with <see cref="SendAltEmailVerificationEmailResult.InvalidAltEmail"/> set to true if 
        /// account alt email is null or empty. 
        /// </returns>
        public virtual async Task<SendAltEmailVerificationEmailResult> SendAltEmailVerificationEmailAsync(TAccount account, 
            string subject, string messageFormat, string linkDomain)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (messageFormat == null)
            {
                throw new ArgumentNullException(nameof(messageFormat));
            }
            if (linkDomain == null)
            {
                throw new ArgumentNullException(nameof(linkDomain));
            }

            if (string.IsNullOrEmpty(account.AltEmail))
            {
                return SendAltEmailVerificationEmailResult.GetInvalidAltEmailResult();
            }

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, ConfirmAltEmailTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.AltEmail,
                subject,
                string.Format(messageFormat, linkDomain, token, account.AccountId));

            await _emailService.SendEmailAsync(mimeMessage);

            return SendAltEmailVerificationEmailResult.GetSucceededResult();
        }

        /// <summary>
        /// Sends two factor code email. Inserts token into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        public virtual async Task SendTwoFactorCodeEmailAsync(TAccount account, string subject, string messageFormat)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (messageFormat == null)
            {
                throw new ArgumentNullException(nameof(messageFormat));
            }

            string token = GetToken(TokenServiceOptions.TotpTokenService, TwoFactorTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                subject,
                string.Format(messageFormat, token));

            await _emailService.SendEmailAsync(mimeMessage);
        }
        #endregion
    }
}
 
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
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

namespace Jering.AccountManagement.Security
{
    // TODO there are some repository methods that return true/false (such as UpdateAccountEmailAsync) 
    // the underlying stored procedures should throw exceptions directly instead (right at the source of the issue)
    // to enable handling/debugging at repository and stored procedure levels.

    /// <summary>
    /// Provides an API for managing Account security.
    /// </summary>
    public class AccountSecurityService<TAccount> : IAccountSecurityService<TAccount> where TAccount : IAccount
    {
        private ClaimsPrincipalService<TAccount> _claimsPrincipalService { get; }
        private IAccountRepository<TAccount> _accountRepository { get; }
        private HttpContext _httpContext { get; }
        private AccountSecurityOptions _securityOptions { get; }
        private IEmailService _emailService { get; }

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
        public string ConfirmAlternativeEmailTokenPurpose { get; } = "ConfirmAlternativeEmail";

        /// <summary>
        /// Constructs a new instance of <see cref="AccountSecurityService{TAccount}"/>.
        /// </summary>
        /// <param name="claimsPrincipalService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="emailService"></param>
        /// <param name="serviceProvider"></param>
        public AccountSecurityService(ClaimsPrincipalService<TAccount> claimsPrincipalService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountSecurityOptions> securityOptionsAccessor,
            IAccountRepository<TAccount> accountRepository,
            IEmailService emailService,
            IServiceProvider serviceProvider)
        {
            _emailService = emailService;
            _claimsPrincipalService = claimsPrincipalService;
            _httpContext = httpContextAccessor?.HttpContext;
            _securityOptions = securityOptionsAccessor?.Value;
            _accountRepository = accountRepository;

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
        /// Logs in <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        public virtual async Task LogInAsync(TAccount account, AuthenticationProperties authenticationProperties)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalService.CreateClaimsPrincipalAsync(account, _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme, authenticationProperties);
            authenticationProperties.AllowRefresh = true;

            await _httpContext.Authentication.SignInAsync(
                    _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    claimsPrincipal,
                    authenticationProperties);
        }

        /// <summary>
        /// Refreshes log in for <paramref name="account"/>.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task RefreshLogInAsync(TAccount account)
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
        /// Logs in account with credentials <paramref name="email"/> and <paramref name="password"/> using 
        /// specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>
        /// <see cref="PasswordLogInResult{TAccount}"/> with <see cref="PasswordLogInResult{TAccount}.InvalidCredentials"/> set to true if credentials are invalid. 
        /// <see cref="PasswordLogInResult{TAccount}"/> with <see cref="PasswordLogInResult{TAccount}.TwoFactorRequired"/> set to true if two factor is required. 
        /// <see cref="PasswordLogInResult{TAccount}"/> with <see cref="PasswordLogInResult{TAccount}.Succeeded"/> set to true if application sign in is complete. 
        /// </returns>
        public virtual async Task<PasswordLogInResult<TAccount>> PasswordLogInAsync(string email, string password,
            AuthenticationProperties authenticationProperties)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            TAccount account = await _accountRepository.GetAccountByEmailAndPasswordAsync(email, password);
            if (account != null)
            {
                if (account.TwoFactorEnabled)
                {
                    return PasswordLogInResult<TAccount>.GetTwoFactorRequiredResult(account);
                }

                await LogInAsync(account, authenticationProperties);
                return PasswordLogInResult<TAccount>.GetSucceededResult();
            }

            return PasswordLogInResult<TAccount>.GetInvalidCredentialsResult();
        }

        /// <summary>
        /// Logs off account that sent request. 
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task LogOffAsync()
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
        public virtual string GetLoggedInAccountEmail()
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
        public virtual async Task<TAccount> GetLoggedInAccountAsync()
        {
            if (_httpContext.User == null)
            {
                return default(TAccount);
            }

            return await GetLoggedInAccountAsync(_httpContext.User);
        }

        /// <summary>
        /// Gets logged in account for <param name="claimsPrincipal"></param>. This overload must be used if <see cref="HttpContext.User"/> 
        /// has not been set, for example before authentication is complete.
        /// </summary>
        /// <returns>
        /// An account if there is a logged in account.
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetLoggedInAccountAsync(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (claimsPrincipal.Identity?.AuthenticationType == _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme)
            {
                System.Security.Claims.Claim accountIdClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);
                if (accountIdClaim != null)
                {
                    return await _accountRepository.GetAccountAsync(Convert.ToInt32(accountIdClaim.Value));
                }
            }

            return default(TAccount);
        }

        /// <summary>
        /// Validates <paramref name="token"/>. If valid, sets EmailConfirmed to true for the associated account.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <see cref="ConfirmEmailResult"/> with <see cref="ConfirmEmailResult.ExpiredToken"/> set to true if token has expired.
        /// <see cref="ConfirmEmailResult"/> with <see cref="ConfirmEmailResult.NotLoggedIn"/> set to true if there is not logged in account.
        /// <see cref="ConfirmEmailResult"/> with <see cref="ConfirmEmailResult.InvalidToken"/> set to true if <paramref name="token"/> is invalid. 
        /// <see cref="ConfirmEmailResult"/> with <see cref="ConfirmEmailResult.Succeeded"/> set to true if <paramref name="token"/> is valid and database update succeeds. 
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails</exception>
        public virtual async Task<ConfirmEmailResult> ConfirmEmailAsync(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            TAccount account = await GetLoggedInAccountAsync();

            if (account == null)
            {
                return ConfirmEmailResult.GetNotLoggedInResult();
            }

            ValidateTokenResult result = TokenServices[TokenServiceOptions.DataProtectionTokenService].
               ValidateToken(ConfirmEmailTokenPurpose, token, account);

            if (result.Invalid)
            {
                return ConfirmEmailResult.GetInvalidTokenResult();
            }

            if (result.Expired)
            {
                return ConfirmEmailResult.GetExpiredTokenResult();
            }

            if (!await _accountRepository.UpdateAccountEmailVerifiedAsync(account.AccountId, true))
            {
                throw new Exception();
            }

            return ConfirmEmailResult.GetSucceededResult();
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
        /// Instructs cookie authentication middleware to add two factor cookie to <see cref="HttpResponse"/>.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task CreateTwoFactorCookieAsync(TAccount account)
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
        /// Validates <paramref name="token"/>. If valid, peforms application log in for user specified by two factor cookie.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPersistent"></param>
        /// <returns>
        /// <see cref="TwoFactorLogInResult{TAccount}"/> with <see cref="TwoFactorLogInResult{TAccount}.InvalidToken"/> set to true if <paramref name="token"/> is invalid.
        /// <see cref="TwoFactorLogInResult{TAccount}"/> with <see cref="TwoFactorLogInResult{TAccount}.Succeeded"/> set to true if <paramref name="token"/> is valid. 
        /// <see cref="TwoFactorLogInResult{TAccount}"/> with <see cref="TwoFactorLogInResult{TAccount}.NotLoggedIn"/> set to true if unable to retrieve two factor account.
        /// </returns>
        public virtual async Task<TwoFactorLogInResult<TAccount>> TwoFactorLogInAsync(string token, bool isPersistent)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            TAccount account = await GetTwoFactorAccountAsync();
            if (account == null)
            {
                return TwoFactorLogInResult<TAccount>.GetNotLoggedInResult();
            }

            ValidateTokenResult result = TokenServices[TokenServiceOptions.TotpTokenService].
                    ValidateToken(TwoFactorTokenPurpose, token, account);

            if (result.Valid)
            {
                // Cleanup two factor cookie
                await _httpContext.Authentication.
                    SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
                await LogInAsync(account, new AuthenticationProperties() { IsPersistent = isPersistent });

                return TwoFactorLogInResult<TAccount>.GetSucceededResult(account);
            }

            return TwoFactorLogInResult<TAccount>.GetInvalidTokenResult();
        }

        /// <summary>
        /// Creates an account with the credentials <paramref name="email"/> and <paramref name="password"/>. If successful, logs in to account.
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
                TAccount account = await _accountRepository.CreateAccountAsync(email, password);

                if (account == null)
                {
                    throw new NullReferenceException(nameof(account));
                }

                await LogInAsync(account, new AuthenticationProperties { IsPersistent = true });

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
        /// Sets password of account with email <paramref name="email"/> to <paramref name="newPassword"/> if <paramref name="token"/> is valid.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <param name="newPassword"></param>
        /// <returns>
        /// <see cref="ResetPasswordResult"/> with <see cref="ResetPasswordResult.InvalidEmail"/> set to true if 
        /// <paramref name="email"/> is not associated with any account.
        /// <see cref="ResetPasswordResult"/> with <see cref="ResetPasswordResult.Succeeded"/> set to true if 
        /// password reset succeeds.
        /// <see cref="ResetPasswordResult"/> with <see cref="ResetPasswordResult.InvalidToken"/> set to true if
        /// <paramref name="token"/> is invalid.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<ResetPasswordResult> ResetPasswordAsync(string token, string email, string newPassword)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }
            if (newPassword == null)
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            TAccount account = await _accountRepository.GetAccountByEmailOrAlternativeEmailAsync(email);
            if (account == null)
            {
                return ResetPasswordResult.GetInvalidEmailResult();
            }

            ValidateTokenResult result = ValidateToken(TokenServiceOptions.DataProtectionTokenService,
                ResetPasswordTokenPurpose,
                account,
                token);

            if (result.Invalid)
            {
                return ResetPasswordResult.GetInvalidTokenResult();
            }

            if(result.Expired)
            {
                return ResetPasswordResult.GetExpiredTokenResult();
            }

            if (!await _accountRepository.UpdateAccountPasswordHashAsync(account.AccountId, newPassword))
            {
                throw new Exception();
            }

            return ResetPasswordResult.GetSucceededResult();
        }

        /// <summary>
        /// Sets email of account with id <paramref name="accountId"/> to <paramref name="newEmail"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="newEmail"></param>
        /// <returns>
        /// <see cref="UpdateEmailResult"/> with <see cref="UpdateEmailResult.Succeeded"/> set to true if update succeeds.
        /// <see cref="UpdateEmailResult"/> with <see cref="UpdateEmailResult.InvalidEmail"/> set to true if new email is in use.
        /// </returns>
        /// <exception cref="Exception">Thrown if account email update fails unexpectedly</exception>
        public virtual async Task<UpdateEmailResult> UpdateEmailAsync(int accountId, string newEmail)
        {
            if(newEmail == null)
            {
                throw new ArgumentNullException(nameof(newEmail));
            }

            try
            {
                if (!await _accountRepository.UpdateAccountEmailAsync(accountId, newEmail))
                {
                    // TODO the underlying stored procedure should throw a sql exception if update fails
                    throw new Exception();
                }

                return UpdateEmailResult.GetSucceededResult();
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return UpdateEmailResult.GetInvalidEmailResult();
                }

                throw;
            }
        }

        /// <summary>
        /// Sets password hash of account with id <paramref name="accountId"/> using <paramref name="newPassword"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="newPassword"></param>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task UpdatePasswordHashAsync(int accountId, string newPassword)
        {
            if(newPassword == null)
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            if (!await _accountRepository.UpdateAccountPasswordHashAsync(accountId, newPassword))
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Sets AlternativeEmail of account with id <paramref name="accountId"/> to <paramref name="newAlternativeEmail"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="newAlternativeEmail"></param>
        /// <returns>
        /// <see cref="UpdateAlternativeEmailResult"/> with <see cref="UpdateAlternativeEmailResult.Succeeded"/> set to true if update succeeds.
        /// <see cref="UpdateAlternativeEmailResult"/> with <see cref="UpdateAlternativeEmailResult.InvalidAlternativeEmail"/> set to true if alternative email is in use.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<UpdateAlternativeEmailResult> UpdateAlternativeEmailAsync(int accountId, 
            string newAlternativeEmail)
        {
            if(newAlternativeEmail == null)
            {
                throw new ArgumentNullException(nameof(newAlternativeEmail));
            }

            try
            {
                if (!await _accountRepository.UpdateAccountAlternativeEmailAsync(accountId, newAlternativeEmail))
                {
                    throw new Exception();
                }

                return UpdateAlternativeEmailResult.GetSucceededResult();
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return UpdateAlternativeEmailResult.GetInvalidAlternativeEmailResult();
                }

                throw;
            }
        }

        /// <summary>
        /// Sets DisplayName of account with id <paramref name="accountId"/> to <paramref name="newDisplayName"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="newDisplayName"></param>
        /// <returns>
        /// <see cref="UpdateDisplayNameResult"/> with <see cref="UpdateDisplayNameResult.Succeeded"/> set to true if update succeeds.
        /// <see cref="UpdateDisplayNameResult"/> with <see cref="UpdateDisplayNameResult.InvalidDisplayName"/> set to true if display name is in use.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task<UpdateDisplayNameResult> UpdateDisplayNameAsync(int accountId, 
            string newDisplayName)
        {
            if(newDisplayName == null)
            {
                throw new ArgumentNullException(nameof(newDisplayName));
            }

            try
            {
                if (!await _accountRepository.UpdateAccountDisplayNameAsync(accountId, newDisplayName))
                {
                    throw new Exception();
                }

                return UpdateDisplayNameResult.GetSucceededResult();
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return UpdateDisplayNameResult.GetInvalidDisplayNameResult();
                }

                throw;
            }
        }

        /// <summary>
        /// Sets TwoFactorEnabled of account with id <paramref name="accountId"/> to <paramref name="twoFactorEnabled"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="twoFactorEnabled"></param>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        public virtual async Task UpdateTwoFactorEnabledAsync(int accountId, bool twoFactorEnabled)
        {
            if (!await _accountRepository.UpdateAccountTwoFactorEnabledAsync(accountId, twoFactorEnabled))
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Sends reset password email. Inserts <paramref name="linkDomain"/>, token and account email into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        public virtual async Task SendResetPasswordEmailAsync(TAccount account, string subject, 
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

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, ResetPasswordTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                subject,
                string.Format(messageFormat, 
                linkDomain, 
                WebUtility.UrlEncode(token),
                WebUtility.UrlEncode(account.Email)));

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
        public virtual async Task SendAlternativeEmailVerificationEmailAsync(TAccount account, string subject, 
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

            string token = GetToken(TokenServiceOptions.DataProtectionTokenService, ConfirmAlternativeEmailTokenPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                subject,
                string.Format(messageFormat, linkDomain, token, account.AccountId));

            await _emailService.SendEmailAsync(mimeMessage);
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
    }
}

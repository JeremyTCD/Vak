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

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an API for managing Account security.
    /// </summary>
    public class AccountSecurityService<TAccount> : IAccountSecurityService<TAccount> where TAccount : IAccount
    {
        private ClaimsPrincipalService<TAccount> _claimsPrincipalService { get; }
        private IAccountRepository<TAccount> _accountRepository { get; }
        private HttpContext _httpContext { get; }
        private AccountSecurityOptions _securityOptions { get; }
        private Dictionary<string, ITokenService<TAccount>> _tokenServices { get; } = new Dictionary<string, ITokenService<TAccount>>();

        /// <summary>
        /// The data protection purpose used for email confirmation related methods.
        /// </summary>
        protected const string _confirmEmailTokenPurpose = "EmailConfirmation";
        /// <summary>
        /// 
        /// </summary>
        protected const string _twoFactorTokenPurpose = "TwoFactor";

        /// <summary>
        /// Constructs a new instance of <see cref="AccountSecurityService{TAccount}"/>.
        /// </summary>
        /// <param name="claimsPrincipalService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="serviceProvider"></param>
        public AccountSecurityService(ClaimsPrincipalService<TAccount> claimsPrincipalService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountSecurityOptions> securityOptionsAccessor,
            IAccountRepository<TAccount> accountRepository,
            IServiceProvider serviceProvider)
        {
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
        /// 
        /// </summary>
        /// <param name="tokenServiceName"></param>
        /// <param name="tokenService"></param>
        public virtual void RegisterTokenProvider(string tokenServiceName, ITokenService<TAccount> tokenService)
        {
            if (tokenService == null)
            {
                throw new ArgumentNullException(nameof(tokenService));
            }
            _tokenServices[tokenServiceName] = tokenService;
        }

        /// <summary>
        /// Logs in specified <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns></returns>
        public virtual async Task LogInAsync(TAccount account, AuthenticationProperties authenticationProperties)
        {
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
        /// <returns></returns>
        public virtual async Task RefreshLogInAsync(TAccount account)
        {
            _claimsPrincipalService.UpdateClaimsPrincipal(account, _httpContext.User);

            bool isPersistent = Convert.ToBoolean(_httpContext.User.FindFirst(_securityOptions.ClaimsOptions.IsPersistenClaimType).Value);

            await _httpContext.Authentication.SignInAsync(
                    _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    _httpContext.User,
                    new AuthenticationProperties { IsPersistent = isPersistent, AllowRefresh = true });
        }

        /// <summary>
        /// Logs in account with specified <paramref name="email"/> and <paramref name="password"/> using 
        /// specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>
        /// <see cref="PasswordLogInResult{TAccount}"/> with <see cref="PasswordLogInResult{TAccount}.Failed"/> set to true if credentials are invalid. 
        /// <see cref="PasswordLogInResult{TAccount}"/> with <see cref="PasswordLogInResult{TAccount}.TwoFactorRequired"/> set to true if two factor is required. 
        /// <see cref="PasswordLogInResult{TAccount}"/> with <see cref="PasswordLogInResult{TAccount}.Succeeded"/> set to true if application sign in is complete. 
        /// </returns>
        public virtual async Task<PasswordLogInResult<TAccount>> PasswordLogInAsync(string email, string password, AuthenticationProperties authenticationProperties)
        {
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

            return PasswordLogInResult<TAccount>.GetFailedResult();
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
        /// Email account if it exists, null otherwise.
        /// </returns>
        public virtual string GetLoggedInAccountEmail()
        {
            return _httpContext.User.FindFirst(_securityOptions.ClaimsOptions.UsernameClaimType)?.Value;
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
            if (claimsPrincipal?.Identity?.AuthenticationType == _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme)
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
        /// <see cref="ConfirmEmailResult.Failed"/> if there is no logged in account.
        /// <see cref="ConfirmEmailResult.InvalidToken"/> if token is invalid.
        /// <see cref="ConfirmEmailResult.Failed"/> if unable to update account email confirmed. 
        /// <see cref="ConfirmEmailResult.Succeeded"/> if <paramref name="token"/> is valid and EmailConfirmed updates successfully.
        /// </returns>
        public virtual async Task<ConfirmEmailResult> ConfirmEmailAsync(string token)
        {
            TAccount account = await GetLoggedInAccountAsync();

            if (account == null)
            {
                return ConfirmEmailResult.Failed;
            }

            if (!await _tokenServices[TokenServiceOptions.DataProtectionTokenService].ValidateTokenAsync(_confirmEmailTokenPurpose, token, account))
            {
                return ConfirmEmailResult.InvalidToken;
            }

            if (!await _accountRepository.UpdateAccountEmailVerifiedAsync(account.AccountId, true))
            {
                return ConfirmEmailResult.Failed;
            }

            return ConfirmEmailResult.Succeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public virtual async Task<string> GetTokenAsync(string tokenService, string purpose, TAccount account)
        {
            return await _tokenServices[tokenService].GenerateTokenAsync(purpose, account);
        }

        /// <summary>
        /// Validates <paramref name="token"/> created by <paramref name="tokenService"/>.
        /// 
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <param name="token"></param>
        /// <returns>
        /// True if token is valid, false otherwise.
        /// </returns>
        public virtual async Task<bool> ValidateTokenAsync(string tokenService, string purpose, TAccount account, string token)
        {
            return await _tokenServices[tokenService].ValidateTokenAsync(purpose, token, account);
        }

        /// <summary>
        /// Gets account using two factor cookie's account Id value. 
        /// </summary>
        /// <returns>
        /// Null if two factor cookie is invalid.
        /// Null if two factor cookie does not have an account Id value.
        /// An account if two factor cookie's account Id value exists.
        /// </returns>
        public virtual async Task<TAccount> GetTwoFactorAccountAsync()
        {
            ClaimsPrincipal claimsPrincipal = await _httpContext.Authentication.AuthenticateAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            if (claimsPrincipal == null)
            {
                return default(TAccount);
            }

            System.Security.Claims.Claim accountIdClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);

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
            ClaimsPrincipal claimsPrincipal = _claimsPrincipalService.CreateClaimsPrincipal(account.AccountId, _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            await _httpContext.Authentication.SignInAsync(
                _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme,
                claimsPrincipal);
        }

        /// <summary>
        /// Validates two factor token. If valid, peforms application log in for user specified by two factor cookie.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPersistent"></param>
        /// <returns>
        /// <see cref="TwoFactorLogInResult{TAccount}"/> with <see cref="TwoFactorLogInResult{TAccount}.Failed"/> set to true if unable to retrieve two factor account 
        /// or <paramref name="token"/> is invalid.
        /// <see cref="TwoFactorLogInResult{TAccount}"/> with <see cref="TwoFactorLogInResult{TAccount}.Succeeded"/> set to true if <paramref name="token"/> is valid. 
        /// </returns>
        public virtual async Task<TwoFactorLogInResult<TAccount>> TwoFactorLogInAsync(string token, bool isPersistent)
        {
            TAccount account = await GetTwoFactorAccountAsync();
            if (account != null)
            {
                if (await _tokenServices[TokenServiceOptions.TotpTokenService].ValidateTokenAsync(_twoFactorTokenPurpose, token, account))
                {
                    // Cleanup two factor cookie
                    await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
                    await LogInAsync(account, new AuthenticationProperties() { IsPersistent = isPersistent });

                    return TwoFactorLogInResult<TAccount>.GetSucceededResult(account);
                }
            }

            return TwoFactorLogInResult<TAccount>.GetFailedResult();
        }

        /// <summary>
        /// Creates an account with the specified <paramref name="email"/> and <paramref name="password"/>. If successful, logs in account and sends
        /// confirmation email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// <see cref="CreateAccountResult{TAccount}"/>. 
        /// </returns>
        public virtual async Task<CreateAccountResult<TAccount>> CreateAccountAsync(string email, string password)
        {
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
        /// Sets email of account with id <paramref name="accountId"/> to <paramref name="newEmail"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="newEmail"></param>
        /// <returns>
        /// <see cref="UpdateAccountEmailResult"/> with <see cref="UpdateAccountEmailResult.Failed"/> set to true if update fails unexpectedly.
        /// <see cref="UpdateAccountEmailResult"/> with <see cref="UpdateAccountEmailResult.Succeeded"/> set to true if update succeeds.
        /// <see cref="UpdateAccountEmailResult"/> with <see cref="UpdateAccountEmailResult.EmailInUse"/> set to true if new email is already in use.
        /// </returns>
        public virtual async Task<UpdateAccountEmailResult> UpdateAccountEmailAsync(int accountId, string newEmail)
        {
            try
            {
                if (!await _accountRepository.UpdateAccountEmailAsync(accountId, newEmail))
                {
                    return UpdateAccountEmailResult.GetFailedResult();
                }
                else
                {
                    return UpdateAccountEmailResult.GetSucceededResult();
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return UpdateAccountEmailResult.GetEmailInUseResult();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Sets password hash of account with id <paramref name="accountId"/> using <paramref name="newPassword"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="newPassword"></param>
        /// <returns>
        /// <see cref="UpdateAccountPasswordHashResult"/> with <see cref="UpdateAccountPasswordHashResult.Failed"/> set to true if update fails unexpectedly.
        /// <see cref="UpdateAccountPasswordHashResult"/> with <see cref="UpdateAccountPasswordHashResult.Succeeded"/> set to true if update succeeds.
        /// </returns>
        public virtual async Task<UpdateAccountPasswordHashResult> UpdateAccountPasswordHashAsync(int accountId, string newPassword)
        {
            if (!await _accountRepository.UpdateAccountPasswordHashAsync(accountId, newPassword))
            {
                return UpdateAccountPasswordHashResult.GetFailedResult();
            }
            else
            {
                return UpdateAccountPasswordHashResult.GetSucceededResult();
            }
        }

        /// <summary>
        /// Sets AlternativeEmail of account with id <paramref name="accountId"/> to <paramref name="alternativeEmail"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="alternativeEmail"></param>
        /// <returns>
        /// <see cref="UpdateAccountAlternativeEmailResult"/> with <see cref="UpdateAccountAlternativeEmailResult.Failed"/> set to true if update fails unexpectedly.
        /// <see cref="UpdateAccountAlternativeEmailResult"/> with <see cref="UpdateAccountAlternativeEmailResult.Succeeded"/> set to true if update succeeds.
        /// <see cref="UpdateAccountAlternativeEmailResult"/> with <see cref="UpdateAccountAlternativeEmailResult.AlternativeEmailInUse"/> set to true if alternative email is in use.
        /// </returns>
        public virtual async Task<UpdateAccountAlternativeEmailResult> UpdateAccountAlternativeEmailAsync(int accountId, string alternativeEmail)
        {
            try
            {
                if (!await _accountRepository.UpdateAccountAlternativeEmailAsync(accountId, alternativeEmail))
                {
                    return UpdateAccountAlternativeEmailResult.GetFailedResult();
                }
                else
                {
                    return UpdateAccountAlternativeEmailResult.GetSucceededResult();
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return UpdateAccountAlternativeEmailResult.GetAlternativeEmailInUseResult();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Sets DisplayName of account with id <paramref name="accountId"/> to <paramref name="displayName"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="displayName"></param>
        /// <returns>
        /// <see cref="UpdateAccountDisplayNameResult"/> with <see cref="UpdateAccountDisplayNameResult.Failed"/> set to true if update fails unexpectedly.
        /// <see cref="UpdateAccountDisplayNameResult"/> with <see cref="UpdateAccountDisplayNameResult.Succeeded"/> set to true if update succeeds.
        /// <see cref="UpdateAccountDisplayNameResult"/> with <see cref="UpdateAccountDisplayNameResult.DisplayNameInUse"/> set to true if display name is in use.
        /// </returns>
        public virtual async Task<UpdateAccountDisplayNameResult> UpdateAccountDisplayNameAsync(int accountId, string displayName)
        {
            try
            {
                if (!await _accountRepository.UpdateAccountDisplayNameAsync(accountId, displayName))
                {
                    return UpdateAccountDisplayNameResult.GetFailedResult();
                }
                else
                {
                    return UpdateAccountDisplayNameResult.GetSucceededResult();
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return UpdateAccountDisplayNameResult.GetDisplayNameInUseResult();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Sets TwoFactorEnabled of account with id <paramref name="accountId"/> to <paramref name="twoFactorEnabled"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="twoFactorEnabled"></param>
        /// <returns>
        /// <see cref="UpdateAccountTwoFactorEnabledResult"/> with <see cref="UpdateAccountTwoFactorEnabledResult.Failed"/> set to true if update fails unexpectedly.
        /// <see cref="UpdateAccountTwoFactorEnabledResult"/> with <see cref="UpdateAccountTwoFactorEnabledResult.Succeeded"/> set to true if update succeeds.
        /// </returns>
        public virtual async Task<UpdateAccountTwoFactorEnabledResult> UpdateAccountTwoFactorEnabledAsync(int accountId, bool twoFactorEnabled)
        {
            if (!await _accountRepository.UpdateAccountTwoFactorEnabledAsync(accountId, twoFactorEnabled))
            {
                return UpdateAccountTwoFactorEnabledResult.GetFailedResult();
            }
            else
            {
                return UpdateAccountTwoFactorEnabledResult.GetSucceededResult();
            }
        }
    }
}

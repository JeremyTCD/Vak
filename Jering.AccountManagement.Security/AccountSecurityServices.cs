using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an API for managing Accounts.
    /// </summary>
    public class AccountSecurityServices<TAccount> : IAccountSecurityServices<TAccount> where TAccount : IAccount
    {
        private ClaimsPrincipalFactory<TAccount> _claimsPrincipalFactory { get; }
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
        /// Constructs a new instance of <see cref="AccountSecurityServices{TAccount}"/>.
        /// </summary>
        /// <param name="claimsPrincipalFactory"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="serviceProvider"></param>
        public AccountSecurityServices(ClaimsPrincipalFactory<TAccount> claimsPrincipalFactory,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountSecurityOptions> securityOptionsAccessor,
            IAccountRepository<TAccount> accountRepository,
            IServiceProvider serviceProvider//,
                                            //IEmailSender emailSender
            )
        {
            if (claimsPrincipalFactory == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipalFactory));
            }

            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            if (accountRepository == null)
            {
                throw new ArgumentNullException(nameof(accountRepository));
            }

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (securityOptionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(securityOptionsAccessor));
            }

            _claimsPrincipalFactory = claimsPrincipalFactory;
            _httpContext = httpContextAccessor.HttpContext;
            _securityOptions = securityOptionsAccessor.Value;
            _accountRepository = accountRepository;

            foreach (string tokenServiceName in _securityOptions.TokenServiceOptions.TokenServiceMap.Keys)
            {
                ITokenService<TAccount> tokenService = (ITokenService<TAccount>)serviceProvider.GetRequiredService(_securityOptions.TokenServiceOptions.TokenServiceMap[tokenServiceName]);
                if (tokenService != null)
                {
                    _tokenServices[tokenServiceName] = tokenService;
                }
            }
        }

        /// <summary>
        /// Signs in specified <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task ApplicationSignInAsync(TAccount account, AuthenticationProperties authenticationProperties)
        {
            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalFactory.CreateClaimsPrincipleAsync(account, _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);

            await _httpContext.Authentication.SignInAsync(
                    _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    claimsPrincipal,
                    authenticationProperties);
        }

        /// <summary>
        /// Signs in account with specified <paramref name="email"/> and <paramref name="password"/> using 
        /// specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/> that returns true if sign in is successful and false otherwise.</returns>
        public virtual async Task<ApplicationSignInResult> ApplicationPasswordSignInAsync(string email, string password, AuthenticationProperties authenticationProperties)
        {
            TAccount account = await _accountRepository.GetAccountByEmailAndPasswordAsync(email, password);
            if (account != null)
            {
                if (account.TwoFactorEnabled)
                {
                    await CreateTwoFactorCookieAsync(account, authenticationProperties);
                    await SendTwoFactorTokenByEmailAsync(account);
                    return ApplicationSignInResult.TwoFactorRequired;
                }

                await ApplicationSignInAsync(account, authenticationProperties);
                return ApplicationSignInResult.Succeeded;
            }

            return ApplicationSignInResult.Failed;
        }

        /// <summary>
        /// Signs out account that sent request. 
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task SignOutAsync()
        {
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
        }

        /// <summary>
        /// Validates <paramref name="token"/>. If valid, sets EmailConfirmed to true for account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="token"></param>
        /// <returns>A <see cref="Task"/> that returns true if <paramref name="token"/> is valid and EmailConfirmed 
        /// updates successfully, false otherwise.</returns>
        public virtual async Task<bool> ConfirmEmailAsync(int accountId, string token)
        {
            TAccount account = await _accountRepository.GetAccountAsync(accountId);

            return await _tokenServices[TokenServiceOptions.DataProtectionTokenService].ValidateTokenAsync(_confirmEmailTokenPurpose, token, account) &&
                await _accountRepository.UpdateAccountEmailConfirmedAsync(accountId);
        }

        /// <summary>
        /// Sends confirmation email to account with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>A <see cref="Task"/> that returns true if confirmation email is sent successfully.</returns>
        public virtual async Task<bool> SendConfirmationEmailAsync(int accountId)
        {
            TAccount account = await _accountRepository.GetAccountAsync(accountId);
            return await SendConfirmationEmailAsync(account);
        }

        /// <summary>
        /// Sends confirmation email to specified <paramref name="account"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>A <see cref="Task"/> that returns true if confirmation email is sent successfully.</returns>
        public virtual async Task<bool> SendConfirmationEmailAsync(TAccount account)
        {
            string token = await _tokenServices[TokenServiceOptions.DataProtectionTokenService].GenerateTokenAsync(_confirmEmailTokenPurpose, account);

            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
            // Send an email with this link
            //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
            //           "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Validates <paramref name="token"/>. If valid, updates PasswordHash for account.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns>A <see cref="Task"/> that returns true if <paramref name="token"/> is valid and PasswordHash 
        /// updates successfully, false otherwise.</returns>
        public virtual async Task<bool> UpdatePasswordAsync(TAccount account, string password, string token)
        {
            return await _tokenServices[TokenServiceOptions.DataProtectionTokenService].ValidateTokenAsync(_confirmEmailTokenPurpose, token, account) &&
                await _accountRepository.UpdateAccountPasswordHashAsync(account.AccountId, password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SendTwoFactorTokenByEmailAsync(TAccount account)
        {
            string token = await _tokenServices[TokenServiceOptions.TotpTokenService].GenerateTokenAsync(_twoFactorTokenPurpose, account);

            //await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(account), "Security Code", "Your security code is: " + token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// Null if authentication fails.
        /// ClaimsPrincipal if authentication succeeds.
        /// </returns>
        public async Task<TAccount> GetTwoFactorAccountAsync()
        {
            ClaimsPrincipal claimsPrincipal = await _httpContext.Authentication.AuthenticateAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            if(claimsPrincipal == null)
            {
                return default(TAccount);
            }

            return await _claimsPrincipalFactory.CreateAccountAsync(claimsPrincipal);
        }

        /// <summary>
        /// Signs in specified <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task CreateTwoFactorCookieAsync(TAccount account, AuthenticationProperties authenticationProperties)
        {
            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalFactory.CreateClaimsPrincipleAsync(account, _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            // TODO: does making the two factor cookie persistent fuck things up? if the user signs out and signs in again does the old cookie get overwritten?
            await _httpContext.Authentication.SignInAsync(
                _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme,
                claimsPrincipal,
                authenticationProperties);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPersistent"></param>
        /// <returns></returns>
        public virtual async Task<TwoFactorSignInResult> TwoFactorSignInAsync(string token, bool isPersistent)
        {
            TAccount account = await GetTwoFactorAccountAsync();
            if (account == null)
            {
                return TwoFactorSignInResult.Failed;
            }

            if (await _tokenServices[TokenServiceOptions.TotpTokenService].ValidateTokenAsync(_twoFactorTokenPurpose, token, account))
            {
                // Cleanup two factor user id cookie
                await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

                await ApplicationSignInAsync(account, new AuthenticationProperties() { IsPersistent = isPersistent });
                return TwoFactorSignInResult.Succeeded;
            }

            return TwoFactorSignInResult.Failed;
        }
    }
}

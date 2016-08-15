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
    /// Provides an API for managing Account security.
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
            _claimsPrincipalFactory = claimsPrincipalFactory;
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
            if(tokenService == null)
            {
                throw new ArgumentNullException(nameof(tokenService));
            }
            _tokenServices[tokenServiceName] = tokenService;
        }

        /// <summary>
        /// Signs in specified <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task ApplicationSignInAsync(TAccount account, AuthenticationProperties authenticationProperties)
        {
            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalFactory.CreateAccountClaimsPrincipalAsync(account, _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);

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
        /// <returns>
        /// <see cref="ApplicationSignInResult.Failed"/> if credentials are invalid. 
        /// <see cref="ApplicationSignInResult.TwoFactorRequired"/> if two factor is required. 
        /// <see cref="ApplicationSignInResult.Succeeded"/> if application sign in is complete. 
        /// </returns>
        public virtual async Task<ApplicationSignInResult> ApplicationPasswordSignInAsync(string email, string password, AuthenticationProperties authenticationProperties)
        {
            TAccount account = await _accountRepository.GetAccountByEmailAndPasswordAsync(email, password);
            if (account != null)
            {
                if (!account.EmailConfirmed)
                {
                    //TODO: handle email confirmation
                    return ApplicationSignInResult.EmailConfirmationRequired;
                }

                if (account.TwoFactorEnabled)
                {
                    await CreateTwoFactorCookieAsync(account);
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
        /// Generates and sends a two factor token to the email address associated with <paramref name="account"/>.
        /// </summary>
        /// <returns></returns>
        public virtual async Task SendTwoFactorTokenByEmailAsync(TAccount account)
        {
            
            string token = await _tokenServices[TokenServiceOptions.TotpTokenService].GenerateTokenAsync(_twoFactorTokenPurpose, account);

            //await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(account), "Security Code", "Your security code is: " + token);
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

            if(claimsPrincipal == null)
            {
                return default(TAccount);
            }

            System.Security.Claims.Claim accountIdClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);

            if(accountIdClaim == null)  
            {
                return default(TAccount);
            }

            int accountId = Convert.ToInt32(accountIdClaim.Value);

            return await _accountRepository.GetAccountAsync(accountId);
        }

        /// <summary>
        /// Instructs cookie authentication middleware to add two factor cookie to <see cref="HttpResponse"/> .
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public virtual async Task CreateTwoFactorCookieAsync(TAccount account)
        {
            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalFactory.CreateAccountIdClaimsPrincipleAsync(account.AccountId, _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            await _httpContext.Authentication.SignInAsync(
                _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme,
                claimsPrincipal);
        }

        /// <summary>
        /// Validates two factor token. If valid, peforms application sign for user specified by two factor cookie.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPersistent"></param>
        /// <returns>
        /// <see cref="TwoFactorSignInResult.Failed"/> if unable to retrieve two factor account.
        /// <see cref="TwoFactorSignInResult.Succeeded"/> if <paramref name="token"/> is valid. 
        /// <see cref="TwoFactorSignInResult.Failed"/> if <paramref name="token"/> is invalid. 
        /// </returns>
        public virtual async Task<TwoFactorSignInResult> TwoFactorSignInAsync(string token, bool isPersistent)
        {
            TAccount account = await GetTwoFactorAccountAsync();
            if (account == null)
            {
                return TwoFactorSignInResult.Failed;
            }

            if (await _tokenServices[TokenServiceOptions.TotpTokenService].ValidateTokenAsync(_twoFactorTokenPurpose, token, account))
            {
                // Cleanup two factor cookie
                await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
                await ApplicationSignInAsync(account, new AuthenticationProperties() { IsPersistent = isPersistent });

                return TwoFactorSignInResult.Succeeded;
            }

            return TwoFactorSignInResult.Failed;
        }
    }
}

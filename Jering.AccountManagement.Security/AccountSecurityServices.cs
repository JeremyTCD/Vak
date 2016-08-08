using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an API for managing Accounts.
    /// </summary>
    public class AccountSecurityServices<TAccount> where TAccount : IAccount
    {
        private ClaimsPrincipalFactory<TAccount> _claimsPrincipalFactory { get; }
        private IAccountRepository<TAccount> _accountRepository { get; }
        private HttpContext _httpContext { get; }
        private AccountSecurityOptions _securityOptions { get; }
        private ITokenService<TAccount> _totpTokenService { get; }
        private ITokenService<TAccount> _dataProtectionTokenService { get; }
        /// <summary>
        /// The data protection purpose used for email confirmation related methods.
        /// </summary>
        protected const string _confirmEmailTokenPurpose = "EmailConfirmation";

        /// <summary>
        /// Constructs a new instance of <see cref="AccountSecurityServices{TAccount}"/>.
        /// </summary>
        /// <param name="claimsPrincipalFactory"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="dataProtectionTokenService"></param>
        /// <param name="totpTokenService"></param>
        public AccountSecurityServices(ClaimsPrincipalFactory<TAccount> claimsPrincipalFactory, 
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountSecurityOptions> securityOptionsAccessor,
            IAccountRepository<TAccount> accountRepository,
            ITokenService<TAccount> totpTokenService,
            ITokenService<TAccount> dataProtectionTokenService)
        {
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _httpContext = httpContextAccessor.HttpContext;
            _securityOptions = securityOptionsAccessor.Value;
            _accountRepository = accountRepository;
            _totpTokenService = totpTokenService;
            _dataProtectionTokenService = dataProtectionTokenService;
        }

        /// <summary>
        /// Signs in specified <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task SignInAsync(TAccount account, AuthenticationProperties authenticationProperties)
        {
            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(account);

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
        public virtual async Task<bool> PasswordSignInAsync(string email, string password, AuthenticationProperties authenticationProperties)
        {
            TAccount account = await _accountRepository.GetAccountByEmailAndPasswordAsync(email, password);
            if (account != null)
            {
                // TODO: Check if 2-factor required. If so do a context.authentiation.SignInAsync with a two factor principal and return
                // SignInResult.TwoFactorRequired
                await SignInAsync(account, authenticationProperties);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Signs out account that sent request. 
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task SignOutAsync()
        {
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);
            // await _httpContext.Authentication.SignOutAsync(_identityOptions.Cookies.TwoFactorUserIdCookieAuthenticationScheme);
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

            return await _dataProtectionTokenService.ValidateToken(_confirmEmailTokenPurpose, token, account) &&
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
            string token = await _dataProtectionTokenService.GenerateToken(_confirmEmailTokenPurpose, account);

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
            return await _dataProtectionTokenService.ValidateToken(_confirmEmailTokenPurpose, token, account) && 
                await _accountRepository.UpdateAccountPasswordHashAsync(account.AccountId, password);
        }

        //public async Task TwoFactorSignInAsync()
        //{
        //    await Task.FromResult(0);
        //}

        //public async Task GetTwoFactorAuthenticationUserAsync()
        //{
        //    await Task.FromResult(0);
        //}        
    }
}

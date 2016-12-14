using Jering.Accounts.DatabaseInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class CookieSecurityStampValidator<TAccount> : ICookieSecurityStampValidator where TAccount : IAccount
    {
        private readonly ClaimsOptions _claimsOptions;
        private readonly CookieAuthOptions _cookieOptions;
        private readonly IAccountRepository<TAccount> _accountRepository;
        private readonly IClaimsPrincipalService<TAccount> _claimsPrincipalService;

        /// <summary>
        /// Constructs an instance of <see cref="CookieSecurityStampValidator{TAccount}"/> .
        /// </summary>
        /// <param name="claimsOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="claimsPrincipalService"></param>
        /// <param name="cookieOptionsAccessor"></param>
        public CookieSecurityStampValidator(IOptions<ClaimsOptions> claimsOptionsAccessor,
                        IOptions<CookieAuthOptions> cookieOptionsAccessor,
                        IAccountRepository<TAccount> accountRepository,
                        IClaimsPrincipalService<TAccount> claimsPrincipalService)
        {
            _accountRepository = accountRepository;
            _claimsOptions = claimsOptionsAccessor.Value;
            _claimsPrincipalService = claimsPrincipalService;
            _cookieOptions = cookieOptionsAccessor.Value;
        }

        /// <summary>
        /// Validates <see cref="ClaimsPrincipal"/> security stamp claim. Logs account out and rejects <see cref="ClaimsPrincipal"/>  if security stamp
        /// is invalid.
        /// </summary>
        /// <param name="context"></param>
        public virtual async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context.Principal != null) {

                if (context.Principal.Identity?.AuthenticationType == _cookieOptions.ApplicationCookieOptions.AuthenticationScheme)
                {
                    System.Security.Claims.Claim accountIdClaim = context.Principal.FindFirst(_claimsOptions.AccountIdClaimType);
                    if (accountIdClaim != null)
                    {
                        TAccount account = await _accountRepository.GetAccountAsync(Convert.ToInt32(accountIdClaim.Value));

                        if(account?.SecurityStamp.ToString() == context.Principal.FindFirst(_claimsOptions.SecurityStampClaimType)?.Value)
                        {
                            return;
                        }
                    }
                }

                context.RejectPrincipal();
                await context.HttpContext.Authentication.SignOutAsync(_cookieOptions.ApplicationCookieOptions.AuthenticationScheme);
                await context.HttpContext.Authentication.SignOutAsync(_cookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
            }
        }
    }

    /// <summary>
    /// Static helper class used to configure a CookieAuthNotifications to validate a cookie against a user's security
    /// stamp.
    /// </summary>
    public static class CookieSecurityStampValidator
    {
        /// <summary>
        /// Validates a principal against a user's stored security stamp.
        /// the identity.
        /// </summary>
        /// <param name="context">The context containing the <see cref="ClaimsPrincipal"/>
        /// and <see cref="AuthenticationProperties"/> to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous validation operation.</returns>
        public static Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
        {
            if (context.HttpContext.RequestServices == null)
            {
                throw new InvalidOperationException("RequestServices is null.");
            }

            ICookieSecurityStampValidator validator = context.HttpContext.RequestServices.GetRequiredService<ICookieSecurityStampValidator>();
            return validator.ValidateAsync(context);
        }
    }
}

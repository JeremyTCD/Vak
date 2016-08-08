using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class CookieSecurityStampValidator<TAccount> where TAccount : IAccount
    {
        private readonly AccountSecurityOptions _securityOptions;
        private readonly IAccountRepository<TAccount> _accountRepository;
        private readonly AccountSecurityServices<TAccount> _accountSecurityServices;
        private readonly ClaimsPrincipalFactory<TAccount> _claimsPrincipalFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="accountSecurityServices"></param>
        /// <param name="claimsPrincipalFactory"></param>
        public CookieSecurityStampValidator(IOptions<AccountSecurityOptions> securityOptionsAccessor,
                        IAccountRepository<TAccount> accountRepository,
                        AccountSecurityServices<TAccount> accountSecurityServices,
                        ClaimsPrincipalFactory<TAccount> claimsPrincipalFactory)
        {
            _accountRepository = accountRepository;
            _securityOptions = securityOptionsAccessor.Value;
            _accountSecurityServices = accountSecurityServices;
            _claimsPrincipalFactory = claimsPrincipalFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
            if (context.Options != null && context.Options.SystemClock != null)
            {
                currentUtc = context.Options.SystemClock.UtcNow;
            }

            DateTimeOffset? issuedUtc = context.Properties.IssuedUtc;
            TimeSpan timeElapsed = currentUtc.Subtract(issuedUtc.Value);

            if (timeElapsed > _securityOptions.CookieOptions.CookieSecurityStampLifespan)
            {
                int accountId = Int32.Parse(context.Principal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType).Value);
                string cookieSecurityStamp = context.Principal.FindFirst(_securityOptions.ClaimsOptions.SecurityStampClaimType).Value;
                TAccount account = await _accountRepository.GetAccountAsync(accountId);
                if (account.SecurityStamp.ToString() != cookieSecurityStamp)
                {
                    context.RejectPrincipal();
                    await _accountSecurityServices.SignOutAsync();
                }
                else
                {
                    context.ReplacePrincipal(await _claimsPrincipalFactory.CreateAsync(account));
                    context.ShouldRenew = true;
                }
            }          
        }
    }

    /// <summary>
    /// Static helper class used to configure a CookieAuthenticationNotifications to validate a cookie against a user's security
    /// stamp.
    /// </summary>
    public static class CookieSecurityStampValidator
    {
        /// <summary>
        /// Validates a principal against a user's stored security stamp.
        /// the identity.
        /// </summary>
        /// <param name="context">The context containing the <see cref="System.Security.Claims.ClaimsPrincipal"/>
        /// and <see cref="Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties"/> to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous validation operation.</returns>
        public static Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
        {
            // TODO this should never be null, confirm and remove
            if (context.HttpContext.RequestServices == null)
            {
                throw new InvalidOperationException("RequestServices is null.");
            }

            CookieSecurityStampValidator<IAccount> validator = context.HttpContext.RequestServices.GetRequiredService<CookieSecurityStampValidator<IAccount>>();
            return validator.ValidateAsync(context);
        }
    }
}

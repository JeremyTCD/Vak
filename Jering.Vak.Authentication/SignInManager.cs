using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Jering.Vak.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Jering.Vak.Authentication
{
    public class SignInManager
    {
        public SignInManager(ClaimsPrincipalFactory claimsPrincipalFactory, 
            IHttpContextAccessor httpContextAccessor,
            IOptions<IdentityOptions> identityOptionsAccessor)
        {
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _httpContext = httpContextAccessor.HttpContext;
            _identityOptions = identityOptionsAccessor.Value;
        }

        private ClaimsPrincipalFactory _claimsPrincipalFactory { get; }
        private HttpContext _httpContext { get; }
        private IdentityOptions _identityOptions { get; }

        public async Task SignInAsync(Account account, AuthenticationProperties authenticationProperties)
        {
            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(account);

            //if (authenticationMethod != null)
            //{
                //userPrincipal.Identities.First().AddClaim(new Claim(ClaimTypes.AuthenticationMethod, authenticationMethod));
            //}
            await _httpContext.Authentication.SignInAsync(
                    _identityOptions.Cookies.ApplicationCookieAuthenticationScheme,
                    claimsPrincipal,
                    authenticationProperties);
        }
    }
}

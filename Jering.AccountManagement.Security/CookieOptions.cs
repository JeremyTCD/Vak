using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for cookie authentication. 
    /// </summary>
    public class CookieOptions
    {
        private static readonly string CookiePrefix = "Jering";
        private static readonly string DefaultApplicationScheme = CookiePrefix + ".Application";
        private static readonly string DefaultTwoFactorRememberMeScheme = CookiePrefix + ".TwoFactorRememberMe";
        private static readonly string DefaultTwoFactorUserIdScheme = CookiePrefix + ".TwoFactorUserId";

        /// <summary>
        /// The options for the application cookie.
        /// </summary>
        public CookieAuthenticationOptions ApplicationCookieOptions { get; set; } = new CookieAuthenticationOptions
        {
            AuthenticationScheme = DefaultApplicationScheme,
            AutomaticAuthenticate = true,
            AutomaticChallenge = true,
            LoginPath = new PathString("/Account/Login"),
            Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = CookieSecurityStampValidator.ValidatePrincipalAsync
            }
        };

        /// <summary>
        /// The options for the two factor remember me cookie.
        /// </summary>
        public CookieAuthenticationOptions TwoFactorRememberMeCookieOptions { get; set; } = new CookieAuthenticationOptions
        {
            AutomaticAuthenticate = false,
            AuthenticationScheme = DefaultTwoFactorRememberMeScheme,
            CookieName = DefaultTwoFactorRememberMeScheme
        };

        /// <summary>
        /// The options for the two factor user id cookie.
        /// </summary>
        public CookieAuthenticationOptions TwoFactorUserIdCookieOptions { get; set; } = new CookieAuthenticationOptions
        {
            AutomaticAuthenticate = false,
            AuthenticationScheme = DefaultTwoFactorUserIdScheme,
            CookieName = DefaultTwoFactorUserIdScheme,
            ExpireTimeSpan = TimeSpan.FromMinutes(5)
        };

        /// <summary>
        /// Lifespan of cookie security stamps.
        /// </summary>
        public TimeSpan CookieSecurityStampLifespan { get; set; } = TimeSpan.FromMinutes(30);
    }
}

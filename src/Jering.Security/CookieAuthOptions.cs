using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace Jering.Security
{
    /// <summary>
    /// Specifies options for cookie auth. 
    /// </summary>
    public class CookieAuthOptions
    {
        private static readonly string CookiePrefix = "Jering";
        private static readonly string DefaultApplicationScheme = CookiePrefix + ".Application";
        private static readonly string DefaultTwoFactorScheme = CookiePrefix + ".TwoFactor";
        private static readonly string DefaultEmailConfirmationScheme = CookiePrefix + ".EmailConfirmation";

        /// <summary>
        /// The options for the application cookie.
        /// </summary>
        public CookieAuthenticationOptions ApplicationCookieOptions { get; set; } = new CookieAuthenticationOptions
        {
            AuthenticationScheme = DefaultApplicationScheme,
            CookieName = DefaultApplicationScheme,
            AutomaticAuthenticate = true,
            AutomaticChallenge = true,
            SlidingExpiration = true,
            Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = CookieSecurityStampValidator.ValidatePrincipalAsync
            }
        };

        /// <summary>
        /// The options for the two factor cookie.
        /// </summary>
        public CookieAuthenticationOptions TwoFactorCookieOptions { get; set; } = new CookieAuthenticationOptions
        {
            AutomaticAuthenticate = false,
            AuthenticationScheme = DefaultTwoFactorScheme,
            CookieName = DefaultTwoFactorScheme,
            ExpireTimeSpan = TimeSpan.FromMinutes(5),
        };
    }
}

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for cookie authentication. 
    /// </summary>
    public class CookieOptions
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
            LoginPath = new PathString("/Account/LogIn"),
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
            ExpireTimeSpan = TimeSpan.FromMinutes(5)
        };
    }
}

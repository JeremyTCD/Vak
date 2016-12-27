using Jering.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Specifies options for <see cref="Jering.Accounts.AccountService"/>. 
    /// </summary>
    public class AccountServiceOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="ClaimsOptions"/> for the security library.
        /// </summary>
        public ClaimsOptions ClaimsOptions { get; set; } = new ClaimsOptions();

        /// <summary>
        /// Gets or sets the <see cref="CookieOptions"/> for the security library. 
        /// </summary>
        public CookieAuthOptions CookieOptions { get; set; } = new CookieAuthOptions();

        /// <summary>
        /// Gets or sets the <see cref="TokenServiceOptions"/> for the security library. 
        /// </summary>
        public TokenServiceOptions TokenServiceOptions { get; set; } = new TokenServiceOptions();

        public string EmailVerificationEmailSubject { get; set; }
        public string EmailVerificationEmailMessage { get; set; }
        public string EmailVerificationLinkDomain { get; set; }
        public string EmailVerificationLinkPath { get; set; }
        public string AltEmailVerificationLinkPath { get; set; }

        public string ResetPasswordEmailSubject { get; set; }
        public string ResetPasswordEmailMessage { get; set; }
        public string ResetPasswordLinkDomain { get; set; }
        public string ResetPasswordLinkPath { get; set; }

        public string TwoFactorCodeEmailSubject { get; set; }
        public string TwoFactorCodeEmailMessage { get; set; }

        public string ConfirmEmailTokenPurpose { get; set; } = "EmailConfirmation";
        public string TwoFactorTokenPurpose { get; set; } = "TwoFactor";
        public string ResetPasswordTokenPurpose { get; set; } = "ResetPassword";
        public string ConfirmAltEmailTokenPurpose { get; set; } = "ConfirmAltEmail";
    }
}

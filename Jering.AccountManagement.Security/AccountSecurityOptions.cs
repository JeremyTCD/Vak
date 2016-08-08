using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for <see cref="Jering.AccountManagement.Security"/>. 
    /// </summary>
    public class AccountSecurityOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="ClaimsOptions"/> for the security library.
        /// </summary>
        public ClaimsOptions ClaimsOptions { get; set; } = new ClaimsOptions();

        /// <summary>
        /// Gets or sets the <see cref="CookieOptions"/> for the security library. 
        /// </summary>
        public CookieOptions CookieOptions { get; set; } = new CookieOptions();

        /// <summary>
        /// Gets or sets the <see cref="DataProtectionTokenOptions"/> for the security library. 
        /// </summary>
        public DataProtectionTokenOptions DataProtectionTokenOptions { get; set; } = new DataProtectionTokenOptions();
    }
}

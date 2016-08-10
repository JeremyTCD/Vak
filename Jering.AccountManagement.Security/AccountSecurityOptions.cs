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
        public ClaimOptions ClaimsOptions { get; set; } = new ClaimOptions();

        /// <summary>
        /// Gets or sets the <see cref="CookieOptions"/> for the security library. 
        /// </summary>
        public CookieOptions CookieOptions { get; set; } = new CookieOptions();

        /// <summary>
        /// Gets or sets the <see cref="TokenServiceOptions"/> for the security library. 
        /// </summary>
        public TokenServiceOptions TokenServiceOptions { get; set; } = new TokenServiceOptions();
    }
}

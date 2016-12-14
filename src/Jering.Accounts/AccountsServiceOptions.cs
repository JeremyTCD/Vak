using Jering.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Specifies options for <see cref="Jering.Accounts.AccountsService"/>. 
    /// </summary>
    public class AccountsServiceOptions
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
    }
}

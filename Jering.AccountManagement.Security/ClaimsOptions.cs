using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for <see cref="Claim"/>s. 
    /// </summary>
    public class ClaimsOptions
    {
        /// <summary>
        /// Gets or sets the ClaimType used for a Role claim.
        /// </summary>
        /// <remarks>
        /// This defaults to <see cref="ClaimTypes.Role"/>.
        /// </remarks>
        public string RoleClaimType { get; set; } = ClaimTypes.Role;

        /// <summary>
        /// Gets or sets the ClaimType used for the user name claim.
        /// </summary>
        /// <remarks>
        /// This defaults to <see cref="ClaimTypes.Name"/>.
        /// </remarks>
        public string UsernameClaimType { get; set; } = ClaimTypes.Name;

        /// <summary>
        /// Gets or sets the ClaimType used for the user identifier claim.
        /// </summary>
        /// <remarks>
        /// This defaults to <see cref="ClaimTypes.NameIdentifier"/>.
        /// </remarks>
        public string AccountIdClaimType { get; set; } = ClaimTypes.NameIdentifier;

        /// <summary>
        /// Gets or sets the ClaimType used for the security stamp claim..
        /// </summary>
        /// <remarks>
        /// This defaults to "AspNet.Identity.SecurityStamp".
        /// </remarks>
        public string SecurityStampClaimType { get; set; } = "Jering.VectorArtKit.Account.SecurityStamp";
    }
}

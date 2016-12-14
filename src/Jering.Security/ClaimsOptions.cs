using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.Security
{
    /// <summary>
    /// Specifies options for <see cref="Claim"/>s. 
    /// </summary>
    public class ClaimsOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public string RoleClaimType { get; set; } = ClaimTypes.Role;

        /// <summary>
        /// 
        /// </summary>
        public string UsernameClaimType { get; set; } = ClaimTypes.Name;

        /// <summary>
        /// 
        /// </summary>
        public string AccountIdClaimType { get; set; } = ClaimTypes.NameIdentifier;

        /// <summary>
        /// 
        /// </summary>
        public string IsPersistenClaimType { get; set; } = ClaimTypes.IsPersistent;

        /// <summary>
        /// 
        /// </summary>
        public string SecurityStampClaimType { get; set; } = "Jering.VectorArtKit.Account.SecurityStamp";
    }
}

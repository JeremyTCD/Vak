using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class Claims
    {
        public Claims()
        {
            AccountClaims = new HashSet<AccountClaims>();
            RoleClaims = new HashSet<RoleClaims>();
        }

        public int ClaimId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public virtual ICollection<AccountClaims> AccountClaims { get; set; }
        public virtual ICollection<RoleClaims> RoleClaims { get; set; }
    }
}

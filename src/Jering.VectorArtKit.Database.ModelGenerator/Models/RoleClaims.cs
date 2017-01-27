using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class RoleClaims
    {
        public int ClaimId { get; set; }
        public int RoleId { get; set; }

        public virtual Claims Claim { get; set; }
        public virtual Roles Role { get; set; }
    }
}

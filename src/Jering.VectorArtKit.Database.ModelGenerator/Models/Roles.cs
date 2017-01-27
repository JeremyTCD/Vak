using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class Roles
    {
        public Roles()
        {
            AccountRoles = new HashSet<AccountRoles>();
            RoleClaims = new HashSet<RoleClaims>();
        }

        public int RoleId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AccountRoles> AccountRoles { get; set; }
        public virtual ICollection<RoleClaims> RoleClaims { get; set; }
    }
}

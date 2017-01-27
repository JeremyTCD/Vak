using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class AccountRoles
    {
        public int AccountId { get; set; }
        public int RoleId { get; set; }

        public virtual Accounts Account { get; set; }
        public virtual Roles Role { get; set; }
    }
}

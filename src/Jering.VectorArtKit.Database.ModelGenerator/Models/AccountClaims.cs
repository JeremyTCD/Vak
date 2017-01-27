using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class AccountClaims
    {
        public int ClaimId { get; set; }
        public int AccountId { get; set; }

        public virtual Accounts Account { get; set; }
        public virtual Claims Claim { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class Accounts
    {
        public Accounts()
        {
            AccountClaims = new HashSet<AccountClaims>();
            AccountRoles = new HashSet<AccountRoles>();
            VakUnits = new HashSet<VakUnits>();
        }

        public int AccountId { get; set; }
        public string DisplayName { get; set; }
        public Guid SecurityStamp { get; set; }
        public string PasswordHash { get; set; }
        public DateTimeOffset PasswordLastChanged { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string AltEmail { get; set; }
        public bool AltEmailVerified { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public byte[] RowVersion { get; set; }

        public virtual ICollection<AccountClaims> AccountClaims { get; set; }
        public virtual ICollection<AccountRoles> AccountRoles { get; set; }
        public virtual ICollection<VakUnits> VakUnits { get; set; }
    }
}

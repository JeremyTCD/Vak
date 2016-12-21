using System;
using Jering.Accounts.DatabaseInterface;

namespace Jering.VectorArtKit.DatabaseInterface
{
    public class VakAccount : IAccount
    {
        public int AccountId { get; set; }

        public string AltEmail { get; set; }

        public bool AltEmailVerified { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public bool EmailVerified { get; set; }

        public string PasswordHash { get; set; }

        public DateTimeOffset PasswordLastChanged { get; set; }

        public byte[] RowVersion { get; set; }

        public Guid SecurityStamp { get; set; }

        public bool TwoFactorEnabled { get; set; }
    }
}

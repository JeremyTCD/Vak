using System;

namespace Jering.Accounts.DatabaseInterface
{
    public interface IAccount
    {
        int AccountId { get; set; }

        string DisplayName { get; set; }

        DateTimeOffset PasswordLastChanged { get; set; }

        Guid SecurityStamp { get; set; }

        string Email { get; set; }

        string AltEmail { get; set; }

        string PasswordHash { get; set; }

        bool EmailVerified { get; set; }

        bool AltEmailVerified { get; set; }

        bool TwoFactorEnabled { get; set; }

        byte[] RowVersion { get; set; }
    }
}

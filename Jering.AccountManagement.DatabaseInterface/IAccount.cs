using System;

namespace Jering.AccountManagement.DatabaseInterface
{
    /// <summary>
    /// Represents an account.
    /// </summary>
    public interface IAccount
    {
        /// <summary>
        /// Account Id.
        /// </summary>
        int AccountId { get; set; }

        /// <summary>
        /// Account's username.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// A random value that changes whenever an account's security data changes.
        /// </summary>
        Guid SecurityStamp { get; set; }

        /// <summary>
        /// Password hash.
        /// </summary>
        byte[] PasswordHash { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string AlternativeEmail { get; set; }

        /// <summary>
        /// True if the email is confirmed, false otherwise.
        /// </summary>
        bool EmailVerified { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool AlternativeEmailVerified { get; set; }

        /// <summary>
        /// True if two factor is enabled, false otherwise.
        /// </summary>
        bool TwoFactorEnabled { get; set; }
    }
}

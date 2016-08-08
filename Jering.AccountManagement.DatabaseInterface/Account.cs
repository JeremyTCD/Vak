using System;

namespace Jering.AccountManagement.DatabaseInterface
{
    /// <summary>
    /// Represents an account.
    /// </summary>
    public class Account : IAccount
    {
        /// <summary>
        /// Account Id.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Account's username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// A random value that changes whenever an account's security data changes.
        /// </summary>
        public Guid SecurityStamp { get; set; }

        /// <summary>
        /// Password hash.
        /// </summary>
        public byte[] PasswordHash { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// True if the email is confirmed, false otherwise.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// True if two factor is enabled, false otherwise.
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}

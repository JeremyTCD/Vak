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
        /// Account's display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime PasswordLastChanged { get; set; }

        /// <summary>
        /// A random value that changes whenever an account's security data changes.
        /// </summary>
        public Guid SecurityStamp { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// True if the email is confirmed, false otherwise.
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AlternativeEmailVerified { get; set; }

        /// <summary>
        /// True if two factor is enabled, false otherwise.
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}

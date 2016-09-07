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
        public virtual int AccountId { get; set; }

        /// <summary>
        /// Account's display name.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime PasswordLastChanged { get; set; }

        /// <summary>
        /// A random value that changes whenever an account's security data changes.
        /// </summary>
        public virtual Guid SecurityStamp { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string AlternativeEmail { get; set; }

        /// <summary>
        /// True if the email is confirmed, false otherwise.
        /// </summary>
        public virtual bool EmailVerified { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool AlternativeEmailVerified { get; set; }

        /// <summary>
        /// True if two factor is enabled, false otherwise.
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }
    }
}

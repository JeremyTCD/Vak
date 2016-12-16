using System;

namespace Jering.Accounts.DatabaseInterface
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
        public virtual DateTimeOffset PasswordLastChanged { get; set; }

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
        public virtual string AltEmail { get; set; }

        /// <summary>
        /// True if the email is confirmed, false otherwise.
        /// </summary>
        public virtual bool EmailVerified { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool AltEmailVerified { get; set; }

        /// <summary>
        /// True if two factor is enabled, false otherwise.
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual byte[] RowVersion { get; set; }
    }
}

using Jering.AccountManagement.DatabaseInterface;
using System;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class TestAccount : IAccount
    {
        /// <summary>
        /// Account Id.
        /// </summary>
        public virtual int AccountId { get; set; }

        /// <summary>
        /// Account's username.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// A random value that changes whenever an account's security data changes.
        /// </summary>
        public virtual Guid SecurityStamp { get; set; }

        /// <summary>
        /// Password hash.
        /// </summary>
        public virtual byte[] PasswordHash { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// True if the email is confirmed, false otherwise.
        /// </summary>
        public virtual bool EmailVerified { get; set; }

        /// <summary>
        /// True if two factor is enabled, false otherwise.
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string AlternativeEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool AlternativeEmailVerified { get; set; }
    }
}

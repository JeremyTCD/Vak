using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security.UnitTests
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
        public virtual string Username { get; set; }

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
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// True if two factor is enabled, false otherwise.
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }
    }
}

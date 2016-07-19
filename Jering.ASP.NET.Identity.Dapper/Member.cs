using Microsoft.AspNet.Identity;
using System;

namespace Jering.ASP.NET.Identity.Dapper
{
    /// <summary>
    /// Class that implements the ASP.NET Identity IUser interface 
    /// </summary>
    public class Member : IUser<int>
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public Member()
        {
            //  Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Constructor that takes member name as argument
        /// </summary>
        /// <param name="memberName"></param>
        public Member(string memberName)
            : this()
        {
            UserName = memberName;
        }

        /// <summary>
        /// Member ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Member's name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Email
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        ///     The salted/hashed form of the member password
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        ///     A random value that should change whenever a members credentials have changed (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        ///     Is two factor enabled for the member
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this member
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public virtual int AccessFailedCount { get; set; }
    }
}

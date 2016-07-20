using Microsoft.AspNetCore.Identity;
using System;

namespace Jering.ASP.NET.Identity.Dapper
{
    /// <summary>
    /// Class that implements the ASP.NET Identity IUser interface 
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public Member()
        {
        }

        /// <summary>
        /// Constructor that takes member name as argument
        /// </summary>
        /// <param name="memberName"></param>
        public Member(string memberName)
            : this()
        {
            Username = memberName;
        }

        /// <summary>
        /// Member ID
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// Member's username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Member's normalized username
        /// </summary>
        public string NormalizedUsername { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///  Email
        /// </summary>
        public string NormalizedEmail { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        ///     The salted/hashed form of the member password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        ///     A random value that should change whenever a members credentials have changed (password changed, login removed)
        /// </summary>
        public string SecurityStamp { get; set; }

        /// <summary>
        ///     Is two factor enabled for the member
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this member
        /// </summary>
        public bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public int AccessFailedCount { get; set; }
    }
}

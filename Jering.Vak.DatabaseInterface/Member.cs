using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Vak.DatabaseInterface
{
    public class Member
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public Member()
        {
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
        ///     A random value that should change whenever a members credentials have changed (password changed, login removed)
        /// </summary>
        public string SecurityStamp { get; set; }

        /// <summary>
        ///     The salted/hashed form of the member password
        /// </summary>
        public byte[] PasswordHash { get; set; }

        /// <summary>
        ///  Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        ///     Is two factor enabled for the member
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}

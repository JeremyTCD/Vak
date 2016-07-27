using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Vak.DatabaseInterface
{
    public class Account
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public Account()
        {
        }

        /// <summary>
        /// Account ID
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Account's username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     A random value that should change whenever a accounts credentials have changed (password changed, login removed)
        /// </summary>
        public string SecurityStamp { get; set; }

        /// <summary>
        ///  Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        ///     Is two factor enabled for the account
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}

using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.ConfirmEmailAsync(string)"/>.
    /// </summary>
    public class ConfirmEmailResult { 
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool InvalidToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ExpiredToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool NotLoggedIn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ConfirmEmailResult GetNotLoggedInResult()
        {
            return new ConfirmEmailResult() { NotLoggedIn = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ConfirmEmailResult GetSucceededResult()
        {
            return new ConfirmEmailResult() { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ConfirmEmailResult GetInvalidTokenResult()
        {
            return new ConfirmEmailResult() { InvalidToken = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ConfirmEmailResult GetExpiredTokenResult()
        {
            return new ConfirmEmailResult() { ExpiredToken = true };
        }
    }
}


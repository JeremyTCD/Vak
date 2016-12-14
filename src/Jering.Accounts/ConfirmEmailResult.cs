using Jering.Accounts.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.ConfirmEmailAsync(string)"/>.
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


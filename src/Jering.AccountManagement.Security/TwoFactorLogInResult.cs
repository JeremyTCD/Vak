using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.TwoFactorLogInAsync"/>.
    /// </summary>
    public class TwoFactorLogInResult<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TAccount Account { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static TwoFactorLogInResult<TAccount> GetSucceededResult(TAccount account)
        {
            return new TwoFactorLogInResult<TAccount>() { Succeeded = true, Account = account };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static TwoFactorLogInResult<TAccount> GetFailedResult()
        {
            return new TwoFactorLogInResult<TAccount>() { Failed = true };
        }
    }
}

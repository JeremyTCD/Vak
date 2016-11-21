using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.PasswordLogInAsync"/>.
    /// </summary>
    public class PasswordLogInResult<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool TwoFactorRequired { get; set; }
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
        public static PasswordLogInResult<TAccount> GetSucceededResult()
        {
            return new PasswordLogInResult<TAccount>() { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PasswordLogInResult<TAccount> GetFailedResult()
        {
            return new PasswordLogInResult<TAccount>() { Failed = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static PasswordLogInResult<TAccount> GetTwoFactorRequiredResult(TAccount account)
        {
            return new PasswordLogInResult<TAccount>() { TwoFactorRequired = true, Account = account };
        }
    }
}

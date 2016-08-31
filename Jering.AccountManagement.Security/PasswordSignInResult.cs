using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityServices{TAccount}.PasswordSignInAsync"/>.
    /// </summary>
    public class PasswordSignInResult<TAccount> where TAccount : IAccount
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
        public static PasswordSignInResult<TAccount> GetSucceededResult()
        {
            return new PasswordSignInResult<TAccount>() { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PasswordSignInResult<TAccount> GetFailedResult()
        {
            return new PasswordSignInResult<TAccount>() { Failed = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static PasswordSignInResult<TAccount> GetTwoFactorRequiredResult(TAccount account)
        {
            return new PasswordSignInResult<TAccount>() { TwoFactorRequired = true, Account = account };
        }
    }
}

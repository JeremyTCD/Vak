using Jering.Accounts.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.PasswordLogInAsync"/>.
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
        public bool InvalidCredentials { get; set; }
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
        public static PasswordLogInResult<TAccount> GetInvalidCredentialsResult()
        {
            return new PasswordLogInResult<TAccount>() { InvalidCredentials = true };
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

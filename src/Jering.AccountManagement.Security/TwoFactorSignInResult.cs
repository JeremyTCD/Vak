using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.TwoFactorSignInAsync"/>.
    /// </summary>
    public class TwoFactorSignInResult
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
        /// <returns></returns>
        public static TwoFactorSignInResult GetSucceededResult()
        {
            return new TwoFactorSignInResult() { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static TwoFactorSignInResult GetFailedResult()
        {
            return new TwoFactorSignInResult() { Failed = true };
        }
    }
}

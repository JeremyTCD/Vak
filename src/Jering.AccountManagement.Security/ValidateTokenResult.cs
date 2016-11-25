using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="ITokenService{TAccount}.ValidateToken(string, string, TAccount)"/>.
    /// </summary>
    public class ValidateTokenResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Expired { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Invalid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ValidateTokenResult GetValidResult()
        {
            return new ValidateTokenResult() { Valid = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ValidateTokenResult GetInvalidResult()
        {
            return new ValidateTokenResult() { Invalid = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ValidateTokenResult GetExpiredResult()
        {
            return new ValidateTokenResult() { Expired = true };
        }
    }
}

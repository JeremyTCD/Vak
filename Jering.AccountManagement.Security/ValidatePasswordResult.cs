using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// 
    /// </summary>
    public enum ValidatePasswordResult
    {
        /// <summary>
        /// 
        /// </summary>
        TooShort,
        /// <summary>
        /// 
        /// </summary>
        NonAlphanumericRequired,
        /// <summary>
        /// 
        /// </summary>
        DigitRequired,
        /// <summary>
        /// 
        /// </summary>
        LowercaseRequired,
        /// <summary>
        /// 
        /// </summary>
        UppercaseRequired,
        /// <summary>
        /// 
        /// </summary>
        Valid
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for <see cref="PasswordValidationService"/>.
    /// </summary>
    public class PasswordOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public int RequiredLength { get; set; } = 8;

        /// <summary>
        /// 
        /// </summary>
        public bool NonAlphanumericRequired { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool DigitRequired { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool LowerCaseRequired { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool UpperCaseRequired { get; set; } = true;
    }
}

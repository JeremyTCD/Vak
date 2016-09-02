using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    /// <summary>
    /// 
    /// </summary>
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }

        public string Token { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ConfirmPassword { get; set; }
    }
}

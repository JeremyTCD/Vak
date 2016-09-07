using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [ValidateEmailAddress(nameof(StringOptions.ErrorMessage_Email_Invalid), typeof(StringOptions))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}

using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [ValidateEmailAddress(nameof(StringOptions.Email_Invalid), typeof(StringOptions))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}

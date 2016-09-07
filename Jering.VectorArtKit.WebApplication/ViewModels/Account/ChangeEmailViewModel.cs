using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class ChangeEmailViewModel
    {
        public string CurrentEmail { get; set; }

        [Required]
        [ValidateEmailAddress(nameof(StringOptions.ErrorMessage_Email_Invalid), typeof(StringOptions))]
        [ValidateDiffers(nameof(CurrentEmail), nameof(StringOptions.ErrorMessage_NewEmail_MustDiffer), typeof(StringOptions))]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

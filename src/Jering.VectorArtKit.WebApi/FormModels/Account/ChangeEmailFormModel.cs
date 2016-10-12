using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class ChangeEmailFormModel
    {
        public string CurrentEmail { get; set; }

        [Required]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [ValidateDiffers(nameof(CurrentEmail), nameof(Strings.ErrorMessage_NewEmail_MustDiffer), typeof(Strings))]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class ChangeAlternativeEmailViewModel
    {
        public string CurrentAlternativeEmail { get; set; }

        [Required]
        [ValidateEmailAddress(nameof(StringOptions.ErrorMessage_Email_Invalid), typeof(StringOptions))]
        [ValidateDiffers(nameof(CurrentAlternativeEmail), nameof(StringOptions.ErrorMessage_NewEmail_MustDiffer), typeof(StringOptions))]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "New alternative email")]
        public string NewAlternativeEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class ChangeAlternativeEmailFormModel
    {
        public string CurrentAlternativeEmail { get; set; }

        [Required]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [ValidateDiffers(nameof(CurrentAlternativeEmail), nameof(Strings.ErrorMessage_NewEmail_MustDiffer), typeof(Strings))]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "New alternative email")]
        public string NewAlternativeEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class SignUpViewModel
    {
        [Required]
        [ValidateEmailAddress(nameof(StringOptions.ErrorMessage_Email_Invalid), typeof(StringOptions))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [ValidateMinLength(8)]
        [ValidateHasLowercase]
        [ValidateHasUppercase]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [ValidateMatches(nameof(Password), nameof(StringOptions.ErrorMessage_ConfirmPassword_Differs), typeof(StringOptions))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}

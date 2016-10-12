using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class LoginFormModel
    {
        [Required]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [ValidateMinLength(8, nameof(Strings.ErrorMessage_Password_TooShort), typeof(Strings))]
        [ValidateHasLowercase(nameof(Strings.ErrorMessage_Password_RequiresLowerCase), typeof(Strings))]
        [ValidateHasUppercase(nameof(Strings.ErrorMessage_Password_RequiresUpperCase), typeof(Strings))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

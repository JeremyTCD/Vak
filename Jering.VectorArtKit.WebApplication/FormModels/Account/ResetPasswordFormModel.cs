using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.FormModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ResetPasswordFormModel
    {
        public string Email { get; set; }

        public string Token { get; set; }

        [Required]
        [ValidateMinLength(8, nameof(Strings.ErrorMessage_Password_TooShort), typeof(Strings))]
        [ValidateHasLowercase(nameof(Strings.ErrorMessage_Password_RequiresLowerCase), typeof(Strings))]
        [ValidateHasUppercase(nameof(Strings.ErrorMessage_Password_RequiresUpperCase), typeof(Strings))]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [ValidateMatches(nameof(NewPassword), nameof(Strings.ErrorMessage_ConfirmPassword_Differs), typeof(Strings))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; }
    }
}

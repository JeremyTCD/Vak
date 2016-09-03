using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    /// <summary>
    /// 
    /// </summary>
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }

        public string Token { get; set; }

        [Required]
        [ValidateMinLength(8)]
        [ValidateHasLowercase]
        [ValidateHasUppercase]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [ValidateMatches(nameof(NewPassword), nameof(StringOptions.ConfirmPassword_DoesNotMatchPassword), typeof(StringOptions))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; }
    }
}

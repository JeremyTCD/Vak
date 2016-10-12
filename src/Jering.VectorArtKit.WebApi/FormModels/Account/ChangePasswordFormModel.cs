using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class ChangePasswordFormModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required]
        [ValidateMinLength(8, nameof(Strings.ErrorMessage_Password_TooShort), typeof(Strings))]
        [ValidateHasLowercase(nameof(Strings.ErrorMessage_Password_RequiresLowerCase), typeof(Strings))]
        [ValidateHasUppercase(nameof(Strings.ErrorMessage_Password_RequiresUpperCase), typeof(Strings))]
        [ValidateDiffers(nameof(CurrentPassword), nameof(Strings.ErrorMessage_NewPassword_MustDiffer), typeof(Strings))]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [ValidateMatches(nameof(NewPassword), nameof(Strings.ErrorMessage_ConfirmPassword_Differs), typeof(Strings))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; }
    }
}

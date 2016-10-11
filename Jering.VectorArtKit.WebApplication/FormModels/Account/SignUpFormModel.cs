using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApplication.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.FormModels
{
    public class SignUpFormModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Required), typeof(Strings))]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [DynamicInput(false, "email", nameof(Strings.DisplayName_Email), typeof(Strings), "input", 0, placeholderResourceName: nameof(Strings.DisplayName_Email))]
        public string Email { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_Required), typeof(Strings))]
        //[ValidateHasLowercase(nameof(Strings.ErrorMessage_Password_RequiresLowerCase), typeof(Strings))]
        //[ValidateHasUppercase(nameof(Strings.ErrorMessage_Password_RequiresUpperCase), typeof(Strings))]
        [DynamicInput(false, "password", nameof(Strings.DisplayName_Password), typeof(Strings), "input", 1, placeholderResourceName: nameof(Strings.DisplayName_Password))]
        public string Password { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_Required), typeof(Strings))]
        //[ValidateMatches(nameof(Password), nameof(Strings.ErrorMessage_ConfirmPassword_Differs), typeof(Strings))]
        [DynamicInput(false, "password", nameof(Strings.DisplayName_ConfirmPassword), typeof(Strings), "input", 1, placeholderResourceName: nameof(Strings.DisplayName_ConfirmPassword))]
        public string ConfirmPassword { get; set; }
    }
}

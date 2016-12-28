using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApi.RequestModels.Account
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), nameof(Strings.ButtonText_SignUp), typeof(Strings))]
    public class SignUpRequestModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Email_Required), typeof(Strings))]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [AsyncValidate(nameof(Strings.ErrorMessage_Email_InUse), typeof(Strings), nameof(AccountController), 
            nameof(AccountController.ValidateEmailNotInUse))]
        [DynamicControl("input", nameof(Strings.DisplayName_Email), typeof(Strings), 0)]
        [DynamicControlProperty("type", "email")]
        public string Email { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_Password_Required), typeof(Strings))]
        [ValidateComplexity(nameof(Strings.ErrorMessage_Password_TooSimple), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_Password), typeof(Strings), 1)]
        [DynamicControlProperty("type", "password")]
        public string Password { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_ConfirmPassword_Required), typeof(Strings))]
        [ValidateMatches(nameof(Password), nameof(Strings.ErrorMessage_ConfirmPassword_Differs), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_ConfirmPassword), typeof(Strings), 2)]
        [DynamicControlProperty("type", "password")]
        public string ConfirmPassword { get; set; }
    }
}

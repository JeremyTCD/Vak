using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Resources;

namespace Jering.VectorArtKit.WebApi.RequestModels.Account
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), nameof(Strings.ButtonText_Submit), typeof(Strings))]
    public class ResetPasswordRequestModel
    {
        [DynamicControl("input", order: 0)]
        [DynamicControlProperty("type", "hidden")]
        public string Email { get; set; }

        [DynamicControl("input", order: 1)]
        [DynamicControlProperty("type", "hidden")]
        public string Token { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_Password_Required), typeof(Strings))]
        [ValidateComplexity(nameof(Strings.ErrorMessage_Password_TooSimple), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_NewPassword), typeof(Strings), 2)]
        [DynamicControlProperty("type", "password")]
        public string NewPassword { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_ConfirmPassword_Required), typeof(Strings))]
        [ValidateMatches(nameof(NewPassword), nameof(Strings.ErrorMessage_ConfirmPassword_Differs), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_ConfirmPassword), typeof(Strings), 3)]
        [DynamicControlProperty("type", "password")]
        public string ConfirmPassword { get; set; }
    }
}

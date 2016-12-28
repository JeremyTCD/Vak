using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Resources;

namespace Jering.VectorArtKit.WebApi.RequestModels.Account
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), nameof(Strings.ButtonText_Submit), typeof(Strings))]
    public class TwoFactorVerifyEmailRequestModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_TwoFactorCode_Required), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_TwoFactorCode), typeof(Strings), 0)]
        [DynamicControlProperty("type", "text")]
        public string Code { get; set; }
    }
}

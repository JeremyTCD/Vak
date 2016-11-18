using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), typeof(Strings))]
    public class LogInFormModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Email_Required), typeof(Strings))]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_Email), typeof(Strings), 0)]
        [DynamicControlProperty("type", "email")]
        public string Email { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_Password_Required), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_Password), typeof(Strings), 1)]
        [DynamicControlProperty("type", "password")]
        public string Password { get; set; }

        [DynamicControl("input", nameof(Strings.DisplayName_RememberMe), typeof(Strings), 2)]
        [DynamicControlProperty("type", "checkbox")]
        public bool RememberMe { get; set; }
    }
}

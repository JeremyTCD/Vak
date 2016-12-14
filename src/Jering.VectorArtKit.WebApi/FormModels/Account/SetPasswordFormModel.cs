using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), nameof(Strings.ButtonText_Submit), typeof(Strings))]
    public class SetPasswordFormModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Password_Required), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_CurrentPassword), typeof(Strings), 0)]
        [DynamicControlProperty("type", "password")]
        public string CurrentPassword { get; set; }

        [ValidateDiffers(nameof(CurrentPassword), nameof(Strings.ErrorMessage_NewPassword_MustDiffer), typeof(Strings))]
        [ValidateRequired(nameof(Strings.ErrorMessage_NewPassword_Required), typeof(Strings))]
        [ValidateComplexity(nameof(Strings.ErrorMessage_Password_TooSimple), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_NewPassword), typeof(Strings), 1)]
        [DynamicControlProperty("type", "password")]
        public string NewPassword { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_ConfirmPassword_Required), typeof(Strings))]
        [ValidateMatches(nameof(NewPassword), nameof(Strings.ErrorMessage_ConfirmPassword_Differs), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_ConfirmNewPassword), typeof(Strings), 2)]
        [DynamicControlProperty("type", "password")]
        public string ConfirmNewPassword { get; set; }
    }
}

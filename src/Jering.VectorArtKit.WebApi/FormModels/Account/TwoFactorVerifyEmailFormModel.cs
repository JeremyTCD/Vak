using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), nameof(Strings.ButtonText_Submit), typeof(Strings))]
    public class TwoFactorVerifyEmailFormModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_TwoFactorCode_Required), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_TwoFactorCode), typeof(Strings), 0)]
        [DynamicControlProperty("type", "text")]
        public string Code { get; set; }
    }
}

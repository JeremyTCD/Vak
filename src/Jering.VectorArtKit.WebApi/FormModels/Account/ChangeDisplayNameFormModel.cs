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
    public class ChangeDisplayNameFormModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Password_Required), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_Password), typeof(Strings), 0)]
        [DynamicControlProperty("type", "password")]
        public string Password { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_NewDisplayName_Required), typeof(Strings))]
        [AsyncValidate(nameof(Strings.ErrorMessage_DisplayName_InUse),
            typeof(Strings),
            nameof(DynamicFormsController),
            nameof(DynamicFormsController.ValidateDisplayNameNotInUse))]
        [DynamicControl("input", nameof(Strings.DisplayName_NewDisplayName), typeof(Strings), 1)]
        [DynamicControlProperty("type", "text")]
        public string NewDisplayName { get; set; }
    }
}

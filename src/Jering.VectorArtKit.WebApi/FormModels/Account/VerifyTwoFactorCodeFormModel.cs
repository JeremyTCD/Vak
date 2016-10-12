using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class VerifyTwoFactorCodeFormModel
    {
        [ValidateLength(6, nameof(Strings.ErrorMessage_TwoFactorCode_Invalid), typeof(Strings))]
        [Required]
        public string Code { get; set; }

        public bool IsPersistent { get; set; }
    }
}

using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class VerifyTwoFactorCodeViewModel
    {
        [ValidateLength(6, nameof(StringOptions.VerifyTwoFactorCode_InvalidCode), typeof(StringOptions))]
        [Required]
        public string Code { get; set; }

        public bool IsPersistent { get; set; }
    }
}

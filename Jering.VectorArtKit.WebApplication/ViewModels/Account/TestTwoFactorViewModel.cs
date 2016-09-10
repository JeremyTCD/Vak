using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class TestTwoFactorViewModel
    {
        [ValidateLength(6, nameof(StringOptions.ErrorMessage_TwoFactorCode_Invalid), typeof(StringOptions))]
        [Required]
        public string Code { get; set; }
    }
}

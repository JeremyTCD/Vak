using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class ForgotPasswordFormModel
    {
        [Required]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}

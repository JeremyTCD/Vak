using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class ChangeDisplayNameFormModel
    {
        public string CurrentDisplayName { get; set; }

        [Required]
        [ValidateMinLength(2, nameof(Strings.ErrorMessage_DisplayName_FormatInvalid), typeof(Strings))]
        [ValidateDiffers(nameof(CurrentDisplayName), nameof(Strings.ErrorMessage_NewDisplayName_MustDiffer), typeof(Strings))]
        [Display(Name = "New display name")]
        public string NewDisplayName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

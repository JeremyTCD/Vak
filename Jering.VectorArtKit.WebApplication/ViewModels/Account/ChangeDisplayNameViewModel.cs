using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class ChangeDisplayNameViewModel
    {
        public string CurrentDisplayName { get; set; }

        [Required]
        [ValidateMinLength(2, nameof(StringOptions.ErrorMessage_DisplayName_FormatInvalid), typeof(StringOptions))]
        [ValidateDiffers(nameof(CurrentDisplayName), nameof(StringOptions.ErrorMessage_NewDisplayName_MustDiffer), typeof(StringOptions))]
        [Display(Name = "New display name")]
        public string NewDisplayName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

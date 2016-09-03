using Jering.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class ConfirmEmailViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [ValidateEmailAddress]
        public string Email { get; set; }
    }
}

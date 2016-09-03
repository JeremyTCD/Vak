using Jering.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class EmailVerificationConfirmationViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public int AccountId { get; set; }
    }
}

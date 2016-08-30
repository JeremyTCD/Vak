using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels
{
    public class VerifyCodeViewModel
    {
        //TODO: validate as two factor code
        [Required]
        public string Code { get; set; }

        [Display(Name = "Remember me?")]
        public bool IsPersistent { get; set; }
    }
}

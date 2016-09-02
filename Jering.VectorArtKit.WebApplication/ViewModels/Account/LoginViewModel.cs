using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class LoginViewModel 
    {
        [Required]
        [ValidateEmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [ValidateMinLength(8)]
        [ValidateHasLowercase]
        [ValidateHasUppercase]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

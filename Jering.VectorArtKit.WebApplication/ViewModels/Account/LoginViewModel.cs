using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels
{
    public class LoginViewModel 
    {
        [Required]
        [ValidateAsEmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [ValidateAsPassword]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

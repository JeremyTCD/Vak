using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [ValidateAsEmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [ValidateAsPassword]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        [ValidateAsConfirmPassword(nameof(Password))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}

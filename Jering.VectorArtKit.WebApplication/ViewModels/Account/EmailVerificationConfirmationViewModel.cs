using System.ComponentModel.DataAnnotations;

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

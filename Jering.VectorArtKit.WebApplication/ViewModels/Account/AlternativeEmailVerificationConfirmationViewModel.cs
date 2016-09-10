using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Account
{
    public class AlternativeEmailVerificationConfirmationViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public int AccountId { get; set; }
    }
}

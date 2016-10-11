using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.FormModels
{
    public class EmailVerificationConfirmationFormModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public int AccountId { get; set; }
    }
}

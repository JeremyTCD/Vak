using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.FormModels
{
    public class AlternativeEmailVerificationConfirmationFormModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public int AccountId { get; set; }
    }
}

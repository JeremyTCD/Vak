using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    public class EmailVerificationConfirmationFormModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public int AccountId { get; set; }
    }
}

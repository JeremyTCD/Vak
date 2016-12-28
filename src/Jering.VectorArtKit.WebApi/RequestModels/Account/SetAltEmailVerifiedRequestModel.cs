using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;

namespace Jering.VectorArtKit.WebApi.RequestModels.Account
{
    public class SetAltEmailVerifiedRequestModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Token_Required), typeof(Strings))]
        public string Token { get; set; }
    }
}

using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;

namespace Jering.VectorArtKit.WebApi.RequestModels.Account
{
    public class CheckInUseRequestModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Value_Required), typeof(Strings))]
        public string Value { get; set; }
    }
}

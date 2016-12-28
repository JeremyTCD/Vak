using Jering.DataAnnotations;
using Jering.VectorArtKit.WebApi.Resources;

namespace Jering.VectorArtKit.WebApi.RequestModels.Shared
{
    public class ValidateValueRequestModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Value_Required), typeof(Strings))]
        public string Value { get; set; }
    }
}

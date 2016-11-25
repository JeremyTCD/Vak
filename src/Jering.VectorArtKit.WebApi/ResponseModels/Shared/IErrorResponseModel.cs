using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Shared
{
    public interface IErrorResponseModel
    {
        bool ExpectedError { get; set; }
        string ErrorMessage { get; set; }
        SerializableError ModelState { get; set; }
    }
}

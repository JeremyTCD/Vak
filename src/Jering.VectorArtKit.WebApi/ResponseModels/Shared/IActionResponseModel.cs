using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Shared
{
    public interface IActionResponseModel
    {
        SerializableError ModelState { get; set; }
    }
}

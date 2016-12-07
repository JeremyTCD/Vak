using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class ChangeEmailResponseModel: IErrorResponseModel
    {
        public SerializableError ModelState { get; set; }

        public bool ExpectedError { get; set; }

        public string ErrorMessage { get; set; }
    }
}

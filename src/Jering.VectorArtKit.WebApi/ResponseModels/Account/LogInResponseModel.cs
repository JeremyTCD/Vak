using Jering.Mvc;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class LogInResponseModel: IErrorResponseModel
    {
        public bool AuthenticationError { get; set; }
        public bool AntiForgeryError { get; set; }
        public bool TwoFactorRequired { get; set; }
        public SerializableError ModelState { get; set; }
        public bool ExpectedError { get; set; }

        public string ErrorMessage { get; set; }
    }
}

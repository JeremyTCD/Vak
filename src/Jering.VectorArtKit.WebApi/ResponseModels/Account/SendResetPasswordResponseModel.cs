using System;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class SendResetPasswordResponseModel: IErrorResponseModel, IActionResponseModel
    {
        public SerializableError ModelState { get; set; }

        public bool ExpectedError { get; set; }

        public string ErrorMessage { get; set; }
    }
}

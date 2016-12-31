using System;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Jering.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class TwoFactorLogInResponseModel: IErrorResponseModel
{
public bool AuthenticationError { get; set; }
public bool AntiForgeryError { get; set; }

        public bool ExpiredCredentials { get; set; }
        public SerializableError ModelState { get; set; }

        public bool ExpectedError { get; set; }

        public string ErrorMessage { get; set; }
    }
}

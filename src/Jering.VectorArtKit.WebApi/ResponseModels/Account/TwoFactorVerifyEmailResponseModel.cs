using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class TwoFactorVerifyEmailResponseModel: IErrorResponseModel
    {
        public SerializableError ModelState { get; set; }
        public bool ExpectedError { get; set; }

        public string ErrorMessage { get; set; }
    }
}

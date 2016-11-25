﻿using System;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class TwoFactorLogInResponseModel: IErrorResponseModel, IActionResponseModel
    {
        public string Username { get; set; }
        public bool IsPersistent { get; set; }
        public bool TokenExpired { get; set; }
        public SerializableError ModelState { get; set; }

        public bool ExpectedError { get; set; }

        public string ErrorMessage { get; set; }
    }
}

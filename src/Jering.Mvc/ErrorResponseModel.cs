using System;
using Microsoft.AspNetCore.Mvc;

namespace Jering.Mvc
{
    public class ErrorResponseModel: IErrorResponseModel
    {
        public bool ExpectedError { get; set; }
        public string ErrorMessage { get; set; }
        public bool AuthenticationError { get; set; }
        public bool AntiForgeryError { get; set; }

        public SerializableError ModelState { get; set; }
    }
}

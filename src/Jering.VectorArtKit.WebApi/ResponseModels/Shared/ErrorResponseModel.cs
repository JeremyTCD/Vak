using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Shared
{
    public class ErrorResponseModel
    {
        public bool ExpectedError { get; set; }
        public SerializableError ModelState { get; set; }
        public string ErrorMessage { get; set; }
    }
}

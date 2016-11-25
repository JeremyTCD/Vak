using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Shared
{
    public interface IErrorResponseModel
    {
        bool ExpectedError { get; set; }
        string ErrorMessage { get; set; }
    }
}

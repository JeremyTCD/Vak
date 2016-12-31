using Microsoft.AspNetCore.Mvc;

namespace Jering.Mvc
{
    public interface IErrorResponseModel
    {
        bool ExpectedError { get; set; }
        string ErrorMessage { get; set; }
        bool AuthenticationError { get; set; }
        bool AntiForgeryError { get; set; }
        SerializableError ModelState { get; set; }
    }
}

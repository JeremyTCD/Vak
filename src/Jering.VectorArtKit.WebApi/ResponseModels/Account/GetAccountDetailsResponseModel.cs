using Jering.Mvc;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class GetAccountDetailsResponseModel: IErrorResponseModel
    {
        public string DurationSinceLastPasswordChange { get; set; }
        public string DisplayName { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string AltEmail { get; set; }
        public bool AltEmailVerified { get; set; }
        public SerializableError ModelState { get; set; }
        public bool ExpectedError { get; set; }

        public string ErrorMessage { get; set; }
        public bool AuthenticationError { get; set; }
        public bool AntiForgeryError { get; set; }
    }
}

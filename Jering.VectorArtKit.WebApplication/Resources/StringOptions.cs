using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Shared
{
    public class StringOptions
    {
        public string Password_Invalid { get; set; } = "Password must be at least 8 characters long. Password must include both upper and lower case characters.";

        public string ConfirmPassword_DoesNotMatchPassword { get; set; } = "Confirmation password does not match password.";

        public string Email_Invalid { get; set; } = "Email address is invalid.";

        public string Login_Failed { get; set; } = "Invalid email or password.";

        public string SignUp_AccountWithEmailExists { get; set; } = "An account with this email already exists.";

        public string VerifyTwoFactorCode_InvalidCode { get; set; } = "Invalid code.";

        public string ConfirmEmail_Subject { get; set; } = "Confirm email";
        public string ConfirmEmail_Message { get; set; } = "Please confirm your email by clicking this link: <a href=\"{0}\">link</a>";

        public string TwoFactorEmail_Subject { get; set; } = "Two factor code";
        public string TwoFactorEmail_Message { get; set; } = "Your code is: {0}";
    }
}

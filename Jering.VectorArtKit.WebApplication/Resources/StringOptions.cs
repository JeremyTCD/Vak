using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.Resources
{
    public class StringOptions
    {
        public string Password_Invalid { get; set; } = "Password must be at least 8 characters long. Password must include both upper and lower case characters.";

        public string ConfirmPassword_DoesNotMatchPassword { get; set; } = "Confirmation password does not match password.";

        public string Email_Invalid { get; set; } = "Email address is invalid.";

        public string LogInView_Title { get; set; } = "Log in";
        public string LogIn_Failed { get; set; } = "Invalid email or password.";

        public string SignUpView_Title { get; set; } = "Sign up";
        public string SignUp_AccountWithEmailExists { get; set; } = "An account with this email already exists.";

        public string VerifyTwoFactorCodeView_Title { get; set; } = "Verify two factor code";
        public string VerifyTwoFactorCode_InvalidCode { get; set; } = "Invalid code.";

        public string ConfirmEmail_Subject { get; set; } = "Confirm email";
        public string ConfirmEmail_Message { get; set; } = "Please confirm your email by clicking this link: <a href=\"{0}\">link</a>";

        public string TwoFactorEmail_Subject { get; set; } = "Two factor code";
        public string TwoFactorEmail_Message { get; set; } = "Your code is: {0}";

        public string SendTwoFactorCode_Relog { get; set; } = "An error occurred. Please log in again.";
        public string SendTwoFactorCode_EmailSent { get; set; } = "An email has been sent to {0}.";

        public string ForgotPasswordView_Title { get; set; } = "Forgot password";

        public string ForgotPasswordConfirmationView_Title { get; set; } = "Forgot password confirmation";

        public string ResetPasswordView_Title { get; set; } = "Reset password";
        public string ResetPasswordEmail_Subject { get; set; } = "Reset password";
        public string ResetPasswordEmail_Message { get; set; } = "Please reset your email at this link: <a href=\"{0}\">link</a>";

        public string ResetPasswordConfirmationView_Title { get; set; } = "Reset password confirmation";

        public string ErrorView_Title { get; set; } = "Error";
    }
}

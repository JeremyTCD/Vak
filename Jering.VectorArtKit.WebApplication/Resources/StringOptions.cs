namespace Jering.VectorArtKit.WebApplication.Resources
{
    public class StringOptions
    {
        #region View titles
        public string ViewTitle_LogIn { get; set; } = "Log in";
        public string ViewTitle_SignUp { get; set; } = "Sign up";
        public string ViewTitle_VerifyTwoFactorCode { get; set; } = "Verify two factor code";
        public string ViewTitle_ForgotPassword { get; set; } = "Forgot password";
        public string ViewTitle_ForgotPasswordConfirmation { get; set; } = "Forgot password confirmation";
        public string ViewTitle_ResetPassword { get; set; } = "Reset password";
        public string ViewTitle_ResetPasswordConfirmation { get; set; } = "Reset password confirmation";
        public string ViewTitle_EmailVerificationConfirmation { get; set; } = "Email verification confirmation";
        public string ViewTitle_Error { get; set; } = "Error";
        public string ViewTitle_ManageAccount { get; set; } = "Manage account";
        public string ViewTitle_ChangePassword { get; set; } = "Change password";
        public string ViewTitle_ChangeAlternativeEmail { get; set; } = "Change alternative email";
        public string ViewTitle_ChangeDisplayName { get; set; } = "Change display name";
        public string ViewTitle_ChangeEmail { get; set; } = "Change email";
        public string ViewTitle_EnableTwoFactor { get; set; } = "Enable two factor";
        public string ViewTitle_EnableTwoFactorConfirmation { get; set; } = "Enable two factor confirmation";
        public string ViewTitle_DisableTwoFactorConfirmation { get; set; } = "Disable two factor confirmation";
        public string ViewTitle_SendEmailVerificationConfirmation { get; set; } = "Send email verification confirmation";
        #endregion

        #region Email
        public string ConfirmEmail_Subject { get; set; } = "Confirm email";
        public string TwoFactorEmail_Subject { get; set; } = "Two factor code";
        public string TwoFactorEmail_Message { get; set; } = "Your code is: {0}";
        public string SendTwoFactorCode_EmailSent { get; set; } = "An email has been sent to {0}.";
        public string ResetPasswordEmail_Subject { get; set; } = "Reset password";
        public string ResetPasswordEmail_Message { get; set; } = "Please reset your email at this link: <a href=\"{0}\">link</a>";
        #endregion

        #region Validation error messages
        public string Password_Invalid { get; set; } = "Password must be at least 8 characters long. Password must include both upper and lower case characters.";
        public string ConfirmPassword_DoesNotMatchPassword { get; set; } = "Confirmation password does not match password.";
        public string Email_Invalid { get; set; } = "Email address is invalid.";
        public string LogIn_Failed { get; set; } = "Invalid email or password.";
        public string SignUp_AccountWithEmailExists { get; set; } = "An account with this email already exists.";
        public string VerifyTwoFactorCode_InvalidCode { get; set; } = "Invalid code.";
        public string ConfirmEmail_Message { get; set; } = "Please confirm your email by clicking this link: <a href=\"{0}\">link</a>";
        public string ErrorMessage_InvalidCurrentPassword { get; set; } = "Invalid current password";
        #endregion

        #region Display names
        public string DisplayName_Password = "Password";
        public string DisplayName_Email = "Email address";
        public string DisplayName_TwoFactor = "Two factor authentication";
        public string DisplayName_DisplayName = "Display name";
        public string DisplayName_AlternativeEmail = "Alternative email address";
        #endregion
    }
}

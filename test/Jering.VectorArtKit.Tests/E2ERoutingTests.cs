using Jering.VectorArtKit.EndToEnd.Tests;
using OpenQA.Selenium;
using System;
using Xunit;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using Dapper;
using System.Text.RegularExpressions;

namespace Jering.VectorArtKit.Tests.EndToEnd
{
    [Collection("E2ECollection")]
    public class E2ERoutingTests
    {
        private int _waitTime = 5000;
        private const string _baseUrl = "http://localhost:4200/";
        private const string _testEmail = "test@email.com";
        private const string _testNewEmail = "testNew@email.com";
        private const string _testAltEmail = "testAlternative@email.com";
        private const string _testPassword = "testPassword";
        private const string _testNewPassword = "testNewPassword";
        private const string _testNewDisplayName = "testNewDisplayName";
        private string _emailFileName = $"{Environment.GetEnvironmentVariable("TMP")}\\SmtpTest.txt";
        private const string _homeRelativeUrl = "home";
        private const string _signUpRelativeUrl = "sign-up";
        private const string _logInRelativeUrl = "log-in";
        private const string _forgotPasswordRelativeUrl = "log-in/forgot-password";
        private const string _resetPasswordRelativeUrl = "log-in/reset-password";
        private const string _manageAccountRelativeUrl = "manage-account";
        private const string _changeAltEmailRelativeUrl = "manage-account/change-alt-email";
        private const string _changeEmailRelativeUrl = "manage-account/change-email";
        private const string _twoFactorVerifyEmailRelativeUrl = "manage-account/two-factor-verify-email";
        private const string _changeDisplayNameRelativeUrl = "manage-account/change-display-name";
        private const string _changePasswordRelativeUrl = "manage-account/change-password";
        private const string _verifyEmailRelativeUrl = "manage-account/verify-email";
        private const string _verifyAltEmailRelativeUrl = "manage-account/verify-alt-email";

        private ReusableWebDriver _webDriver { get; }
        private SqlConnection _sqlConnection { get; }
        private Action _resetAccountsTable { get; }

        public E2ERoutingTests(E2EFixture fixture)
        {
            _webDriver = fixture.WebDriver;
            _sqlConnection = fixture.SqlConnection;
            _resetAccountsTable = fixture.ResetAccountsTable;
            _resetAccountsTable();
        }

        #region Routes
        [Fact]
        public void SignUp_SignUpLogsIntoAccountAndNavigatesToHome()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();

            // Act and Assert
            SignUp(_testEmail, _testPassword, _testPassword);
        }

        [Fact]
        public void SignUp_LogInNavigatesToLogIn()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Log in']")).Click();
            
            // Assert
            WaitNavigateToLogIn();
        }

        [Fact]
        public void LogOffLogsOutOfAccountAndNavigatesToHome()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);

            // Act and Assert
            LogOff(_testEmail);
        }

        [Fact]
        public void LogIn_LogInLogsInToAccountAndNavigatesToReturnUrl()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
            NavigateToLogIn();
            WaitNavigateToLogIn();

            // Act and Assert
            LogIn(_testEmail, _testPassword, _baseUrl + _homeRelativeUrl);
        }

        [Fact]
        public void LogIn_ForgotPasswordNavigatesToForgotPassword()
        {
            // Arrange
            NavigateToLogIn();
            WaitNavigateToLogIn();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Forgot password']")).Click();

            // Assert
            WaitNavigateToForgotPassword();
        }

        [Fact]
        public void LogIn_SignUpNavigatesToSignUp()
        {
            // Arrange
            NavigateToLogIn();
            WaitNavigateToLogIn();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Sign up']")).Click();

            // Assert
            WaitNavigateToSignUp();
        }

        [Fact]
        public void ForgotPassword_SubmitSendsEmailAndDisplaysConfirmationMessage()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
            NavigateToForgotPassword();
            WaitNavigateToForgotPassword();

            // Act and Assert
            ClearEmailFile();
            ForgotPassword(_testEmail);
            Assert.False(string.IsNullOrEmpty(ReadEmailFile()));
        }

        [Fact]
        public void ForgotPassword_ReturnToLogInNavigatesToLogIn()
        {
            // Arrange
            NavigateToForgotPassword();
            WaitNavigateToForgotPassword();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Return to log in']")).Click();

            // Assert
            WaitNavigateToLogIn();
        }

        [Fact]
        public void ResetPassword_SubmitResetsPasswordAndDisplaysConfirmationMessage()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
            NavigateToForgotPassword();
            WaitNavigateToForgotPassword();
            ClearEmailFile();
            ForgotPassword(_testEmail);
            string email = ReadEmailFile();
            string resetPasswordUrl = Regex.Match(email, "href=\"(.*?)\"").Groups[1].Value;
            NavigateToResetPassword(resetPasswordUrl);
            WaitNavigateToResetPassword();

            // Act
            ResetPassword(_testNewPassword);

            // Assert
            NavigateToLogIn();
            WaitNavigateToLogIn();
            LogIn(_testEmail, _testNewPassword, _baseUrl + _homeRelativeUrl);
        }

        [Fact]
        public void ManageAccount_DisplaysAccountDetails()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);

            // Act
            NavigateToManageAccount();

            // Assert
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void ManageAccount_ChangePasswordNavigatesToChangePassword()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Change password']")).Click();

            // Assert
            WaitNavigateToChangePassword();
        }

        [Fact]
        public void ManageAccount_ChangeEmailAddressNavigatesToChangeEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Change email address']")).Click();

            // Assert
            WaitNavigateToChangeEmail();
        }

        [Fact]
        public void ManageAccount_EmailSendEmailVerificationEmailSendsVerificationEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();
            ClearEmailFile();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Send verification email']")).Click();

            // Assert
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//div[contains(text(),'Email verification email sent')]")).Count == 1, _waitTime);
            Assert.False(string.IsNullOrEmpty(ReadEmailFile()));
        }

        [Fact]
        public void ManageAccount_ChangeAltEmailAddressNavigatesToChangeAltEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Change alt email address']")).Click();

            // Assert
            WaitNavigateToChangeAltEmail();
        }

        [Fact]
        public void ManageAccount_AltEmailSendEmailVerificationEmailSendsVerificationEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeAltEmail();
            WaitNavigateToChangeAltEmail();
            ChangeAltEmail(_testPassword, _testAltEmail);
            ClearEmailFile();

            // Act
            _webDriver.FindElement(By.XPath("//h3[text()='Alternative email address']/..//a[contains(text(), 'Send verification email')]")).Click();

            // Assert
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//div[contains(text(),'Email verification email sent')]")).Count == 1, _waitTime);
            // Should check email subject. Not possible until selenium is .net core compatible and web api project can be referenced.
            string email = ReadEmailFile();
            Assert.False(string.IsNullOrEmpty(email));
        }

        [Fact]
        public void ManageAccount_ChangeDisplayNameAddressNavigatesToChangeDisplayName()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Change display name']")).Click();

            // Assert
            WaitNavigateToChangeDisplayName();
        }

        [Fact]
        public void ManageAccount_EnableTwoFactorAuthNavigatesToTwoFactorVerifyEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Enable two factor auth']")).Click();

            // Assert
            WaitNavigateToTwoFactorVerifyEmail();
        }

        [Fact]
        public void ManageAccount_DisableTwoFactorAuthDisablesTwoFactorAuth()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();
            ClearEmailFile();
            _webDriver.FindElement(By.XPath("//a[text()='Enable two factor auth']")).Click();
            WaitNavigateToTwoFactorVerifyEmail();
            string email = ReadEmailFile();
            string code = Regex.Match(email, "Your code is: (\\d{6,6})").Groups[1].Value;
            TwoFactorVerifyEmail(code);

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Disable two factor auth']")).Click();

            // Assert
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//div[contains(text(),'Two factor auth disabled')]")).Count == 1, _waitTime);
        }

        [Fact]
        public void ChangeAltEmail_SubmitChangesAltEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeAltEmail();
            WaitNavigateToChangeAltEmail();

            // Act and Assert
            ChangeAltEmail(_testPassword, _testAltEmail);
        }

        [Fact]
        public void ChangeAltEmail_CancelNavigatesToManageAccount()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeAltEmail();
            WaitNavigateToChangeAltEmail();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Cancel']")).Click();

            // Assert
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void ChangeEmail_SubmitChangesEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeEmail();
            WaitNavigateToChangeEmail();

            // Act and Assert
            ChangeEmail(_testPassword, _testNewEmail);
        }

        [Fact]
        public void ChangeEmail_CancelNavigatesToManageAccount()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeEmail();
            WaitNavigateToChangeEmail();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Cancel']")).Click();

            // Assert
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void ChangeDisplayName_SubmitChangesDisplayName()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeDisplayName();
            WaitNavigateToChangeDisplayName();

            // Act and Assert
            ChangeDisplayName(_testPassword, _testNewDisplayName);
        }

        [Fact]
        public void ChangeDisplayName_CancelNavigatesToManageAccount()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeDisplayName();
            WaitNavigateToChangeDisplayName();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Cancel']")).Click();

            // Assert
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void ChangePassword_SubmitChangesPassword()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangePassword();
            WaitNavigateToChangePassword();

            // Act
            ChangePassword(_testPassword, _testNewPassword);

            // Assert
            LogOff(_testEmail);
            NavigateToLogIn();
            WaitNavigateToLogIn();
            LogIn(_testEmail, _testNewPassword, _homeRelativeUrl);
        }

        [Fact]
        public void ChangePassword_CancelNavigatesToManageAccount()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangePassword();
            WaitNavigateToChangePassword();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Cancel']")).Click();

            // Assert
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void TwoFactorVerifyEmail_SubmitVerifiesEmailAndEnablesTwoFactorAuth()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();
            ClearEmailFile();
            _webDriver.FindElement(By.XPath("//a[text()='Enable two factor auth']")).Click();
            WaitNavigateToTwoFactorVerifyEmail();
            string email = ReadEmailFile();
            string code = Regex.Match(email, "Your code is: (\\d{6,6})").Groups[1].Value;

            // Act and Assert
            TwoFactorVerifyEmail(code);
        }

        [Fact]
        public void TwoFactorVerifyEmail_CancelNavigatesToManageAccount()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToTwoFactorVerifyEmail();
            WaitNavigateToTwoFactorVerifyEmail();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Cancel']")).Click();

            // Assert
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void VerifyEmail_VerifiesEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToManageAccount();
            WaitNavigateToManageAccount();
            ClearEmailFile();
            _webDriver.FindElement(By.XPath("//a[text()='Send verification email']")).Click();
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//div[contains(text(),'Email verification email sent')]")).Count == 1, _waitTime);
            string email = ReadEmailFile();
            string link = Regex.Match(email, "href=\"(.*?)\"").Groups[1].Value;

            // Act 
            NavigateToVerifyEmail(link);
            WaitNavigateToVerifyEmail();

            // Assert
            NavigateToManageAccount();
            WaitNavigateToManageAccount();
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//div[contains(text(),'Email address verified')]")).Count == 1, _waitTime);
        }

        [Fact]
        public void VerifyEmail_GetANewLinkNavigatesToManageAccount()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToVerifyEmail();
            WaitNavigateToVerifyEmail();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Get a new link']")).Click();

            // Assert
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void VerifyAltEmail_VerifiesAltEmail()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToChangeAltEmail();
            WaitNavigateToChangeAltEmail();
            ChangeAltEmail(_testPassword, _testAltEmail);
            WaitNavigateToManageAccount();
            ClearEmailFile();
            _webDriver.FindElement(By.XPath("//h3[text()='Alternative email address']/..//a[contains(text(), 'Send verification email')]")).Click();
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//div[contains(text(),'Email verification email sent')]")).Count == 1, _waitTime);
            string email = ReadEmailFile();
            string link = Regex.Match(email, "href=\"(.*?)\"").Groups[1].Value;

            // Act 
            NavigateToVerifyAltEmail(link);
            WaitNavigateToVerifyAltEmail();

            // Assert
            NavigateToManageAccount();
            WaitNavigateToManageAccount();
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//div[contains(text(),'Alternative email address verified')]")).Count == 1, _waitTime);
        }

        [Fact]
        public void VerifyAltEmail_GetANewLinkNavigatesToManageAccount()
        {
            // Arrange
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            NavigateToVerifyAltEmail();
            WaitNavigateToVerifyAltEmail();

            // Act
            _webDriver.FindElement(By.XPath("//a[text()='Get a new link']")).Click();

            // Assert
            WaitNavigateToManageAccount();
        }
        #endregion

        #region Guards
        [Fact]
        public void HttpServiceRedirectsToLogInIfServerSendsUnauthorizedHttpResponse()
        {
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
            // mimic invalid cookie situation (auth.guard passes but server returns 401)
            _webDriver.ExecuteScript("window.localStorage.clear()");
            _webDriver.ExecuteScript("window.localStorage['vakUsername'] = 'dummyUsername'");
            NavigateToManageAccount();
            WaitNavigateToLogIn();
            LogIn(_testEmail, _testPassword, _manageAccountRelativeUrl);
            WaitNavigateToManageAccount();
        }

        [Fact]
        public void AuthGuardRedirectsToLogInIfNotLoggedIn()
        {
            NavigateToSignUp();
            WaitNavigateToSignUp();
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);

            NavigateToManageAccount();
            WaitNavigateToLogIn();
            LogIn(_testEmail, _testPassword, _manageAccountRelativeUrl);
            WaitNavigateToManageAccount();
        }
        #endregion

        #region Helpers

        public void NavigateToSignUp()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _signUpRelativeUrl);
        }

        public void WaitNavigateToSignUp()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("email")).Count > 0, _waitTime);
        }

        public void SignUp(string email, string password, string confirmPassword)
        {
            _webDriver.FindElement(By.Id("email")).SendKeys(email);
            _webDriver.FindElement(By.Id("password")).SendKeys(password);
            _webDriver.FindElement(By.Id("confirmPassword")).SendKeys(password);

            _webDriver.FindElement(By.XPath("//button[text()='Sign up']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_homeRelativeUrl) &&
                wd.FindElements(By.LinkText($"Account:{email}")).Count == 1, _waitTime);
        }

        public void LogOff(string email)
        {
            _webDriver.FindElement(By.LinkText("Log off")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_homeRelativeUrl) &&
                wd.FindElements(By.LinkText($"Account:{email}")).Count == 0, _waitTime);
        }

        public void NavigateToLogIn()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _logInRelativeUrl);
        }

        public void WaitNavigateToLogIn()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("email")).Count > 0, _waitTime);
        }

        public void LogIn(string email, string password, string returnRelativeUrl)
        {
            _webDriver.FindElement(By.Id("email")).SendKeys(email);
            _webDriver.FindElement(By.Id("password")).SendKeys(password);

            _webDriver.FindElement(By.XPath("//button[text()='Log in']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(returnRelativeUrl) &&
                wd.FindElements(By.LinkText($"Account:{email}")).Count == 1, _waitTime);
        }

        public void NavigateToForgotPassword()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _forgotPasswordRelativeUrl);
        }

        public void WaitNavigateToForgotPassword()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("email")).Count > 0, _waitTime);
        }

        public void ForgotPassword(string email)
        {
            _webDriver.FindElement(By.Id("email")).SendKeys(email);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.FindElements(By.LinkText("Did not receive email help")).Count == 1, _waitTime);
        }

        public void NavigateToResetPassword(string resetPasswordUrl = null)
        {
            _webDriver.Navigate().GoToUrl(resetPasswordUrl == null ? _baseUrl + _resetPasswordRelativeUrl : resetPasswordUrl);
        }

        public void WaitNavigateToResetPassword()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("newPassword")).Count > 0, _waitTime);
        }

        public void ResetPassword(string newPassword)
        {
            _webDriver.FindElement(By.Id("newPassword")).SendKeys(newPassword);
            _webDriver.FindElement(By.Id("confirmPassword")).SendKeys(newPassword);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.FindElements(By.LinkText("Log in")).Count == 1, _waitTime);
        }

        public void NavigateToManageAccount()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _manageAccountRelativeUrl);
        }

        public void WaitNavigateToManageAccount()
        {
            _webDriver.Wait(wd => wd.FindElements(By.XPath("//h2[text()='Manage Account']")).Count == 1, _waitTime);
        }

        public void NavigateToChangeAltEmail()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changeAltEmailRelativeUrl);
        }

        public void WaitNavigateToChangeAltEmail()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("password")).Count > 0, _waitTime);
        }

        public void ChangeAltEmail(string password, string newAltEmail)
        {
            _webDriver.FindElement(By.Id("password")).SendKeys(password);
            _webDriver.FindElement(By.Id("newAltEmail")).SendKeys(newAltEmail);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_baseUrl + _manageAccountRelativeUrl) &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Alternative email address: {newAltEmail}')]")).Count == 1, _waitTime);
        }

        public void NavigateToChangeEmail()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changeEmailRelativeUrl);
        }

        public void WaitNavigateToChangeEmail()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("password")).Count > 0, _waitTime);
        }

        public void ChangeEmail(string password, string newEmail)
        {
            _webDriver.FindElement(By.Id("password")).SendKeys(password);
            _webDriver.FindElement(By.Id("newEmail")).SendKeys(newEmail);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_baseUrl + _manageAccountRelativeUrl) &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Email address: {newEmail}')]")).Count == 1, _waitTime);
        }

        public void NavigateToChangeDisplayName()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changeDisplayNameRelativeUrl);
        }

        public void WaitNavigateToChangeDisplayName()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("password")).Count > 0, _waitTime);
        }

        public void ChangeDisplayName(string password, string newDisplayName)
        {
            _webDriver.FindElement(By.Id("password")).SendKeys(password);
            _webDriver.FindElement(By.Id("newDisplayName")).SendKeys(newDisplayName);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_baseUrl + _manageAccountRelativeUrl) &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Display name: {newDisplayName}')]")).Count == 1, _waitTime);
        }

        public void NavigateToChangePassword()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changePasswordRelativeUrl);
        }

        public void WaitNavigateToChangePassword()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("currentPassword")).Count > 0, _waitTime);
        }

        public void ChangePassword(string password, string newPassword)
        {
            _webDriver.FindElement(By.Id("currentPassword")).SendKeys(password);
            _webDriver.FindElement(By.Id("newPassword")).SendKeys(newPassword);
            _webDriver.FindElement(By.Id("confirmNewPassword")).SendKeys(newPassword);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.FindElements(By.XPath("//h2[text()='Manage Account']")).Count == 1, _waitTime);
        }

        public void NavigateToTwoFactorVerifyEmail()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _twoFactorVerifyEmailRelativeUrl);
        }

        public void WaitNavigateToTwoFactorVerifyEmail()
        {
            _webDriver.Wait(wd => wd.FindElements(By.Id("code")).Count > 0, _waitTime);
        }

        public void TwoFactorVerifyEmail(string code)
        {
            _webDriver.FindElement(By.Id("code")).SendKeys(code);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_baseUrl + _manageAccountRelativeUrl) &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Two factor auth enabled')]")).Count == 1 &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Email address verified')]")).Count == 1, _waitTime);
        }

        public void NavigateToVerifyEmail(string link = null)
        {
            _webDriver.Navigate().GoToUrl(link == null ? _baseUrl + _verifyEmailRelativeUrl : link);
        }

        public void WaitNavigateToVerifyEmail()
        {
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//h2[contains(text(),'Verify email')]")).Count > 0, _waitTime);
        }

        public void NavigateToVerifyAltEmail(string link = null)
        {
            _webDriver.Navigate().GoToUrl(link == null ? _baseUrl + _verifyAltEmailRelativeUrl : link);
        }

        public void WaitNavigateToVerifyAltEmail()
        {
            _webDriver.Wait(wd => wd.FindElements(By.XPath($"//h2[contains(text(),'Verify alternative email')]")).Count > 0, _waitTime);
        }

        public void ResetAccountsTable()
        {
            _sqlConnection.Execute("Delete from [dbo].[Accounts];" +
               "DBCC CHECKIDENT('[dbo].[Accounts]', RESEED, 0);", commandType: CommandType.Text);
        }

        public void ClearEmailFile()
        {
            File.WriteAllText(_emailFileName, "");
        }

        public string ReadEmailFile()
        {
            return File.ReadAllText(_emailFileName);
        }

        #endregion
    }
}



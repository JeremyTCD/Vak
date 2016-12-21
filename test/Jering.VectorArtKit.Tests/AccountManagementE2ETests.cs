using Jering.VectorArtKit.EndToEnd.Tests;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium.Interactions;

namespace Jering.VectorArtKit.Tests.EndToEnd
{
    [Collection("E2ECollection")]
    public class AccountManagementE2ETests
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
        private const string _changeDisplayNameRelativeUrl = "manage-account/change-display-name";
        private const string _changePasswordRelativeUrl = "manage-account/change-password";

        private ReusableWebDriver _webDriver { get; }
        private SqlConnection _sqlConnection { get; }
        private Action _resetAccountsTable { get; }

        public AccountManagementE2ETests(E2EFixture fixture)
        {
            _webDriver = fixture.WebDriver;
            _sqlConnection = fixture.SqlConnection;
            _resetAccountsTable = fixture.ResetAccountsTable;
            _resetAccountsTable();
        }

        [Fact]
        public void SignUpSendsEmailLogsIntoAccountAndNavigatesToHome()
        {
            ClearEmailFile();

            SignUp(_testEmail, _testPassword, _testPassword);

            Assert.False(string.IsNullOrEmpty(ReadEmailFile()));
        }

        [Fact]
        public void LogOffLogsOutOfAccountAndNavigatesToHome()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
        }

        [Fact]
        public void LogInLogsInToAccountAndNavigatesToReturnUrl()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
            LogIn(_testEmail, _testPassword, _baseUrl + _homeRelativeUrl);
        }

        [Fact]
        public void ForgotPasswordSendsEmailAndDisplaysConfirmationMessage()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);

            ClearEmailFile();

            ForgotPassword(_testEmail);

            Assert.False(string.IsNullOrEmpty(ReadEmailFile()));
        }

        [Fact]
        public void ResetPasswordResetsPasswordAndDisplaysConfirmationMessage()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
            ForgotPassword(_testEmail);

            string email = ReadEmailFile();
            string resetPasswordUrl = Regex.Match(email, "href=\"(.*?)\"").Groups[1].Value;

            ResetPassword(_testNewPassword, resetPasswordUrl);
            LogIn(_testEmail, _testNewPassword, _baseUrl + _homeRelativeUrl);
        }

        [Fact]
        public void ManageAccountDisplaysAccountDetails()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            ManageAccount();
        }

        [Fact]
        public void HttpServiceRedirectsToLogInIfServerSendsUnauthorizedHttpResponse()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);
            // mimic invalid cookie situation (auth.guard passes but server returns 401)
            _webDriver.ExecuteScript("window.localStorage.clear()");
            _webDriver.ExecuteScript("window.localStorage['vakUsername'] = 'dummyUsername'");
            ManageAccount_NotLoggedIn(_testEmail, _testPassword);
        }

        [Fact]
        public void AuthGuardRedirectsToLogInIfNotLoggedIn()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            LogOff(_testEmail);

            ManageAccount_NotLoggedIn(_testEmail, _testPassword);
        }

        [Fact]
        public void ChangeAltEmailChangesAltEmail()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            ChangeAltEmail(_testPassword, _testAltEmail);
        }

        [Fact]
        public void ChangeEmailChangesEmail()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            ChangeEmail(_testPassword, _testNewEmail);
        }

        [Fact]
        public void ChangeDisplayNameChangesDisplayName()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            ChangeDisplayName(_testPassword, _testNewDisplayName);
        }

        [Fact]
        public void ChangePasswordChangesPassword()
        {
            SignUp(_testEmail, _testPassword, _testPassword);
            ChangePassword(_testPassword, _testNewPassword);
            LogOff(_testEmail);
            LogIn(_testEmail, _testNewPassword, _homeRelativeUrl);
        }

        #region Helpers

        public void SignUp(string email, string password, string confirmPassword)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _signUpRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Email")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Email")).SendKeys(email);
            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.Id("ConfirmPassword")).SendKeys(password);

            _webDriver.FindElement(By.XPath("//button[text()='Sign up']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_homeRelativeUrl) && 
                wd.FindElements(By.LinkText($"Account:{email}")).Count == 1, _waitTime);
        }

        public void LogOff(string email) {
            _webDriver.FindElement(By.LinkText("Log off")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_homeRelativeUrl) && 
                wd.FindElements(By.LinkText($"Account:{email}")).Count == 0, _waitTime);
        }


        public void LogIn(string email, string password, string returnRelativeUrl)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _logInRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Email")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Email")).SendKeys(email);
            _webDriver.FindElement(By.Id("Password")).SendKeys(password);

            _webDriver.FindElement(By.XPath("//button[text()='Log in']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(returnRelativeUrl) &&
                wd.FindElements(By.LinkText($"Account:{email}")).Count == 1, _waitTime);
        }

        public void ForgotPassword(string email)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _forgotPasswordRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Email")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Email")).SendKeys(email);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.FindElements(By.LinkText("Did not receive email help")).Count == 1, _waitTime);
        }

        public void ResetPassword(string newPassword, string resetPasswordUrl)
        {
            _webDriver.Navigate().GoToUrl(resetPasswordUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("NewPassword")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("NewPassword")).SendKeys(newPassword);
            _webDriver.FindElement(By.Id("ConfirmPassword")).SendKeys(newPassword);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.FindElements(By.LinkText("Log in")).Count == 1, _waitTime);
        }

        public void ManageAccount()
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _manageAccountRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.XPath("//h2[text()='Manage Account']")).Count == 1, _waitTime);
        }

        public void ManageAccount_NotLoggedIn(string email, string password)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _manageAccountRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Email")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Email")).SendKeys(email);
            _webDriver.FindElement(By.Id("Password")).SendKeys(password);

            _webDriver.FindElement(By.XPath("//button[text()='Log in']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_manageAccountRelativeUrl) &&
                wd.FindElements(By.LinkText($"Account:{email}")).Count == 1, _waitTime);
        }

        public void ChangeAltEmail(string password, string newAltEmail)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changeAltEmailRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Password")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.Id("NewAltEmail")).SendKeys(newAltEmail);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_baseUrl + _manageAccountRelativeUrl) &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Alternative email address: {newAltEmail}')]")).Count == 1, _waitTime);
        }

        public void ChangeEmail(string password, string newEmail)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changeEmailRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Password")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.Id("NewEmail")).SendKeys(newEmail);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_baseUrl + _manageAccountRelativeUrl) &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Email address: {newEmail}')]")).Count == 1, _waitTime);
        }

        public void ChangeDisplayName(string password, string newDisplayName)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changeDisplayNameRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Password")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.Id("NewDisplayName")).SendKeys(newDisplayName);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.Url.Contains(_baseUrl + _manageAccountRelativeUrl) &&
                wd.FindElements(By.XPath($"//div[contains(text(),'Display name: {newDisplayName}')]")).Count == 1, _waitTime);
        }

        public void ChangePassword(string password, string newPassword)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _changePasswordRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("CurrentPassword")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("CurrentPassword")).SendKeys(password);
            _webDriver.FindElement(By.Id("NewPassword")).SendKeys(newPassword);
            _webDriver.FindElement(By.Id("ConfirmNewPassword")).SendKeys(newPassword);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.FindElements(By.XPath("//h2[text()='Manage Account']")).Count == 1, _waitTime);
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



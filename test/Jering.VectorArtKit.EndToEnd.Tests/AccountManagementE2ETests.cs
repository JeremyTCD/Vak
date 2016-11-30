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

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [Collection("E2ECollection")]
    public class AccountManagementE2ETests
    {
        private int _waitTime = 5000;
        private string _baseUrl = "http://localhost:4200/";
        private string _testEmail = "test@email.com";
        private string _testPassword = "testPassword";
        private string _testNewPassword = "testNewPassword";
        private string _emailFileName = $"{Environment.GetEnvironmentVariable("TMP")}\\SmtpTest.txt";
        private string _homeRelativeUrl = "home";
        private string _signUpRelativeUrl = "signup";
        private string _logInRelativeUrl = "login";
        private string _forgotPasswordRelativeUrl = "login/forgotpassword";
        private string _resetPasswordRelativeUrl = "login/resetpassword";

        private IWebDriver _webDriver { get; }
        private SqlConnection _sqlConnection { get; }
        private Action _resetAccountsTable { get; }

        public AccountManagementE2ETests(E2EFixture fixture )
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

        #region Helpers

        public void SignUp(string email, string password, string confirmPassword)
        {
            _webDriver.Navigate().GoToUrl(_baseUrl + _signUpRelativeUrl);

            _webDriver.Wait(wd => wd.FindElements(By.Id("Email")).Count > 0, _waitTime);

            _webDriver.FindElement(By.Id("Email")).SendKeys(email);
            _webDriver.FindElement(By.Id("Password")).SendKeys(password);
            _webDriver.FindElement(By.Id("ConfirmPassword")).SendKeys(password);

            _webDriver.Wait(wd => wd.FindElements(By.ClassName("valid")).Count == 2, _waitTime);

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

            _webDriver.Wait(wd => wd.FindElements(By.ClassName("valid")).Count == 1, _waitTime);

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

            _webDriver.Wait(wd => wd.FindElements(By.ClassName("valid")).Count == 1, _waitTime);

            _webDriver.FindElement(By.XPath("//button[text()='Submit']")).Click();

            _webDriver.Wait(wd => wd.FindElements(By.LinkText("Log in")).Count == 1, _waitTime);
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



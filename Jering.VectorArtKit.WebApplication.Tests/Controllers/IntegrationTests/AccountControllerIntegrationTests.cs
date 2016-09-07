using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.Controllers;
using Jering.VectorArtKit.WebApplication.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApplication.Tests.Controllers.IntegrationTests
{
    [Collection("ControllersCollection")]
    public class AccountControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        private VakAccountRepository _vakAccountRepository { get; }
        private Func<Task> _resetAccountsTable { get; }
        private string _accountControllerName { get; } = nameof(AccountController).Replace("Controller", "");
        private StringOptions _stringOptions { get; } = new StringOptions();

        public AccountControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
            _vakAccountRepository = controllersFixture.VakAccountRepository;
            _resetAccountsTable = controllersFixture.ResetAccountsTable;
        }

        [Fact]
        public async Task SignUpGet_ReturnsSignUpViewWithAntiForgeryTokenAndCookie()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/SignUp");

            // Assert
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(httpResponseMessage);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            string html = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_SignUp));
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Theory]
        [MemberData(nameof(SignUpPostData))]
        public async Task SignUpPost_ReturnsSignUpViewWithErrorMessagesIfModelStateIsInvalidOrCreateAccountFails(string email, string password, string confirmPassword)
        {
            // Arrange
            await _resetAccountsTable();
            if (email == "Email1@test.com")
            {
                await _vakAccountRepository.CreateAccountAsync(email, password);
            }

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(email, password, confirmPassword);

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ViewTitle_SignUp));
            if (email == "Email1@test.com")
            {
                Assert.True(html.Contains(_stringOptions.ErrorMessage_EmailInUse));
            }
            else
            {
                Assert.True(html.Contains(_stringOptions.ErrorMessage_Email_Invalid));
                Assert.True(html.Contains(_stringOptions.ErrorMessage_Password_FormatInvalid));
                Assert.True(html.Contains(_stringOptions.ErrorMessage_ConfirmPassword_Differs));
            }
        }

        public static IEnumerable<object[]> SignUpPostData()
        {
            yield return new object[] { "invalidModelState", "x", "y" };
            yield return new object[] { "Email1@test.com", "Password1@", "Password1@" };
        }

        [Fact]
        public async Task SignUpPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "ConfirmPassword", "Password1@" }
            };

            HttpRequestMessage httpRequestMessage = RequestHelper.Create("Account/SignUp", HttpMethod.Post, formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal("BadRequest", httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task SignUpPost_RedirectsToHomeIndexViewAndSendsApplicationCookieAndConfirmEmailEmailIfRegistrationIsSuccessful()
        {
            // Arrange
            await _resetAccountsTable();
            File.WriteAllText(@"Temp\SmtpTest.txt", "");

            // Act
            HttpResponseMessage signUpPostResponse = await SignUp("Email1@test.com", "Password1@", "Password1@");

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(signUpPostResponse);
            Assert.Equal("Jering.Application", cookies.Keys.First());
            Assert.Equal("Redirect", signUpPostResponse.StatusCode.ToString());
            Assert.Equal("/", signUpPostResponse.Headers.Location.ToString());
            
            string confirmEmailEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(_stringOptions.ConfirmEmail_Subject, confirmEmailEmail);
            Assert.Contains("Email1@test.com", confirmEmailEmail);
            Assert.Matches(_stringOptions.ConfirmEmail_Message.Replace("{0}", $"http://localhost/{_accountControllerName}/{nameof(AccountController.EmailVerificationConfirmation)}.*?"), confirmEmailEmail);
        }

        [Fact]
        public async Task LoginGet_ReturnsLoginViewWithAntiForgeryTokenAndCookie()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/Login");

            // Assert
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(httpResponseMessage);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_LogIn));
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Theory]
        [MemberData(nameof(LoginPostData))]
        public async Task LoginPost_ReturnsLoginViewWithErrorMessageIfModelStateOrLoginCredentialsAreInvalid(string email, string password)
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount("Email1@test.com", "Password1@");

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(email, password);

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ViewTitle_LogIn));
            Assert.True(html.Contains(_stringOptions.LogIn_Failed));
        }

        public static IEnumerable<object[]> LoginPostData()
        {
            yield return new object[] { "invalidModelState", "x" };
            yield return new object[] { "invalid@Credentials", "@InvalidCredentials0" };
        }

        [Fact]
        public async Task LoginPost_RedirectsToHomeIndexViewAndSendsAuthenticationCookieIfLoginSucceeds()
        {
            // Arrange
            string email = "Email1@test.com";
            string password = "Password1@";

            await _resetAccountsTable();
            await _vakAccountRepository.CreateAccountAsync(email, password);

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(email, password);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal("Jering.Application", cookies.Keys.First());
            Assert.Equal("Redirect", httpResponseMessage.StatusCode.ToString());
            Assert.Equal("/", httpResponseMessage.Headers.Location.ToString());
        }

        [Fact]
        public async Task LoginPost_RedirectsToVerifyCodeViewAndSendsTwoFactorCookieAndSendsTwoFactorEmailIfTwoFactorIsRequired()
        {
            // Arrange
            string email = "Email1@test.com";
            string password = "Password1@";

            await _resetAccountsTable();
            await CreateAccount(email, password, true);
            File.WriteAllText(@"Temp\SmtpTest.txt", "");

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(email, password);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal("Jering.TwoFactor", cookies.Keys.First());
            Assert.Equal("Redirect", httpResponseMessage.StatusCode.ToString());
            Assert.Equal($"/Account/{nameof(AccountController.VerifyTwoFactorCode)}?IsPersistent=True", httpResponseMessage.Headers.Location.ToString());
            
            string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(_stringOptions.TwoFactorEmail_Subject, twoFactorEmail);
            Assert.Contains(email, twoFactorEmail);
            Assert.Matches(_stringOptions.TwoFactorEmail_Message.Replace("{0}", @"\d{6,6}"), twoFactorEmail);
        }

        [Fact]
        public async Task LoginPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "RememberMe", "true" }
            };

            HttpRequestMessage httpRequestMessage = RequestHelper.Create("Account/Login", HttpMethod.Post, formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal("BadRequest", httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task LogOffPost_RedirectsToHomeIndexViewAndSendsHeaderToRemoveAllCookiesIfSuccessful()
        {
            //Arrange
            string email = "Email1@test.com", password = "Password1@";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage loginPostResponse = await LogIn(email, password);
            HttpRequestMessage indexGetRequest = RequestHelper.CreateWithCookiesFromResponse("Home/Index", HttpMethod.Get, null, loginPostResponse);
            HttpResponseMessage indexGetResponse = await _httpClient.SendAsync(indexGetRequest);

            IDictionary<string, string> logOffPostBody = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", await AntiForgeryTokenHelper.ExtractAntiForgeryToken(indexGetResponse) }
            };
            HttpRequestMessage logOffPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/LogOff", HttpMethod.Post, logOffPostBody, loginPostResponse);
            CookiesHelper.PutCookiesOnRequest(logOffPostRequest, CookiesHelper.ExtractCookiesFromResponse(indexGetResponse));

            // Act
            HttpResponseMessage logOffPostResponse = await _httpClient.SendAsync(logOffPostRequest);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(logOffPostResponse);
            Assert.Equal(2, cookies.Count);
            Assert.Contains("Jering.Application", cookies.Keys);
            Assert.Equal("", cookies["Jering.Application"]);
            Assert.Contains("Jering.TwoFactor", cookies.Keys);
            Assert.Equal("", cookies["Jering.TwoFactor"]);
            Assert.Equal("Redirect", logOffPostResponse.StatusCode.ToString());
            Assert.Equal("/", logOffPostResponse.Headers.Location.ToString());
        }

        [Fact]
        public async Task LogOffPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            //Arrange
            string email = "Email1@test.com", password = "Password1@";

            await _resetAccountsTable();
            await _vakAccountRepository.CreateAccountAsync(email, password);
            await _vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            HttpResponseMessage loginPostResponse = await LogIn(email, password);

            HttpRequestMessage indexGetRequest = RequestHelper.CreateWithCookiesFromResponse("Home/Index", HttpMethod.Get, null, loginPostResponse);
            HttpResponseMessage indexGetResponse = await _httpClient.SendAsync(indexGetRequest);

            HttpRequestMessage logOffPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/LogOff", HttpMethod.Post, null, loginPostResponse);

            // Act
            HttpResponseMessage logOffPostResponse = await _httpClient.SendAsync(logOffPostRequest);

            // Assert
            Assert.Equal("BadRequest", logOffPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task LogOffPost_RedirectsToLoginViewIfAuthenticationFails()
        {
            //Arrange
            string email = "Email1@test.com", password = "Password1@";

            await _resetAccountsTable();
            await _vakAccountRepository.CreateAccountAsync(email, password);
            await _vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            HttpResponseMessage loginPostResponse = await LogIn(email, password);
            HttpRequestMessage indexGetRequest = RequestHelper.CreateWithCookiesFromResponse("Home/Index", HttpMethod.Get, null, loginPostResponse);
            HttpResponseMessage indexGetResponse = await _httpClient.SendAsync(indexGetRequest);

            IDictionary<string, string> logOffPostBody = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", await AntiForgeryTokenHelper.ExtractAntiForgeryToken(indexGetResponse) }
            };
            HttpRequestMessage logOffPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/LogOff", HttpMethod.Post, logOffPostBody, indexGetResponse);

            // Act
            HttpResponseMessage logOffPostResponse = await _httpClient.SendAsync(logOffPostRequest);

            // Assert
            Assert.Equal("Redirect", logOffPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", logOffPostResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task VerifyTwoFactorCodeGet_ReturnsVerifyTwoFactorCodeViewWithAntiForgeryTokenAndCookieIfTwoFactorCookieIsValid()
        {
            // Arrange
            string email = "Email1@test.com";
            string password = "Password1@";

            await _resetAccountsTable();
            await CreateAccount(email, password, true);
            HttpResponseMessage loginPostResponse = await LogIn(email, password);
            HttpRequestMessage VerifyTwoFactorCodeGetRequest = RequestHelper.CreateWithCookiesFromResponse(loginPostResponse.Headers.Location.ToString(), HttpMethod.Get, null, loginPostResponse);

            // Act
            HttpResponseMessage VerifyTwoFactorCodeGetResponse = await _httpClient.SendAsync(VerifyTwoFactorCodeGetRequest);

            // Assert
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(VerifyTwoFactorCodeGetResponse);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(VerifyTwoFactorCodeGetResponse);
            string html = await VerifyTwoFactorCodeGetResponse.Content.ReadAsStringAsync();
            Assert.Equal("OK", VerifyTwoFactorCodeGetResponse.StatusCode.ToString());
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_VerifyTwoFactorCode));
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Fact]
        public async Task VerifyTwoFactorCodeGet_ReturnsErrorViewIfTwoFactorCookieIsInvalid()
        {
            // Arrange
            HttpResponseMessage VerifyTwoFactorCodeGetResponse = await _httpClient.GetAsync("Account/VerifyTwoFactorCode");

            // Assert
            string html = await VerifyTwoFactorCodeGetResponse.Content.ReadAsStringAsync();
            Assert.Equal("OK", VerifyTwoFactorCodeGetResponse.StatusCode.ToString());         
            Assert.True(html.Contains(_stringOptions.ViewTitle_Error));
        }

        [Fact]
        public async Task VerifyTwoFactorCodePost_RedirectsToHomeIndexViewWithApplicationCookieAndRemovesTwoFactorCookieIfCodeIsValid()
        {
            // Arrange
            string email = "Email1@test.com", password = "Password1@";

            await _resetAccountsTable();
            await CreateAccount(email, password, true);
            HttpResponseMessage loginPostResponse = await LogIn(email, password);
            string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

            // Act
            HttpResponseMessage VerifyTwoFactorCodePostResponse = await VerifyTwoFactorCode(CookiesHelper.ExtractCookiesFromResponse(loginPostResponse), code);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(VerifyTwoFactorCodePostResponse);
            Assert.Contains("Jering.Application", cookies.Keys);
            Assert.Contains("Jering.TwoFactor", cookies.Keys);
            Assert.Equal("", cookies["Jering.TwoFactor"]);
            Assert.Equal("Redirect", VerifyTwoFactorCodePostResponse.StatusCode.ToString());
            Assert.Equal("/", VerifyTwoFactorCodePostResponse.Headers.Location.ToString());
        }

        [Theory]
        [MemberData(nameof(VerifyTwoFactorCodePostData))]
        public async Task VerifyTwoFactorCodePost_ReturnsVerifyTwoFactorCodeViewWithErrorMessageIfModelStateOrCodeIsInvalid(string code)
        {
            // Arrange
            string email = "Email1@test.com", password = "Password1@";

            await _resetAccountsTable();
            await CreateAccount(email, password, true);
            HttpResponseMessage loginPostResponse = await LogIn(email, password);
            string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string actualCode = codeMatch.Match(twoFactorEmail).Groups[1].Value;
            // low probability of this but avoid anyway
            if (code == actualCode)
            {
                code = "654321";
            }

            // Act
            HttpResponseMessage VerifyTwoFactorCodePostResponse = await VerifyTwoFactorCode(CookiesHelper.ExtractCookiesFromResponse(loginPostResponse), code);

            // Assert
            Assert.Equal("OK", VerifyTwoFactorCodePostResponse.StatusCode.ToString());
            string html = await VerifyTwoFactorCodePostResponse.Content.ReadAsStringAsync();
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_VerifyTwoFactorCode));
            Assert.True(html.Contains(_stringOptions.VerifyTwoFactorCode_InvalidCode));
        }

        public static IEnumerable<object[]> VerifyTwoFactorCodePostData()
        {
            yield return new object[] { "x" };
            yield return new object[] { "123456" };
        }

        [Fact]
        public async Task ForgotPasswordGet_ReturnsForgotPasswordViewWithAntiForgeryTokenAndCookie()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/ForgotPassword");

            // Assert
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(httpResponseMessage);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            string html = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_ForgotPassword));
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Fact]
        public async Task ForgotPasswordPost_ReturnsForgotPasswordViewIfModelStateIsInvalid()
        {
            // Act
            HttpResponseMessage forgotPasswordPostResponse = await ForgotPassword("invalidEmail");

            // Assert
            string html = await forgotPasswordPostResponse.Content.ReadAsStringAsync();
            
            Assert.Equal("OK", forgotPasswordPostResponse.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ViewTitle_ForgotPassword));
            Assert.True(html.Contains(_stringOptions.ErrorMessage_Email_Invalid));
        }

        [Theory]
        [MemberData(nameof(ForgotPasswordPostData))]
        public async Task ForgotPasswordPost_RedirectsToForgotPasswordConfirmationViewAndSendsResetPasswordEmailIfEmailIsValid(string email)
        {
            // Arrange
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await _resetAccountsTable();
            await CreateAccount(email, "Password1@", true);
            email = email == "invalid@email" ? "random@email" : email;

            // Act
            HttpResponseMessage forgotPasswordPostResponse = await ForgotPassword(email);

            // Assert
            Assert.Equal("Redirect", forgotPasswordPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ForgotPasswordConfirmation)}?Email={email}",
                forgotPasswordPostResponse.Headers.Location.ToString());
            
            string resetPasswordEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            if (email == "valid@email")
            {
                Assert.Contains(_stringOptions.ResetPasswordEmail_Subject, resetPasswordEmail);
                Assert.Contains(email, resetPasswordEmail);
                Assert.Matches(string.Format(_stringOptions.ResetPasswordEmail_Message,
                    $"http://localhost/{_accountControllerName}/{nameof(AccountController.ResetPassword)}.*?"), resetPasswordEmail);
            }
            else
            {
                Assert.Equal("", resetPasswordEmail);
            }

        }

        public static IEnumerable<object[]> ForgotPasswordPostData()
        {
            yield return new object[] { "valid@email" };
            yield return new object[] { "invalid@email" };
        }

        [Fact]
        public async Task ForgotPasswordConfirmationGet_ReturnsForgotPasswordView()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/ForgotPasswordConfirmation");

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_ForgotPasswordConfirmation));
        }

        [Fact]
        public async Task ResetPasswordGet_ReturnsResetPasswordViewWithAntiForgeryTokenAndCookie()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/ResetPassword?Token=token&Email=email");

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());        
            Assert.Contains(_stringOptions.ViewTitle_ResetPassword, html);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(httpResponseMessage);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Fact]
        public async Task ResetPasswordPost_ReturnsResetPasswordViewWithErrorMessagesIfModelStateIsInvalid()
        {
            // Act
            HttpResponseMessage resetPasswordPostResponse = await ResetPassword("token", "email", "invalid", "invalid2");

            // Assert
            string html = await resetPasswordPostResponse.Content.ReadAsStringAsync();
            
            Assert.Equal("OK", resetPasswordPostResponse.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ErrorMessage_Password_FormatInvalid));
            Assert.True(html.Contains(_stringOptions.ErrorMessage_ConfirmPassword_Differs));
        }

        [Fact]
        public async Task ResetPasswordPost_RedirectsToResetPasswordConfirmationViewIfEmailTokenAndModelStateAreValid()
        {
            // Arrange
            string email = "Email1@test.com", newPassword = "Password";

            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await _resetAccountsTable();
            await CreateAccount(email, "Password1@");

            await ForgotPassword(email);
            string resetPasswordEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Regex tokenRegex = new Regex(@"Token=(.*?)&");
            string token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);

            // Act
            HttpResponseMessage resetPasswordPostResponse = await ResetPassword(token, email, newPassword, newPassword);

            // Assert
            Assert.NotNull(_vakAccountRepository.GetAccountByEmailAndPasswordAsync(email, newPassword));
            Assert.Equal("Redirect", resetPasswordPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ResetPasswordConfirmation)}?Email={email}",
                resetPasswordPostResponse.Headers.Location.ToString());
        }

        [Theory]
        [MemberData(nameof(ResetPasswordPostData))]
        public async Task ResetPasswordPost_ReturnsErrorViewIfEmailOrTokenIsInvalid(bool useValidToken, bool useValidEmail)
        {
            // Arrange
            string email = "Email1@test.com", token = "invalid";
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await _resetAccountsTable();
            await CreateAccount(email, "Password1@");

            await ForgotPassword(email);

            if (useValidToken)
            {
                string resetPasswordEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
                Regex tokenRegex = new Regex(@"Token=(.*?)&");
                token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);
            }
            if (!useValidEmail)
            {
                email = "invalid";
            }

            // Act
            HttpResponseMessage resetPasswordPostResponse = await ResetPassword(token, email, "Password", "Password");

            // Assert
            Assert.Equal("OK", resetPasswordPostResponse.StatusCode.ToString());
            string html = await resetPasswordPostResponse.Content.ReadAsStringAsync();
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_Error));
        }

        public static IEnumerable<object[]> ResetPasswordPostData()
        {
            yield return new object[] { false, true };
            yield return new object[] { true, false };
            yield return new object[] { false, false };
        }

        [Fact]
        public async Task ResetPasswordConfirmationGet_ReturnsResetPasswordConfirmationView()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/ResetPasswordConfirmation?Email=test@test.com");

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_ResetPasswordConfirmation));
        }

        [Theory]
        [MemberData(nameof(EmailVerificationConfirmationGetData))]
        public async Task EmailVerificationConfirmationGet_ReturnsErrorViewIfAccountIdTokenOrModelStateIsInvalid(string accountId, string token)
        {
            // Arrange
            if (accountId == "1")
            {
                await _resetAccountsTable();
                await CreateAccount("test@test.com", "Password");
            }

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"Account/EmailVerificationConfirmation?Email={accountId}&Token={token}");

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_Error));
        }

        public static IEnumerable<object[]> EmailVerificationConfirmationGetData()
        {
            yield return new object[] { "", "" };
            yield return new object[] { "1", "invalidtoken" };
        }

        [Fact]
        public async Task EmailVerificationConfirmationGet_ReturnsEmailVerificationConfirmationViewIfEmailTokenAndModelStateAreValid()
        {
            // Arrange
            string email = "Email1@test.com", password = "Password";
            int accountId = 1;

            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await _resetAccountsTable();
            await SignUp(email, password, password);

            string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Regex tokenRegex = new Regex(@"Token=(.*?)&");
            string token = tokenRegex.Match(emailVerificationEmail).Groups[1].Value;

            // Act
            HttpResponseMessage emailVerificationConfirmationGetResponse = await _httpClient.GetAsync($"Account/EmailVerificationConfirmation?AccountId={accountId}&Token={token}");

            // Assert
            Assert.True((await _vakAccountRepository.GetAccountAsync(accountId)).EmailVerified);
            Assert.Equal("OK", emailVerificationConfirmationGetResponse.StatusCode.ToString());
            string html = await emailVerificationConfirmationGetResponse.Content.ReadAsStringAsync();
            
            Assert.True(html.Contains(_stringOptions.ViewTitle_EmailVerificationConfirmation));
        }

        [Fact]
        public async Task ManageAccountGet_ReturnsManageAccountViewIfAuthenticationIsSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            HttpRequestMessage manageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
                HttpMethod.Get,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(manageAccountGetRequest);

            // Assert
            Assert.Equal("OK", manageAccountGetResponse.StatusCode.ToString());
            
            string html = await manageAccountGetResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ManageAccount, html);
        }

        [Fact]
        public async Task ManageAccountGet_RedirectsToLoginViewIfAuthenticationFails()
        {
            // Act
            HttpResponseMessage manageAccountGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.ManageAccount)}");

            // Assert
            Assert.Equal("Redirect", manageAccountGetResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", manageAccountGetResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangePasswordPost_RedirectsToManageAccountWithANewApplicationCookieAndUpdatesPasswordHashIfSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newPassword = "passworD";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changePasswordPostResponse = await ChangePassword(password, newPassword, newPassword, applicationCookie);

            // Assert
            Assert.Equal("Redirect", changePasswordPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", changePasswordPostResponse.Headers.Location.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(changePasswordPostResponse);
            Assert.Equal("Jering.Application", cookies.Keys.First());
            VakAccount account = await _vakAccountRepository.GetAccountByEmailAndPasswordAsync(email, newPassword);
            Assert.NotNull(account);
        }

        [Theory]
        [MemberData(nameof(ChangePasswordPostData))]
        public async Task ChangePasswordPost_ChangePasswordViewWithErrorMessagesIfModelStateOrCurrentPasswordIsInvalid(string currentPassword, string newPassword, string confirmNewPassword)
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changePasswordPostResponse = await ChangePassword(currentPassword, newPassword, confirmNewPassword, applicationCookie);

            // Assert
            Assert.Equal("OK", changePasswordPostResponse.StatusCode.ToString());
            
            string html = await changePasswordPostResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangePassword, html);
            if(currentPassword == "Password" && newPassword == "Password")
            {
                Assert.Contains(_stringOptions.ErrorMessage_NewPassword_FormatInvalid, html);
            }
            else if(currentPassword == "Password" && newPassword == "passworD")
            {
                Assert.Contains(_stringOptions.ErrorMessage_ConfirmPassword_Differs, html);
            }
            else
            {
                Assert.Contains(_stringOptions.ErrorMessage_CurrentPassword_Invalid, html);
            }
        }

        public static IEnumerable<object[]> ChangePasswordPostData()
        {
            yield return new object[] { "Password", "Password", "Password" };
            yield return new object[] { "Password", "passworD", "password" };
            yield return new object[] { "passworD", "Password", "Password" };
        }

        [Fact]
        public async Task ChangePasswordPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newPassword = "passworD";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "CurrentPassword", password },
                {"NewPassword", newPassword },
                {"ConfirmNewPassword", newPassword }
            };
            HttpRequestMessage changePasswordPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}",
                HttpMethod.Post,
                formPostBodyData,
                logInPostResponse);

            // Act
            HttpResponseMessage changePasswordPostResponse = await _httpClient.SendAsync(changePasswordPostRequest);

            // Assert
            Assert.Equal("BadRequest", changePasswordPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task ChangePasswordPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newPassword = "passworD";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ChangePasswordGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage ChangePasswordGetResponse = await _httpClient.SendAsync(ChangePasswordGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(ChangePasswordGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "CurrentPassword", password },
                {"NewPassword", newPassword },
                {"ConfirmNewPassword", newPassword },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changePasswordPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}",
                HttpMethod.Post,
                formPostBodyData,
                ChangePasswordGetResponse);

            // Act
            HttpResponseMessage changePasswordPostResponse = await _httpClient.SendAsync(changePasswordPostRequest);

            // Assert
            Assert.Equal("Redirect", changePasswordPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changePasswordPostResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangePasswordGet_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Act 
            HttpResponseMessage changePasswordGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}");

            // Assert
            Assert.Equal("Redirect", changePasswordGetResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changePasswordGetResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangePasswordGet_ReturnsChangePasswordViewWithAntiForgeryCredentialsIfAuthenticationSucceeds()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ChangePasswordGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}",
                HttpMethod.Get,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage changePasswordGetResponse = await _httpClient.SendAsync(ChangePasswordGetRequest);

            // Assert
            Assert.Equal("OK", changePasswordGetResponse.StatusCode.ToString());
            string html = await changePasswordGetResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangePassword, html);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changePasswordGetResponse);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(changePasswordGetResponse);
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Fact]
        public async Task ChangeEmailGet_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Act 
            HttpResponseMessage changeEmailGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}");

            // Assert
            Assert.Equal("Redirect", changeEmailGetResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changeEmailGetResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangeEmailGet_ReturnsChangeEmailViewWithAntiForgeryCredentialsIfAuthenticationSucceeds()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ChangeEmailGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}",
                HttpMethod.Get,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage changeEmailGetResponse = await _httpClient.SendAsync(ChangeEmailGetRequest);

            // Assert
            Assert.Equal("OK", changeEmailGetResponse.StatusCode.ToString());
            string html = await changeEmailGetResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangeEmail, html);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeEmailGetResponse);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(changeEmailGetResponse);
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Fact]
        public async Task ChangeEmailPost_RedirectsToManageAccountWithANewApplicationCookieAndUpdatesEmailIfSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newEmail = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeEmailPostResponse = await ChangeEmail(password, email, newEmail, applicationCookie);

            // Assert
            Assert.Equal("Redirect", changeEmailPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", changeEmailPostResponse.Headers.Location.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(changeEmailPostResponse);
            Assert.Equal("Jering.Application", cookies.Keys.First());
            VakAccount account = await _vakAccountRepository.GetAccountByEmailAsync(newEmail);
            Assert.NotNull(account);
        }

        [Theory]
        [MemberData(nameof(ChangeEmailPostData))]
        public async Task ChangeEmailPost_ChangeEmailViewWithErrorMessagesIfModelStateOrPasswordIsInvalidOrEmailIsInUse(string testPassword, string newEmail)
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);
            if(newEmail == "inuse@email.com")
            {
                await CreateAccount(newEmail, password);
            }

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeEmailPostResponse = await ChangeEmail(testPassword, email, newEmail, applicationCookie);

            // Assert
            Assert.Equal("OK", changeEmailPostResponse.StatusCode.ToString());
            string html = await changeEmailPostResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangeEmail, html);
            if (newEmail == "invalidEmail")
            {
                Assert.Contains(_stringOptions.ErrorMessage_Email_Invalid, html);
            }
            else if(testPassword == "invalidPassword")
            {
                Assert.Contains(_stringOptions.ErrorMessage_Password_Invalid, html);
            }
            else if(newEmail == "inuse@email.com")
            {
                Assert.Contains(_stringOptions.ErrorMessage_EmailInUse, html);
            }
            else if(newEmail == "email@email.com")
            {
                Assert.Contains(_stringOptions.ErrorMessage_NewEmail_MustDiffer, html);
            }
        }

        public static IEnumerable<object[]> ChangeEmailPostData()
        {
            yield return new object[] { "", "invalidEmail" };
            yield return new object[] { "invalidPassword", "new@email.com" };
            yield return new object[] { "Password", "inuse@email.com" };
            yield return new object[] { "Password", "email@email.com" };
        }

        [Fact]
        public async Task ChangeEmailPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newEmail = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Password", password },
                {"NewEmail", newEmail }
            };
            HttpRequestMessage changeEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}",
                HttpMethod.Post,
                formPostBodyData,
                logInPostResponse);

            // Act
            HttpResponseMessage changeEmailPostResponse = await _httpClient.SendAsync(changeEmailPostRequest);

            // Assert
            Assert.Equal("BadRequest", changeEmailPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task ChangeEmailPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newEmail = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage changeEmailGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage changeEmailGetResponse = await _httpClient.SendAsync(changeEmailGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeEmailGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Password", password },
                {"NewEmail", newEmail },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changeEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}",
                HttpMethod.Post,
                formPostBodyData,
                changeEmailGetResponse);

            // Act
            HttpResponseMessage changeEmailPostResponse = await _httpClient.SendAsync(changeEmailPostRequest);

            // Assert
            Assert.Equal("Redirect", changeEmailPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changeEmailPostResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangeEmailPost_ReturnsErrorViewIfNewEmailDiffersFromCurrentEmailInValidationButNotInAction()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newEmail = "email@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeEmailPostResponse = await ChangeEmail(password, null, newEmail, applicationCookie);

            // Assert
            string html = await changeEmailPostResponse.Content.ReadAsStringAsync();
            Assert.Equal("OK", changeEmailPostResponse.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ViewTitle_Error));
        }

        #region Helpers
        public async Task<HttpResponseMessage> ChangeEmail(string password, string currentEmail, string newEmail, IDictionary<string, string> applicationCookie)
        {
            HttpRequestMessage changeEmailGetRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}",
                HttpMethod.Get,
                null);
            CookiesHelper.PutCookiesOnRequest(changeEmailGetRequest, applicationCookie);
            HttpResponseMessage changeEmailGetResponse = await _httpClient.SendAsync(changeEmailGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeEmailGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"CurrentEmail", currentEmail },
                {"Password", password },
                {"NewEmail", newEmail },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changeEmailPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(changeEmailPostRequest, applicationCookie);
            CookiesHelper.CopyCookiesFromResponse(changeEmailPostRequest, changeEmailGetResponse);

            return await _httpClient.SendAsync(changeEmailPostRequest);
        }

        public async Task<HttpResponseMessage> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword, IDictionary<string, string> applicationCookie)
        {
            HttpRequestMessage ChangePasswordGetRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}",
                HttpMethod.Get,
                null);
            CookiesHelper.PutCookiesOnRequest(ChangePasswordGetRequest, applicationCookie);
            HttpResponseMessage ChangePasswordGetResponse = await _httpClient.SendAsync(ChangePasswordGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(ChangePasswordGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "CurrentPassword", currentPassword },
                {"NewPassword", newPassword },
                {"ConfirmNewPassword", confirmNewPassword },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changePasswordPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(changePasswordPostRequest, applicationCookie);
            CookiesHelper.CopyCookiesFromResponse(changePasswordPostRequest, ChangePasswordGetResponse);

            return await _httpClient.SendAsync(changePasswordPostRequest);
        }

        public async Task<HttpResponseMessage> ForgotPassword(string email)
        {
            HttpResponseMessage forgotPasswordGetRequest = await _httpClient.GetAsync("Account/ForgotPassword");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(forgotPasswordGetRequest);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", email },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            return await _httpClient.SendAsync(RequestHelper.CreateWithCookiesFromResponse("Account/ForgotPassword", HttpMethod.Post, formPostBodyData, forgotPasswordGetRequest));
        }

        public async Task<HttpResponseMessage> ResetPassword(string token, string email, string newPassword, string confirmNewPassword)
        {
            HttpResponseMessage resetPasswordGetResponse = await _httpClient.GetAsync($"Account/ResetPassword?Token={token}&Email={email}");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(resetPasswordGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", email },
                { "Token", token },
                { "NewPassword", newPassword },
                { "ConfirmNewPassword", confirmNewPassword },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            return await _httpClient.SendAsync(RequestHelper.CreateWithCookiesFromResponse("Account/ResetPassword", HttpMethod.Post, formPostBodyData, resetPasswordGetResponse));
        }

        public async Task<HttpResponseMessage> VerifyTwoFactorCode(IDictionary<string, string> twoFactorCookie, string code)
        {
            HttpRequestMessage VerifyTwoFactorCodeGetRequest = RequestHelper.Create("/Account/VerifyTwoFactorCode?IsPersistent=true", HttpMethod.Get, null);
            CookiesHelper.PutCookiesOnRequest(VerifyTwoFactorCodeGetRequest, twoFactorCookie);
            HttpResponseMessage VerifyTwoFactorCodeGetResponse = await _httpClient.SendAsync(VerifyTwoFactorCodeGetRequest);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(VerifyTwoFactorCodeGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
                {
                    { "Code",  code},
                    { "__RequestVerificationToken", antiForgeryToken }
                };
            HttpRequestMessage VerifyTwoFactorCodePostRequest = RequestHelper.Create("Account/VerifyTwoFactorCode", HttpMethod.Post, formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(VerifyTwoFactorCodePostRequest, twoFactorCookie);
            CookiesHelper.CopyCookiesFromResponse(VerifyTwoFactorCodePostRequest, VerifyTwoFactorCodeGetResponse);

            // Act
            return await _httpClient.SendAsync(VerifyTwoFactorCodePostRequest);
        }
        public async Task<HttpResponseMessage> SignUp(string email, string password, string confirmPassword)
        {
            HttpResponseMessage signUpResponse = await _httpClient.GetAsync("Account/SignUp");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(signUpResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", email},
                { "Password", password},
                { "ConfirmPassword",  confirmPassword},
                { "__RequestVerificationToken", antiForgeryToken }
            };

            HttpRequestMessage httpRequestMessage = RequestHelper.CreateWithCookiesFromResponse("Account/SignUp", HttpMethod.Post, formPostBodyData, signUpResponse);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        /// <summary>
        /// Log in to account with credentials <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// Returns login post <see cref="HttpResponseMessage"/>. If successful, returns home index view and contains application cookie. 
        /// </returns>
        public async Task<HttpResponseMessage> LogIn(string email, string password)
        {
            HttpResponseMessage loginGetResponse = await _httpClient.GetAsync("Account/Login");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", email},
                { "Password", password},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            HttpRequestMessage loginPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, formPostBodyData, loginGetResponse);
            return await _httpClient.SendAsync(loginPostRequest);
        }

        public async Task<VakAccount> CreateAccount(string email, string password, bool twoFactorEnabled = false, bool emailConfirmed = false)
        {
            VakAccount account = await _vakAccountRepository.CreateAccountAsync(email, password);
            if (twoFactorEnabled)
            {
                await _vakAccountRepository.UpdateAccountTwoFactorEnabledAsync(account.AccountId, true);
            }
            if (emailConfirmed)
            {
                await _vakAccountRepository.UpdateAccountEmailConfirmedAsync(account.AccountId);
            }

            return account;
        }

        #endregion
    }
}



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
            Assert.Contains(_stringOptions.Email_EmailVerification_Subject, confirmEmailEmail);
            Assert.Contains("Email1@test.com", confirmEmailEmail);
            Assert.Matches(_stringOptions.Email_EmailVerification_Message.Replace("{0}", $"http://localhost/{_accountControllerName}/{nameof(AccountController.EmailVerificationConfirmation)}.*?"), confirmEmailEmail);
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
            await _vakAccountRepository.UpdateAccountEmailVerifiedAsync(1, true);

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
            await _vakAccountRepository.UpdateAccountEmailVerifiedAsync(1, true);

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
            Assert.True(html.Contains(_stringOptions.ErrorMessage_TwoFactorCode_Invalid));
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
            if (email == "alt@email")
            {
                await _vakAccountRepository.UpdateAccountEmailAsync(1, "email@email");
                await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(1, "alt@email");
            }
            else
            {
                email = email == "invalid@email" ? "random@email" : email;
            }

            // Act
            HttpResponseMessage forgotPasswordPostResponse = await ForgotPassword(email);

            // Assert
            Assert.Equal("Redirect", forgotPasswordPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ForgotPasswordConfirmation)}?Email={email}",
                forgotPasswordPostResponse.Headers.Location.ToString());

            string resetPasswordEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            if (email == "valid@email" || email == "alt@email")
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
            yield return new object[] { "alt@email" };
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
            if (currentPassword == "Password" && newPassword == "Password")
            {
                Assert.Contains(_stringOptions.ErrorMessage_NewPassword_FormatInvalid, html);
            }
            else if (currentPassword == "Password" && newPassword == "passworD")
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
        public async Task ChangeEmailPost_RedirectsToManageAccountWithNewApplicationCookieAndUpdatesEmailIfSuccessful()
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
            if (newEmail == "inUse@email.com")
            {
                await CreateAccount(newEmail, password);
            }
            else if (newEmail == "inUseAsAlt@email.com")
            {
                await CreateAccount("email2@email.com", password);
                await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(2, newEmail);
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
            else if (testPassword == "invalidPassword")
            {
                Assert.Contains(_stringOptions.ErrorMessage_Password_Invalid, html);
            }
            else if (newEmail == "inUse@email.com" || newEmail == "inUseAsAlt@email.com")
            {
                Assert.Contains(_stringOptions.ErrorMessage_EmailInUse, html);
            }
            else if (newEmail == "email@email.com")
            {
                Assert.Contains(_stringOptions.ErrorMessage_NewEmail_MustDiffer, html);
            }
        }

        public static IEnumerable<object[]> ChangeEmailPostData()
        {
            yield return new object[] { "", "invalidEmail" };
            yield return new object[] { "invalidPassword", "new@email.com" };
            yield return new object[] { "Password", "inUse@email.com" };
            yield return new object[] { "Password", "inUseAsAlt@email.com" };
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
                {"CurrentEmail", email },
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
                {"CurrentEmail", email },
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

        [Fact]
        public async Task ChangeAlternativeEmailGet_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Act 
            HttpResponseMessage changeAlternativeEmailGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}");

            // Assert
            Assert.Equal("Redirect", changeAlternativeEmailGetResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changeAlternativeEmailGetResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangeAlternativeEmailGet_ReturnsChangeAlternativeEmailViewWithAntiForgeryCredentialsIfAuthenticationSucceeds()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ChangeAlternativeEmailGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}",
                HttpMethod.Get,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage changeAlternativeEmailGetResponse = await _httpClient.SendAsync(ChangeAlternativeEmailGetRequest);

            // Assert
            Assert.Equal("OK", changeAlternativeEmailGetResponse.StatusCode.ToString());
            string html = await changeAlternativeEmailGetResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangeAlternativeEmail, html);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeAlternativeEmailGetResponse);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(changeAlternativeEmailGetResponse);
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }


        [Fact]
        public async Task ChangeAlternativeEmailPost_RedirectsToManageAccountAndUpdatesAlternativeEmailIfSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newAlternativeEmail = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeAlternativeEmailPostResponse = await ChangeAlternativeEmail(password, null, newAlternativeEmail, applicationCookie);

            // Assert
            Assert.Equal("Redirect", changeAlternativeEmailPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", changeAlternativeEmailPostResponse.Headers.Location.ToString());
            VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
            Assert.Equal(newAlternativeEmail, account.AlternativeEmail);
        }

        [Theory]
        [MemberData(nameof(ChangeAlternativeEmailPostData))]
        public async Task ChangeAlternativeEmailPost_ChangeAlternativeEmailViewWithErrorMessagesIfModelStateOrPasswordIsInvalidOrAlternativeEmailIsInUse(string testPassword, string newAlternativeEmail)
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);
            if (newAlternativeEmail == "inUseAsEmail@email.com")
            {
                await CreateAccount(newAlternativeEmail, password);
            }
            else if (newAlternativeEmail == "inUseAsAltEmail@email.com")
            {
                await CreateAccount("email1@email.com", password);
                await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(2, newAlternativeEmail);
            }

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeAlternativeEmailPostResponse = await ChangeAlternativeEmail(testPassword,
                newAlternativeEmail == "matches@email.com" ? newAlternativeEmail : null,
                newAlternativeEmail,
                applicationCookie);

            // Assert
            Assert.Equal("OK", changeAlternativeEmailPostResponse.StatusCode.ToString());
            string html = await changeAlternativeEmailPostResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangeAlternativeEmail, html);
            if (newAlternativeEmail == "invalidAlternativeEmail")
            {
                Assert.Contains(_stringOptions.ErrorMessage_Email_Invalid, html);
            }
            else if (testPassword == "invalidPassword")
            {
                Assert.Contains(_stringOptions.ErrorMessage_Password_Invalid, html);
            }
            else if (newAlternativeEmail == "matches@email.com")
            {
                Assert.Contains(_stringOptions.ErrorMessage_NewEmail_MustDiffer, html);
            }
            else if (newAlternativeEmail == "inUseAsEmail@email.com" || newAlternativeEmail == "inUseAsAltEmail@email.com")
            {
                Assert.Contains(_stringOptions.ErrorMessage_EmailInUse, html);
            }
        }

        public static IEnumerable<object[]> ChangeAlternativeEmailPostData()
        {
            yield return new object[] { "", "invalidAlternativeEmail" };
            yield return new object[] { "invalidPassword", "new@email.com" };
            yield return new object[] { "Password", "matches@email.com" };
            yield return new object[] { "Password", "inUseAsEmail@email.com" };
            yield return new object[] { "Password", "inUseAsAltEmail@email.com" };
        }

        [Fact]
        public async Task ChangeAlternativeEmailPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newAlternativeEmail = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Password", password },
                {"CurrentAlternativeEmail", null },
                {"NewAlternativeEmail", newAlternativeEmail }
            };
            HttpRequestMessage changeAlternativeEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}",
                HttpMethod.Post,
                formPostBodyData,
                logInPostResponse);

            // Act
            HttpResponseMessage changeAlternativeEmailPostResponse = await _httpClient.SendAsync(changeAlternativeEmailPostRequest);

            // Assert
            Assert.Equal("BadRequest", changeAlternativeEmailPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task ChangeAlternativeEmailPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newAlternativeEmail = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage changeAlternativeEmailGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage changeAlternativeEmailGetResponse = await _httpClient.SendAsync(changeAlternativeEmailGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeAlternativeEmailGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Password", password },
                {"CurrentAlternativeEmail", null },
                {"NewAlternativeEmail", newAlternativeEmail },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changeAlternativeEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}",
                HttpMethod.Post,
                formPostBodyData,
                changeAlternativeEmailGetResponse);

            // Act
            HttpResponseMessage changeAlternativeEmailPostResponse = await _httpClient.SendAsync(changeAlternativeEmailPostRequest);

            // Assert
            Assert.Equal("Redirect", changeAlternativeEmailPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changeAlternativeEmailPostResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangeAlternativeEmailPost_ReturnsErrorViewIfNewAlternativeEmailDiffersFromCurrentAlternativeEmailInValidationButNotInAction()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newAlternativeEmail = "email1@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);
            await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(1, newAlternativeEmail);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeAlternativeEmailPostResponse = await ChangeAlternativeEmail(password, null, newAlternativeEmail, applicationCookie);

            // Assert
            string html = await changeAlternativeEmailPostResponse.Content.ReadAsStringAsync();
            Assert.Equal("OK", changeAlternativeEmailPostResponse.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ViewTitle_Error));
        }

        [Fact]
        public async Task ChangeDisplayNamePost_RedirectsToManageAccountAndUpdatesDisplayNameIfSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newDisplayName = "testDisplayName";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeDisplayNamePostResponse = await ChangeDisplayName(password, null, newDisplayName, applicationCookie);

            // Assert
            Assert.Equal("Redirect", changeDisplayNamePostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", changeDisplayNamePostResponse.Headers.Location.ToString());
            VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
            Assert.Equal(newDisplayName, account.DisplayName);
        }

        [Theory]
        [MemberData(nameof(ChangeDisplayNamePostData))]
        public async Task ChangeDisplayNamePost_ChangeDisplayNameViewWithErrorMessagesIfModelStateOrPasswordIsInvalidOrDisplayNameIsInUse(string testPassword, string newDisplayName)
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);
            if (newDisplayName == "inUseDisplayName")
            {
                await CreateAccount("email1@email.com", password);
                await _vakAccountRepository.UpdateAccountDisplayNameAsync(2, newDisplayName);
            }

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeDisplayNamePostResponse = await ChangeDisplayName(testPassword,
                newDisplayName == "doesNotDiffer" ? newDisplayName : null,
                newDisplayName,
                applicationCookie);

            // Assert
            Assert.Equal("OK", changeDisplayNamePostResponse.StatusCode.ToString());
            string html = await changeDisplayNamePostResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangeDisplayName, html);
            if (newDisplayName == "x")
            {
                Assert.Contains(_stringOptions.ErrorMessage_DisplayName_FormatInvalid, html);
            }
            else if (testPassword == "invalidPassword")
            {
                Assert.Contains(_stringOptions.ErrorMessage_Password_Invalid, html);
            }
            else if (newDisplayName == "inuse@email.com")
            {
                Assert.Contains(_stringOptions.ErrorMessage_DisplayName_InUse, html);
            }
            else if (newDisplayName == "doesNotDiffer")
            {
                Assert.Contains(_stringOptions.ErrorMessage_NewDisplayName_MustDiffer, html);
            }
        }

        public static IEnumerable<object[]> ChangeDisplayNamePostData()
        {
            yield return new object[] { "", "x" };
            yield return new object[] { "invalidPassword", "testDisplayName" };
            yield return new object[] { "Password", "inUseDisplayName" };
            yield return new object[] { "Password", "doesNotDiffer" };
        }

        [Fact]
        public async Task ChangeDisplayNamePost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newDisplayName = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Password", password },
                {"CurrentDisplayName", null },
                {"NewDisplayName", newDisplayName }
            };
            HttpRequestMessage changeDisplayNamePostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}",
                HttpMethod.Post,
                formPostBodyData,
                logInPostResponse);

            // Act
            HttpResponseMessage changeDisplayNamePostResponse = await _httpClient.SendAsync(changeDisplayNamePostRequest);

            // Assert
            Assert.Equal("BadRequest", changeDisplayNamePostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task ChangeDisplayNamePost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newDisplayName = "new@email.com";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage changeDisplayNameGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage changeDisplayNameGetResponse = await _httpClient.SendAsync(changeDisplayNameGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeDisplayNameGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Password", password },
                {"CurrentDisplayName", null },
                {"NewDisplayName", newDisplayName },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changeDisplayNamePostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}",
                HttpMethod.Post,
                formPostBodyData,
                changeDisplayNameGetResponse);

            // Act
            HttpResponseMessage changeDisplayNamePostResponse = await _httpClient.SendAsync(changeDisplayNamePostRequest);

            // Assert
            Assert.Equal("Redirect", changeDisplayNamePostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changeDisplayNamePostResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangeDisplayNamePost_ReturnsErrorViewIfNewDisplayNameDiffersFromCurrentDisplayNameInValidationButNotInAction()
        {
            // Arrange
            string email = "email@email.com", password = "Password", newDisplayName = "displayName";

            await _resetAccountsTable();
            await CreateAccount(email, password);
            await _vakAccountRepository.UpdateAccountDisplayNameAsync(1, newDisplayName);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            // Act
            HttpResponseMessage changeDisplayNamePostResponse = await ChangeDisplayName(password, null, newDisplayName, applicationCookie);

            // Assert
            string html = await changeDisplayNamePostResponse.Content.ReadAsStringAsync();
            Assert.Equal("OK", changeDisplayNamePostResponse.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ViewTitle_Error));
        }

        [Fact]
        public async Task ChangeDisplayNameGet_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Act 
            HttpResponseMessage changeDisplayNameGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}");

            // Assert
            Assert.Equal("Redirect", changeDisplayNameGetResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", changeDisplayNameGetResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task ChangeDisplayNameGet_ReturnsChangeDisplayNameViewWithAntiForgeryCredentialsIfAuthenticationSucceeds()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ChangeDisplayNameGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}",
                HttpMethod.Get,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage changeDisplayNameGetResponse = await _httpClient.SendAsync(ChangeDisplayNameGetRequest);

            // Assert
            Assert.Equal("OK", changeDisplayNameGetResponse.StatusCode.ToString());
            string html = await changeDisplayNameGetResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_ChangeDisplayName, html);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeDisplayNameGetResponse);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(changeDisplayNameGetResponse);
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Theory]
        [MemberData(nameof(EnableTwoFactorPostData))]
        public async Task EnableTwoFactorPost_RedirectsToManageAccountViewIfTwoFactorAlreadyEnabledOrIsSuccessfullyEnabled(bool alreadyEnabled)
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);
            await _vakAccountRepository.UpdateAccountEmailVerifiedAsync(1, true);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);
            if (alreadyEnabled)
            {
                // Only do this after logging into avoid a redirect status code for logInPostResponse
                await _vakAccountRepository.UpdateAccountTwoFactorEnabledAsync(1, true);
            }

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            // Act
            HttpResponseMessage enableTwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

            // Assert
            Assert.Equal("Redirect", enableTwoFactorPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", enableTwoFactorPostResponse.Headers.Location.ToString());
            VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
            Assert.True(account.TwoFactorEnabled);
        }

        public static IEnumerable<object[]> EnableTwoFactorPostData()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        [Fact]
        public async Task EnableTwoFactorPost_RedirectsToTestTwoFactorCodeViewAndSendsTwoFactorEmailIfEmailIsNotVerified()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(CookiesHelper.ExtractCookiesFromResponse(logInPostResponse));
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            // Act
            HttpResponseMessage enableTwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

            // Assert
            Assert.Equal("Redirect", enableTwoFactorPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}", enableTwoFactorPostResponse.Headers.Location.ToString());
            string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(_stringOptions.TwoFactorEmail_Subject, twoFactorEmail);
            Assert.Contains(email, twoFactorEmail);
            Assert.Matches(_stringOptions.TwoFactorEmail_Message.Replace("{0}", @"\d{6,6}"), twoFactorEmail);
        }

        [Fact]
        public async Task EnableTwoFactorPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage enableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.EnableTwoFactor)}",
                HttpMethod.Post,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage enableTwoFactorPostResponse = await _httpClient.SendAsync(enableTwoFactorPostRequest);

            // Assert
            Assert.Equal("BadRequest", enableTwoFactorPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task EnableTwoFactorPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
                $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage enableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.EnableTwoFactor)}",
                HttpMethod.Post,
                formPostBodyData,
                manageAccountGetResponse);

            // Act
            HttpResponseMessage enableTwoFactorPostReponse = await _httpClient.SendAsync(enableTwoFactorPostRequest);

            // Assert
            Assert.Equal("Redirect", enableTwoFactorPostReponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", enableTwoFactorPostReponse.Headers.Location.AbsolutePath);
        }

        [Theory]
        [MemberData(nameof(DisableTwoFactorPostData))]
        public async Task DisableTwoFactorPost_RedirectsToManageAccountViewIfTwoFactorAlreadyDisabledOrIsSuccessfullyDisabled(bool alreadyDisabled)
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);
            if (!alreadyDisabled)
            {
                // Only do this after logging into avoid a redirect status code for logInPostResponse
                await _vakAccountRepository.UpdateAccountTwoFactorEnabledAsync(1, true);
            }

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            // Act
            HttpResponseMessage disableTwoFactorPostResponse = await DisableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

            // Assert
            Assert.Equal("Redirect", disableTwoFactorPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", disableTwoFactorPostResponse.Headers.Location.ToString());
            VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
            Assert.False(account.TwoFactorEnabled);
        }

        public static IEnumerable<object[]> DisableTwoFactorPostData()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        [Fact]
        public async Task DisableTwoFactorPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage disableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.DisableTwoFactor)}",
                HttpMethod.Post,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage disableTwoFactorPostResponse = await _httpClient.SendAsync(disableTwoFactorPostRequest);

            // Assert
            Assert.Equal("BadRequest", disableTwoFactorPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task DisableTwoFactorPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
                $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage disableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.DisableTwoFactor)}",
                HttpMethod.Post,
                formPostBodyData,
                manageAccountGetResponse);

            // Act
            HttpResponseMessage disableTwoFactorPostReponse = await _httpClient.SendAsync(disableTwoFactorPostRequest);

            // Assert
            Assert.Equal("Redirect", disableTwoFactorPostReponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", disableTwoFactorPostReponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task TestTwoFactorGet_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Act 
            HttpResponseMessage testTwoFactorGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}");

            // Assert
            Assert.Equal("Redirect", testTwoFactorGetResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", testTwoFactorGetResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task TestTwoFactorGet_ReturnsTestTwoFactorCodeViewWithAntiForgeryCredentialsIfAuthenticationSucceeds()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage TestTwoFactorGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
                HttpMethod.Get,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage testTwoFactorGetResponse = await _httpClient.SendAsync(TestTwoFactorGetRequest);

            // Assert
            Assert.Equal("OK", testTwoFactorGetResponse.StatusCode.ToString());
            string html = await testTwoFactorGetResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_TestTwoFactor, html);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(testTwoFactorGetResponse);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(testTwoFactorGetResponse);
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Fact]
        public async Task TestTwoFactorPost_RedirectsToManageAccountAndUpdatesEmailVerifiedAndTwoFactorEnabledIfSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(CookiesHelper.ExtractCookiesFromResponse(logInPostResponse));
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            HttpResponseMessage enabletwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

            string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

            // Act
            HttpResponseMessage testTwoFactorPostResponse = await TestTwoFactor(code, applicationCookie);

            // Assert
            Assert.Equal("Redirect", testTwoFactorPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", testTwoFactorPostResponse.Headers.Location.ToString());
            VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
            Assert.True(account.EmailVerified);
            Assert.True(account.TwoFactorEnabled);
        }

        [Theory]
        [MemberData(nameof(TestTwoFactorPostData))]
        public async Task TestTwoFactorPost_TestTwoFactorViewWithErrorMessageIfModelStateIsInvalidOrCodeIsInvalid(string code)
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(CookiesHelper.ExtractCookiesFromResponse(logInPostResponse));
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            HttpResponseMessage enabletwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

            // Act
            HttpResponseMessage testTwoFactorPostResponse = await TestTwoFactor(code, applicationCookie);

            // Assert
            Assert.Equal("OK", testTwoFactorPostResponse.StatusCode.ToString());
            string html = await testTwoFactorPostResponse.Content.ReadAsStringAsync();
            Assert.Contains(_stringOptions.ViewTitle_TestTwoFactor, html);
            Assert.Contains(_stringOptions.ErrorMessage_TwoFactorCode_Invalid, html);
        }

        public static IEnumerable<object[]> TestTwoFactorPostData()
        {
            // Invalid format
            yield return new object[] { "12345" };
            // Invalid code
            yield return new object[] { "123456" };
        }

        [Fact]
        public async Task TestTwoFactorPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Code", "123456" }
            };
            HttpRequestMessage testTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
                HttpMethod.Post,
                formPostBodyData,
                logInPostResponse);

            // Act
            HttpResponseMessage testTwoFactorPostResponse = await _httpClient.SendAsync(testTwoFactorPostRequest);

            // Assert
            Assert.Equal("BadRequest", testTwoFactorPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task TestTwoFactorPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage testTwoFactorGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage testTwoFactorGetResponse = await _httpClient.SendAsync(testTwoFactorGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(testTwoFactorGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Code", "123456" },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage testTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
                HttpMethod.Post,
                formPostBodyData,
                testTwoFactorGetResponse);

            // Act
            HttpResponseMessage testTwoFactorPostResponse = await _httpClient.SendAsync(testTwoFactorPostRequest);

            // Assert
            Assert.Equal("Redirect", testTwoFactorPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", testTwoFactorPostResponse.Headers.Location.AbsolutePath);
        }

        [Fact]
        public async Task SendEmailVerificationEmailPost_RedirectsToSendEmailVerificationEmailConfirmationViewAndSendsEmailVerificationEmailIfSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            // Act
            HttpResponseMessage sendEmailVerificationEmailPostResponse = await SendEmailVerificationEmail(applicationCookie, antiForgeryCookie, antiForgeryToken);

            // Assert
            Assert.Equal("Redirect", sendEmailVerificationEmailPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}?Email={email}", sendEmailVerificationEmailPostResponse.Headers.Location.ToString());
            string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(_stringOptions.Email_EmailVerification_Subject, emailVerificationEmail);
            Assert.Contains(email, emailVerificationEmail);
            Assert.Matches(_stringOptions.Email_EmailVerification_Message.Replace("{0}", $"http://localhost/{_accountControllerName}/{nameof(AccountController.EmailVerificationConfirmation)}.*?"), emailVerificationEmail);
        }

        [Fact]
        public async Task SendEmailVerificationEmailPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage sendEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmail)}",
                HttpMethod.Post,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage sendEmailVerificationEmailPostResponse = await _httpClient.SendAsync(sendEmailVerificationEmailPostRequest);

            // Assert
            Assert.Equal("BadRequest", sendEmailVerificationEmailPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task SendEmailVerificationEmailPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
                $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage sendEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmail)}",
                HttpMethod.Post,
                formPostBodyData,
                manageAccountGetResponse);

            // Act
            HttpResponseMessage sendEmailVerificationEmailPostReponse = await _httpClient.SendAsync(sendEmailVerificationEmailPostRequest);

            // Assert
            Assert.Equal("Redirect", sendEmailVerificationEmailPostReponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", sendEmailVerificationEmailPostReponse.Headers.Location.AbsolutePath);
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
            // TODO why not just grab email verification link?
            HttpResponseMessage emailVerificationConfirmationGetResponse = await _httpClient.GetAsync($"Account/EmailVerificationConfirmation?AccountId={accountId}&Token={token}");

            // Assert
            Assert.True((await _vakAccountRepository.GetAccountAsync(accountId)).EmailVerified);
            Assert.Equal("OK", emailVerificationConfirmationGetResponse.StatusCode.ToString());
            string html = await emailVerificationConfirmationGetResponse.Content.ReadAsStringAsync();
            Assert.True(html.Contains(_stringOptions.ViewTitle_EmailVerificationConfirmation));
        }

        [Fact]
        public async Task SendAlternativeEmailVerificationEmailPost_RedirectsToSendEmailVerificationEmailConfirmationViewAndSendsAlternativeEmailVerificationEmailIfSuccessful()
        {
            // Arrange
            string email = "email@email.com", password = "Password", alternativeEmail = "alt@email.com";

            await _resetAccountsTable();
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await CreateAccount(email, password);
            await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(1, alternativeEmail);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            // Act
            HttpResponseMessage sendAlternativeEmailVerificationEmailPostResponse = await SendAlternativeEmailVerificationEmail(applicationCookie, antiForgeryCookie, antiForgeryToken);

            // Assert
            Assert.Equal("Redirect", sendAlternativeEmailVerificationEmailPostResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}", sendAlternativeEmailVerificationEmailPostResponse.Headers.Location.ToString());
            string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(_stringOptions.Email_EmailVerification_Subject, emailVerificationEmail);
            Assert.Contains(alternativeEmail, emailVerificationEmail);
            Assert.Matches(_stringOptions.Email_EmailVerification_Message.Replace("{0}", $"http://localhost/{_accountControllerName}/{nameof(AccountController.AlternativeEmailVerificationConfirmation)}.*?"), emailVerificationEmail);
        }

        [Fact]
        public async Task SendAlternativeEmailVerificationEmailPost_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage sendAlternativeEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendAlternativeEmailVerificationEmail)}",
                HttpMethod.Post,
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage sendAlternativeEmailVerificationEmailPostResponse = await _httpClient.SendAsync(sendAlternativeEmailVerificationEmailPostRequest);

            // Assert
            Assert.Equal("BadRequest", sendAlternativeEmailVerificationEmailPostResponse.StatusCode.ToString());
        }

        [Fact]
        public async Task SendAlternativeEmailVerificationEmailPost_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Arrange
            string email = "email@email.com", password = "Password";

            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
                $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
                HttpMethod.Get,
                null,
                logInPostResponse);
            HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage sendAlternativeEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendAlternativeEmailVerificationEmail)}",
                HttpMethod.Post,
                formPostBodyData,
                manageAccountGetResponse);

            // Act
            HttpResponseMessage sendAlternativeEmailVerificationEmailPostReponse = await _httpClient.SendAsync(sendAlternativeEmailVerificationEmailPostRequest);

            // Assert
            Assert.Equal("Redirect", sendAlternativeEmailVerificationEmailPostReponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", sendAlternativeEmailVerificationEmailPostReponse.Headers.Location.AbsolutePath);
        }

        [Theory]
        [MemberData(nameof(AlternativeEmailVerificationConfirmationGetData))]
        public async Task AlternativeEmailVerificationConfirmationGet_ReturnsErrorViewIfAccountIdTokenOrModelStateIsInvalid(string accountId, string token)
        {
            // Arrange
            if (accountId == "1")
            {
                await _resetAccountsTable();
                await CreateAccount("test@test.com", "Password");
            }

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.AlternativeEmailVerificationConfirmation)}?Email={accountId}&Token={token}");

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());

            Assert.True(html.Contains(_stringOptions.ViewTitle_Error));
        }

        public static IEnumerable<object[]> AlternativeEmailVerificationConfirmationGetData()
        {
            yield return new object[] { "", "" };
            yield return new object[] { "1", "invalidtoken" };
        }

        [Fact]
        public async Task AlternativeEmailVerificationConfirmationGet_ReturnsAlternativeEmailVerificationConfirmationViewIfEmailTokenAndModelStateAreValid()
        {
            // Arrange
            string email = "Email1@test.com", password = "Password", alternativeEmail = "alt@email.com";
            await _resetAccountsTable();
            File.WriteAllText(@"Temp\SmtpTest.txt", "");
            await CreateAccount(email, password);
            await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(1, alternativeEmail);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);
            IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

            HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
            IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

            HttpResponseMessage sendAlternativeEmailVerificationEmailPostResponse = await SendAlternativeEmailVerificationEmail(applicationCookie, antiForgeryCookie, antiForgeryToken);

            string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Regex tokenRegex = new Regex(@"Token=(.*?)&");
            string token = tokenRegex.Match(emailVerificationEmail).Groups[1].Value;

            // Act
            // TODO why not just grab email verification link?
            HttpResponseMessage emailVerificationConfirmationGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.AlternativeEmailVerificationConfirmation)}?AccountId={1}&Token={token}");

            // Assert
            Assert.True((await _vakAccountRepository.GetAccountAsync(1)).AlternativeEmailVerified);
            Assert.Equal("OK", emailVerificationConfirmationGetResponse.StatusCode.ToString());
            string html = await emailVerificationConfirmationGetResponse.Content.ReadAsStringAsync();
            Assert.True(html.Contains(_stringOptions.ViewTitle_AlternativeEmailVerificationConfirmation));
        }

        [Fact]
        public async Task SendEmailVerificationEmailConfirmationGet_ReturnsSendEmailVerificationEmailViewIfAuthenticationSucceeds()
        {
            // Arrange
            string email = "Email1@test.com", password = "Password";
            await _resetAccountsTable();
            await CreateAccount(email, password);

            HttpResponseMessage logInPostResponse = await LogIn(email, password);

            HttpRequestMessage sendEmailVerificationEmailConfirmationGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}?Email={email}",
                HttpMethod.Get, 
                null,
                logInPostResponse);

            // Act
            HttpResponseMessage sendEmailVerificationEmailConfirmationGetResponse = await _httpClient.SendAsync(sendEmailVerificationEmailConfirmationGetRequest);

            // Assert
            string html = await sendEmailVerificationEmailConfirmationGetResponse.Content.ReadAsStringAsync();
            Assert.Equal("OK", sendEmailVerificationEmailConfirmationGetResponse.StatusCode.ToString());
            Assert.True(html.Contains(_stringOptions.ViewTitle_SendEmailVerificationEmailConfirmation));
        }

        [Fact]
        public async Task SendAlternativeEmailVerificationEmailConfirmationGet_RedirectsToLogInViewIfAuthenticationFails()
        {
            // Act
            HttpResponseMessage sendEmailVerificationEmailConfirmationGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}?Email=email@email.com");

            // Assert
            Assert.Equal("Redirect", sendEmailVerificationEmailConfirmationGetResponse.StatusCode.ToString());
            Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", sendEmailVerificationEmailConfirmationGetResponse.Headers.Location.AbsolutePath);
        }

        #region Helpers
        public async Task<HttpResponseMessage> SendAlternativeEmailVerificationEmail(IDictionary<string, string> applicationCookie, IDictionary<string, string> antiForgeryCookie,
            string antiForgeryToken)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage sendAlternativeEmailVerificationEmailPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SendAlternativeEmailVerificationEmail)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(sendAlternativeEmailVerificationEmailPostRequest, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(sendAlternativeEmailVerificationEmailPostRequest, antiForgeryCookie);

            return await _httpClient.SendAsync(sendAlternativeEmailVerificationEmailPostRequest);
        }

        public async Task<HttpResponseMessage> SendEmailVerificationEmail(IDictionary<string, string> applicationCookie, IDictionary<string, string> antiForgeryCookie,
            string antiForgeryToken)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage sendEmailVerificationEmailPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmail)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(sendEmailVerificationEmailPostRequest, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(sendEmailVerificationEmailPostRequest, antiForgeryCookie);

            return await _httpClient.SendAsync(sendEmailVerificationEmailPostRequest);
        }

        public async Task<HttpResponseMessage> TestTwoFactor(string code, IDictionary<string, string> applicationCookie)
        {
            HttpRequestMessage testTwoFactorGetRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
                HttpMethod.Get,
                null);
            CookiesHelper.PutCookiesOnRequest(testTwoFactorGetRequest, applicationCookie);
            HttpResponseMessage testTwoFactorGetResponse = await _httpClient.SendAsync(testTwoFactorGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(testTwoFactorGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"Code", code },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage testTwoFactorPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(testTwoFactorPostRequest, applicationCookie);
            CookiesHelper.CopyCookiesFromResponse(testTwoFactorPostRequest, testTwoFactorGetResponse);

            return await _httpClient.SendAsync(testTwoFactorPostRequest);
        }

        public async Task<HttpResponseMessage> DisableTwoFactor(IDictionary<string, string> applicationCookie, IDictionary<string, string> antiForgeryCookie,
            string antiForgeryToken)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage disableTwoFactorPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.DisableTwoFactor)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(disableTwoFactorPostRequest, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(disableTwoFactorPostRequest, antiForgeryCookie);

            return await _httpClient.SendAsync(disableTwoFactorPostRequest);
        }

        public async Task<HttpResponseMessage> EnableTwoFactor(IDictionary<string, string> applicationCookie, IDictionary<string, string> antiForgeryCookie, 
            string antiForgeryToken)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage enableTwoFactorPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.EnableTwoFactor)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(enableTwoFactorPostRequest, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(enableTwoFactorPostRequest, antiForgeryCookie);

            return await _httpClient.SendAsync(enableTwoFactorPostRequest);
        }

        public async Task<HttpResponseMessage> ManageAccount(IDictionary<string, string> applicationCookie)
        {
            HttpRequestMessage manageAccountGetRequest = RequestHelper.Create(
                $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
                HttpMethod.Get,
                null);
            CookiesHelper.PutCookiesOnRequest(manageAccountGetRequest, applicationCookie);

            return await _httpClient.SendAsync(manageAccountGetRequest);
        }

        public async Task<HttpResponseMessage> ChangeDisplayName(string password, string currentDisplayName, string newDisplayName, 
            IDictionary<string, string> applicationCookie)
        {
            HttpRequestMessage changeDisplayNameGetRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}",
                HttpMethod.Get,
                null);
            CookiesHelper.PutCookiesOnRequest(changeDisplayNameGetRequest, applicationCookie);
            HttpResponseMessage changeDisplayNameGetResponse = await _httpClient.SendAsync(changeDisplayNameGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeDisplayNameGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"CurrentDisplayName", currentDisplayName },
                {"Password", password },
                {"NewDisplayName", newDisplayName },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changeDisplayNamePostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(changeDisplayNamePostRequest, applicationCookie);
            CookiesHelper.CopyCookiesFromResponse(changeDisplayNamePostRequest, changeDisplayNameGetResponse);

            return await _httpClient.SendAsync(changeDisplayNamePostRequest);
        }

        public async Task<HttpResponseMessage> ChangeAlternativeEmail(string password, string currentAlternativeEmail, string newAlternativeEmail, 
            IDictionary<string, string> applicationCookie)
        {
            HttpRequestMessage changeAlternativeEmailGetRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}",
                HttpMethod.Get,
                null);
            CookiesHelper.PutCookiesOnRequest(changeAlternativeEmailGetRequest, applicationCookie);
            HttpResponseMessage changeAlternativeEmailGetResponse = await _httpClient.SendAsync(changeAlternativeEmailGetRequest);

            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(changeAlternativeEmailGetResponse);
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                {"CurrentAlternativeEmail", currentAlternativeEmail },
                {"Password", password },
                {"NewAlternativeEmail", newAlternativeEmail },
                { "__RequestVerificationToken", antiForgeryToken }
            };
            HttpRequestMessage changeAlternativeEmailPostRequest = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}",
                HttpMethod.Post,
                formPostBodyData);
            CookiesHelper.PutCookiesOnRequest(changeAlternativeEmailPostRequest, applicationCookie);
            CookiesHelper.CopyCookiesFromResponse(changeAlternativeEmailPostRequest, changeAlternativeEmailGetResponse);

            return await _httpClient.SendAsync(changeAlternativeEmailPostRequest);
        }

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

        public async Task<HttpResponseMessage> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword, 
            IDictionary<string, string> applicationCookie)
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
                await _vakAccountRepository.UpdateAccountEmailVerifiedAsync(account.AccountId, true);
            }

            return account;
        }

        #endregion
    }
}



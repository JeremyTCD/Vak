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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.SignUpView_Title));
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Theory]
        [MemberData(nameof(SignUpPostData))]
        public async Task SignUpPost_ReturnsSignUpViewWithErrorMessagesIfModelStateIsInvalidOrCreateAccountFails(string email, string password, string confirmPassword)
        {
            // Arrange
            await _resetAccountsTable();
            if (email == "Email1@test.com") {
                await _vakAccountRepository.CreateAccountAsync(email, password);
            }

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(email, password, confirmPassword);

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            StringOptions stringOptions = new StringOptions();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(html.Contains(stringOptions.SignUpView_Title));
            if (email == "Email1@test.com")
            {
                Assert.True(html.Contains(stringOptions.SignUp_AccountWithEmailExists));
            }
            else
            {
                Assert.True(html.Contains(stringOptions.Email_Invalid));
                Assert.True(html.Contains(stringOptions.Password_Invalid));
                Assert.True(html.Contains(stringOptions.ConfirmPassword_DoesNotMatchPassword));
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
            StringOptions stringOptions = new StringOptions();
            string confirmEmailEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(stringOptions.ConfirmEmail_Subject, confirmEmailEmail);
            Assert.Contains("Email1@test.com", confirmEmailEmail);
            Assert.Matches(stringOptions.ConfirmEmail_Message.Replace("{0}", $"http://localhost/{nameof(AccountController).Replace("Controller", "")}/{nameof(AccountController.EmailVerificationConfirmation)}.*?"), confirmEmailEmail);
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.LogInView_Title));
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
            StringOptions stringOptions = new StringOptions();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(html.Contains(stringOptions.LogInView_Title));
            Assert.True(html.Contains(stringOptions.LogIn_Failed));
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
            StringOptions stringOptions = new StringOptions();
            string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(stringOptions.TwoFactorEmail_Subject, twoFactorEmail);
            Assert.Contains(email, twoFactorEmail);
            Assert.Matches(stringOptions.TwoFactorEmail_Message.Replace("{0}", @"\d{6,6}"), twoFactorEmail);
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
            Assert.Equal("/Account/Login", logOffPostResponse.Headers.Location.AbsolutePath);
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.VerifyTwoFactorCodeView_Title));
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.ErrorView_Title));
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.VerifyTwoFactorCodeView_Title));
            Assert.True(html.Contains(stringOptions.VerifyTwoFactorCode_InvalidCode));
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.ForgotPasswordView_Title));
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
            StringOptions stringOptions = new StringOptions();
            Assert.Equal("OK", forgotPasswordPostResponse.StatusCode.ToString());
            Assert.True(html.Contains(stringOptions.ForgotPasswordView_Title));
            Assert.True(html.Contains(stringOptions.Email_Invalid));
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
            Assert.Equal($"/{nameof(AccountController).Replace("Controller", "")}/{nameof(AccountController.ForgotPasswordConfirmation)}?Email={email}", 
                forgotPasswordPostResponse.Headers.Location.ToString());
            StringOptions stringOptions = new StringOptions();
            string resetPasswordEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            if (email == "valid@email")
            {
                Assert.Contains(stringOptions.ResetPasswordEmail_Subject, resetPasswordEmail);
                Assert.Contains(email, resetPasswordEmail);
                Assert.Matches(string.Format(stringOptions.ResetPasswordEmail_Message, 
                    $"http://localhost/{nameof(AccountController).Replace("Controller", "")}/{nameof(AccountController.ResetPassword)}.*?"), resetPasswordEmail);
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.ForgotPasswordConfirmationView_Title));
        }

        [Fact]
        public async Task ResetPasswordGet_ReturnsResetPasswordViewWithAntiForgeryTokenAndCookie()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/ResetPassword?Token=token&Email=email");

            // Assert
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(httpResponseMessage);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            string html = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.ResetPasswordView_Title));
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
            StringOptions stringOptions = new StringOptions();
            Assert.Equal("OK", resetPasswordPostResponse.StatusCode.ToString());
            Assert.True(html.Contains(stringOptions.Password_Invalid));
            Assert.True(html.Contains(stringOptions.ConfirmPassword_DoesNotMatchPassword));
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
            Assert.Equal($"/{nameof(AccountController).Replace("Controller","")}/{nameof(AccountController.ResetPasswordConfirmation)}?Email={email}", 
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.ErrorView_Title));
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.ResetPasswordConfirmationView_Title));
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
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.ErrorView_Title));
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
            Assert.True((await _vakAccountRepository.GetAccountAsync(accountId)).EmailConfirmed);
            Assert.Equal("OK", emailVerificationConfirmationGetResponse.StatusCode.ToString());
            string html = await emailVerificationConfirmationGetResponse.Content.ReadAsStringAsync();
            StringOptions stringOptions = new StringOptions();
            Assert.True(html.Contains(stringOptions.EmailVerificationConfirmationView_Title));
        }

        #region Helpers
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
            if(emailConfirmed)
            {
                await _vakAccountRepository.UpdateAccountEmailConfirmedAsync(account.AccountId);
            }

            return account;
        }

        #endregion
    }
}


  
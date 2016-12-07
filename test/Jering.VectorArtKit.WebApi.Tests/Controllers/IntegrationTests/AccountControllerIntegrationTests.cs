using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.FormModels;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.Account;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [Collection("ControllersCollection")]
    public class AccountControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        private VakAccountRepository _vakAccountRepository { get; }
        private Func<Task> _resetAccountsTable { get; }

        private string _accountControllerName { get; } = nameof(AccountController).Replace("Controller", "");
        private string _dynamicFormsControllerName { get; } = nameof(DynamicFormsController).Replace("Controller", "");
        private string _badRequest { get; } = "BadRequest";
        private string _unauthorized { get; } = "Unauthorized";
        private string _ok { get; } = "OK";
        private string _tempEmailFile { get; } = $"{Environment.GetEnvironmentVariable("TMP")}\\SmtpTest.txt";
        private string _applicationCookieName { get; } = "Jering.Application";
        private string _twoFactorCookieName { get; } = "Jering.TwoFactor";
        private string _testEmail1 { get; } = "test@email1.com";
        private string _testEmail2 { get; } = "test@email2.com";
        private string _testNewEmail { get; } = "testNew@email.com";
        private string _testNewAlternativeEmail { get; } = "testNewAlternative@email.com";
        private string _testNewPassword { get; } = "testNewPassword";
        private string _testInvalidPassword { get; } = "testInvalidPassword";
        private string _testPassword { get; } = "testPassword";
        private string _testToken { get; } = "testToken";
        private string _testNewDisplayName { get; } = "testNewDisplayName";
        public AccountControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
            _vakAccountRepository = controllersFixture.VakAccountRepository;
            _resetAccountsTable = controllersFixture.ResetAccountsTable;
        }

        [Fact]
        public async Task SignUp_Returns400BadRequestAndSignUpResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp("test", "test1", "test2");

            // Assert
            ChangePasswordResponseModel body = JsonConvert.DeserializeObject<ChangePasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_Invalid, (body.ModelState[nameof(SignUpFormModel.Email)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_Password_TooSimple, (body.ModelState[nameof(SignUpFormModel.Password)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_ConfirmPassword_Differs, (body.ModelState[nameof(SignUpFormModel.ConfirmPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task SignUp_Returns400BadRequestAndSignUpResponseModelIfEmailInUse()
        {
            // Arrange
            await _resetAccountsTable();
            await _vakAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(_testEmail1, _testPassword, _testPassword);

            // Assert
            ChangePasswordResponseModel body = JsonConvert.DeserializeObject<ChangePasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_InUse, (body.ModelState[nameof(SignUpFormModel.Email)] as JArray)[0]);
        }

        [Fact]
        public async Task SignUp_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            await _resetAccountsTable();

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", _testEmail1},
                { "Password", _testPassword},
                { "ConfirmPassword",  _testPassword}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SignUp)}", HttpMethod.Post, formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SignUp_Returns200OkSignUpResponseModelApplicationCookieAndSendsEmailVerificationEmailIfRegistrationSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(_testEmail1, _testPassword, _testPassword);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(_applicationCookieName, cookies.Keys.First());
            SignUpResponseModel body = JsonConvert.DeserializeObject<SignUpResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_testEmail1, body.Username);
            string emailVerificationEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_EmailVerification, emailVerificationEmail);
            Assert.Contains(_testEmail1, emailVerificationEmail);
        }

        [Fact]
        public async Task Login_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", _testEmail1},
                { "Password", _testPassword}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.LogIn)}", HttpMethod.Post, formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task Login_Returns400BadRequestAndLogInResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn("", "");

            // Assert
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_Required, (body.ModelState[nameof(LogInFormModel.Email)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_Password_Required, (body.ModelState[nameof(LogInFormModel.Password)] as JArray)[0]);
        }

        [Fact]
        public async Task Login_Returns400BadRequestAndELogInResponseModelIfCredentialsAreInvalid()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Assert
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_LogIn_Failed, body.ErrorMessage);
        }

        [Fact]
        public async Task Login_Returns200OkLoginResponseModelAndApplicationCookieIfLoginSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await _vakAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(_applicationCookieName, cookies.Keys.First());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_testEmail1, body.Username);
            Assert.False(body.TwoFactorRequired);
            Assert.False(body.IsPersistent);
        }

        [Fact]
        public async Task Login_Returns200OkLoginResponseModelTwoFactorCookieAndSendsTwoFactorEmailIfTwoFactorAuthenticationIsRequired()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword, true);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(_twoFactorCookieName, cookies.Keys.First());
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_TwoFactorCode, twoFactorEmail);
            Assert.Contains(_testEmail1, twoFactorEmail);
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(null, body.Username);
            Assert.True(body.TwoFactorRequired);
            Assert.False(body.IsPersistent);
        }

        [Fact]
        public async Task LogOff_Returns200OkAndTwoFactorCookieAndApplicationCookieIfAuthenticationIsSuccessful()
        {
            //Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);
            HttpRequestMessage httpRequestMessage = RequestHelper.CreateWithCookiesFromResponse(
                $"{ _accountControllerName}/{ nameof(AccountController.LogOff)}",
                HttpMethod.Post,
                null,
                httpResponseMessage);

            // Act
            httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(2, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
            Assert.Equal("", cookies[_applicationCookieName]);
            Assert.Contains(_twoFactorCookieName, cookies.Keys);
            Assert.Equal("", cookies[_twoFactorCookieName]);
        }

        [Fact]
        public async Task LogOff_Returns401UnauthorizedWithErrorResponseModelIfAuthenticationFails()
        {
            //Arrange
            await _resetAccountsTable();

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.
                SendAsync(RequestHelper.Create($"{ _accountControllerName}/{ nameof(AccountController.LogOff)}", HttpMethod.Post));

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns200OkAndTwoFactorLogInResponseModelTwoFactorCookieAndApplicationCookieIfLoginSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword, true);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

            // Act
            httpResponseMessage = await TwoFactorLogIn(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage), code);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(2, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
            Assert.Contains(_twoFactorCookieName, cookies.Keys);
            Assert.Equal("", cookies[_twoFactorCookieName]);
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            TwoFactorLogInResponseModel body = JsonConvert.DeserializeObject<TwoFactorLogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_testEmail1, body.Username);
            Assert.False(body.IsPersistent);
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns400BadRequestAndTwoFactorResponseModelIfCodeIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword, true);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;
            string testCode = "000000" == code ? "111111" : "000000";

            // Act
            httpResponseMessage = await TwoFactorLogIn(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage), testCode);

            // Assert
            TwoFactorLogInResponseModel body = JsonConvert.DeserializeObject<TwoFactorLogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_TwoFactorCode_InvalidOrExpired, (body.ModelState[nameof(TwoFactorLogInFormModel.Code)] as JArray)[0]);
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns400BadRequestAndTwoFactorLogInResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword, true);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

            // Act
            httpResponseMessage = await TwoFactorLogIn(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage), "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            TwoFactorLogInResponseModel body = JsonConvert.DeserializeObject<TwoFactorLogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_TwoFactorCode_Required, (body.ModelState[nameof(TwoFactorLogInFormModel.Code)] as JArray)[0]);
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Code", "000000"}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.TwoFactorLogIn)}", HttpMethod.Post, formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns200OkAndSendsResetPasswordEmailIfEmailIsValid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(_testEmail1);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_ResetPassword, resetPasswordEmail);
            Assert.Contains(_testEmail1, resetPasswordEmail);
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns200OkIfEmailIsInvalid()
        {
            await _resetAccountsTable();

            // Act
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(_testEmail1);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns400BadRequestAndSendResetPasswordEmailResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail("");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SendResetPasswordResponseModel body = JsonConvert.DeserializeObject<SendResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_Required, (body.ModelState[nameof(SendResetPasswordEmailFormModel.Email)] as JArray)[0]);
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", _testEmail1}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create(
                $"{_accountControllerName}/{nameof(AccountController.SendResetPasswordEmail)}",
                HttpMethod.Post,
                formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(
                await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ResetPassword_Returns400BadRequestAndResetPasswordResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(_testEmail1);
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);

            // Act
            httpResponseMessage = await ResetPassword(_testEmail1, token, "", "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ResetPasswordResponseModel body = JsonConvert.DeserializeObject<ResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, (body.ModelState[nameof(ResetPasswordFormModel.NewPassword)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_ConfirmPassword_Required, (body.ModelState[nameof(ResetPasswordFormModel.ConfirmPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task ResetPassword_Returns400BadRequestAndResetPasswordResponseModelIfTokenIsInvalidOrExpired()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await ResetPassword(_testEmail1,
                _testToken,
                _testNewPassword,
                _testNewPassword);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ResetPasswordResponseModel body = JsonConvert.DeserializeObject<ResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.True(body.LinkExpiredOrInvalid);
        }

        [Fact]
        public async Task ResetPassword_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", _testEmail1 },
                { "Token", _testToken },
                { "NewPassword", _testNewPassword },
                { "ConfirmPassword", _testNewPassword }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.ResetPassword)}",
                HttpMethod.Post,
                formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ResetPassword_Returns200OkIfPasswordResetSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(_testEmail1);
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);

            // Act
            httpResponseMessage = await ResetPassword(_testEmail1, token, _testNewPassword, _testNewPassword);

            // Assert
            ResetPasswordResponseModel body = JsonConvert.DeserializeObject<ResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.NotNull(await _vakAccountRepository.GetAccountByEmailAndPasswordAsync(_testEmail1, _testNewPassword));
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            Assert.Equal(_testEmail1, body.Email);
        }

        [Fact]
        public async Task GetAccountDetails_Returns200OkAndGetAccountDetailsResponseModelIfAuthenticationSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);

            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);
            HttpRequestMessage manageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.GetAccountDetails)}",
                HttpMethod.Get,
                null,
                httpResponseMessage);

            // Act
            httpResponseMessage = await _httpClient.SendAsync(manageAccountGetRequest);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            GetAccountDetailsResponseModel body = JsonConvert.DeserializeObject<GetAccountDetailsResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Null(body.AlternativeEmail);
            Assert.False(body.AlternativeEmailVerified);
            Assert.Null(body.DisplayName);
            Assert.NotNull(body.DurationSinceLastPasswordChange);
            Assert.Equal(_testEmail1, body.Email);
            Assert.False(body.EmailVerified);
            Assert.False(body.TwoFactorEnabled);
        }

        [Fact]
        public async Task GetAccountDetails_Returns401UnauthorizedWithErrorResponseModelIfAuthenticationFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.
                SendAsync(RequestHelper.Create($"{ _accountControllerName}/{ nameof(AccountController.GetAccountDetails)}", HttpMethod.Get));

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangePassword_Returns401UnauthorizedWithErrorResponseModelIfAuthenticationFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.
                SendAsync(RequestHelper.Create($"{ _accountControllerName}/{ nameof(AccountController.ChangePassword)}", HttpMethod.Post));

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangePassword_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            HttpRequestMessage httpRequestMessage = RequestHelper.
                CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}",
                HttpMethod.Post,
                null,
                httpResponseMessage);

            // Act
            httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangePassword_Returns200OKAndApplicationCookieIfPasswordChangeSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangePassword(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testPassword,
                _testNewPassword,
                _testNewPassword);

            // Assert
            ChangePasswordResponseModel body = JsonConvert.DeserializeObject<ChangePasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(1, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
        }

        [Fact]
        public async Task ChangePassword_Returns400BadRequestAndChangePasswordResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangePassword(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                "",
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangePasswordResponseModel body = JsonConvert.DeserializeObject<ChangePasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, (body.ModelState[nameof(ChangePasswordFormModel.CurrentPassword)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewPassword_Required, (body.ModelState[nameof(ChangePasswordFormModel.NewPassword)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_ConfirmPassword_Required, (body.ModelState[nameof(ChangePasswordFormModel.ConfirmNewPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangePassword_Returns400BadRequestAndChangePasswordResponseModelIfCurrentPasswordIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangePassword(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testInvalidPassword,
                _testNewPassword,
                _testNewPassword);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangePasswordResponseModel body = JsonConvert.DeserializeObject<ChangePasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid, (body.ModelState[nameof(ChangePasswordFormModel.CurrentPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeEmail_Returns401UnauthorizedWithErrorResponseModelIfAuthenticationFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.
                SendAsync(RequestHelper.Create($"{ _accountControllerName}/{ nameof(AccountController.ChangeEmail)}", HttpMethod.Post));

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangeEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            HttpRequestMessage httpRequestMessage = RequestHelper.
                CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}",
                HttpMethod.Post,
                null,
                httpResponseMessage);

            // Act
            httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangeEmail_Returns200OKAndApplicationCookieIfEmailChangeSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testPassword,
                _testNewEmail);

            // Assert
            ChangeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(1, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
        }

        [Fact]
        public async Task ChangeEmail_Returns400BadRequestAndChangeEmailResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, 
                (body.ModelState[nameof(ChangeEmailFormModel.Password)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewEmail_Required, 
                (body.ModelState[nameof(ChangeEmailFormModel.NewEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeEmail_Returns400BadRequestAndChangeEmailResponseModelIfPasswordIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testInvalidPassword,
                _testNewEmail);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid, (body.ModelState[nameof(ChangeEmailFormModel.Password)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeEmail_Returns400BadRequestAndChangeEmailResponseModelIfNewEmailIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testPassword,
                _testEmail1);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_InUse, (body.ModelState[nameof(ChangeEmailFormModel.NewEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeAlternativeEmail_Returns401UnauthorizedWithErrorResponseModelIfAuthenticationFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.
                SendAsync(RequestHelper.Create($"{ _accountControllerName}/{ nameof(AccountController.ChangeAlternativeEmail)}", HttpMethod.Post));

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangeAlternativeEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            HttpRequestMessage httpRequestMessage = RequestHelper.
                CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}",
                HttpMethod.Post,
                null,
                httpResponseMessage);

            // Act
            httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangeAlternativeEmail_Returns200OKIfAlternativeEmailChangeSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeAlternativeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testPassword,
                _testNewAlternativeEmail);

            // Assert
            ChangeAlternativeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeAlternativeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task ChangeAlternativeEmail_Returns400BadRequestAndChangeAlternativeEmailResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeAlternativeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeAlternativeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeAlternativeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, 
                (body.ModelState[nameof(ChangeAlternativeEmailFormModel.Password)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewAlternativeEmail_Required, 
                (body.ModelState[nameof(ChangeAlternativeEmailFormModel.NewAlternativeEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeAlternativeEmail_Returns400BadRequestAndChangeAlternativeEmailResponseModelIfPasswordIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeAlternativeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testInvalidPassword,
                _testNewAlternativeEmail);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeAlternativeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeAlternativeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid, 
                (body.ModelState[nameof(ChangeAlternativeEmailFormModel.Password)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeAlternativeEmail_Returns400BadRequestAndChangeAlternativeEmailResponseModelIfNewAlternativeEmailIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);
            VakAccount account = await CreateAccount(_testEmail2, _testPassword);
            await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(account.AccountId, _testNewAlternativeEmail);

            // Act
            httpResponseMessage = await ChangeAlternativeEmail(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testPassword,
                _testNewAlternativeEmail);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeAlternativeEmailResponseModel body = JsonConvert.DeserializeObject<ChangeAlternativeEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_InUse, 
                (body.ModelState[nameof(ChangeAlternativeEmailFormModel.NewAlternativeEmail)] as JArray)[0]);
        }

        public async Task ChangeDisplayName_Returns401UnauthorizedWithErrorResponseModelIfAuthenticationFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.
                SendAsync(RequestHelper.Create($"{ _accountControllerName}/{ nameof(AccountController.ChangeDisplayName)}", HttpMethod.Post));

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangeDisplayName_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            HttpRequestMessage httpRequestMessage = RequestHelper.
                CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}",
                HttpMethod.Post,
                null,
                httpResponseMessage);

            // Act
            httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ChangeDisplayName_Returns200OKIfDisplayNameChangeSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeDisplayName(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testPassword,
                _testNewDisplayName);

            // Assert
            ChangeDisplayNameResponseModel body = JsonConvert.DeserializeObject<ChangeDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task ChangeDisplayName_Returns400BadRequestAndChangeDisplayNameResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeDisplayName(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeDisplayNameResponseModel body = JsonConvert.DeserializeObject<ChangeDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required,
                (body.ModelState[nameof(ChangeDisplayNameFormModel.Password)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewDisplayName_Required,
                (body.ModelState[nameof(ChangeDisplayNameFormModel.NewDisplayName)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeDisplayName_Returns400BadRequestAndChangeDisplayNameResponseModelIfPasswordIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);

            // Act
            httpResponseMessage = await ChangeDisplayName(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testInvalidPassword,
                _testNewDisplayName);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeDisplayNameResponseModel body = JsonConvert.DeserializeObject<ChangeDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid,
                (body.ModelState[nameof(ChangeDisplayNameFormModel.Password)] as JArray)[0]);
        }

        [Fact]
        public async Task ChangeDisplayName_Returns400BadRequestAndChangeDisplayNameResponseModelIfNewDisplayNameIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await LogIn(_testEmail1, _testPassword);
            VakAccount account = await CreateAccount(_testEmail2, _testPassword);
            await _vakAccountRepository.UpdateAccountDisplayNameAsync(account.AccountId, _testNewDisplayName);

            // Act
            httpResponseMessage = await ChangeDisplayName(CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage),
                _testPassword,
                _testNewDisplayName);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ChangeDisplayNameResponseModel body = JsonConvert.DeserializeObject<ChangeDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_DisplayName_InUse,
                (body.ModelState[nameof(ChangeDisplayNameFormModel.NewDisplayName)] as JArray)[0]);
        }

        //[Theory]
        //[MemberData(nameof(EnableTwoFactorPostData))]
        //public async Task EnableTwoFactor_RedirectsToManageAccountViewIfTwoFactorAlreadyEnabledOrIsSuccessfullyEnabled(bool alreadyEnabled)
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);
        //    await _vakAccountRepository.UpdateAccountEmailVerifiedAsync(1, true);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);
        //    if (alreadyEnabled)
        //    {
        //        // Only do this after logging into avoid a redirect status code for logInPostResponse
        //        await _vakAccountRepository.UpdateAccountTwoFactorEnabledAsync(1, true);
        //    }

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage enableTwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    // Assert
        //    Assert.Equal("Redirect", enableTwoFactorPostResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", enableTwoFactorPostResponse.Headers.Location.ToString());
        //    VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
        //    Assert.True(account.TwoFactorEnabled);
        //}

        //public static IEnumerable<object[]> EnableTwoFactorPostData()
        //{
        //    yield return new object[] { true };
        //    yield return new object[] { false };
        //}

        //[Fact]
        //public async Task EnableTwoFactor_RedirectsToTestTwoFactorCodeViewAndSendsTwoFactorEmailIfEmailIsNotVerified()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    File.WriteAllText(@"Temp\SmtpTest.txt", "");
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(CookiesHelper.ExtractCookiesFromResponse(logInPostResponse));
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage enableTwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    // Assert
        //    Assert.Equal("Redirect", enableTwoFactorPostResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}", enableTwoFactorPostResponse.Headers.Location.ToString());
        //    string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
        //    Assert.Contains(Strings.Email_Subject_TwoFactorEmail, twoFactorEmail);
        //    Assert.Contains(email, twoFactorEmail);
        //    Assert.Matches(Strings.TwoFactorEmail_Message.Replace("{0}", @"\d{6,6}"), twoFactorEmail);
        //}

        //[Fact]
        //public async Task EnableTwoFactor_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage enableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.EnableTwoFactor)}",
        //        HttpMethod.Post,
        //        null,
        //        logInPostResponse);

        //    // Act
        //    HttpResponseMessage enableTwoFactorPostResponse = await _httpClient.SendAsync(enableTwoFactorPostRequest);

        //    // Assert
        //    Assert.Equal(_badRequest, enableTwoFactorPostResponse.StatusCode.ToString());
        //}

        //[Fact]
        //public async Task EnableTwoFactor_RedirectsToLogInViewIfAuthenticationFails()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
        //        $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
        //        HttpMethod.Get,
        //        null,
        //        logInPostResponse);
        //    HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
        //    IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
        //    {
        //        { "__RequestVerificationToken", antiForgeryToken }
        //    };
        //    HttpRequestMessage enableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.EnableTwoFactor)}",
        //        HttpMethod.Post,
        //        formPostBodyData,
        //        manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage enableTwoFactorPostReponse = await _httpClient.SendAsync(enableTwoFactorPostRequest);

        //    // Assert
        //    Assert.Equal("Redirect", enableTwoFactorPostReponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", enableTwoFactorPostReponse.Headers.Location.AbsolutePath);
        //}

        //[Theory]
        //[MemberData(nameof(DisableTwoFactorPostData))]
        //public async Task DisableTwoFactor_RedirectsToManageAccountViewIfTwoFactorAlreadyDisabledOrIsSuccessfullyDisabled(bool alreadyDisabled)
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);
        //    if (!alreadyDisabled)
        //    {
        //        // Only do this after logging into avoid a redirect status code for logInPostResponse
        //        await _vakAccountRepository.UpdateAccountTwoFactorEnabledAsync(1, true);
        //    }

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage disableTwoFactorPostResponse = await DisableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    // Assert
        //    Assert.Equal("Redirect", disableTwoFactorPostResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", disableTwoFactorPostResponse.Headers.Location.ToString());
        //    VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
        //    Assert.False(account.TwoFactorEnabled);
        //}

        //public static IEnumerable<object[]> DisableTwoFactorPostData()
        //{
        //    yield return new object[] { true };
        //    yield return new object[] { false };
        //}

        //[Fact]
        //public async Task DisableTwoFactor_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage disableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.DisableTwoFactor)}",
        //        HttpMethod.Post,
        //        null,
        //        logInPostResponse);

        //    // Act
        //    HttpResponseMessage disableTwoFactorPostResponse = await _httpClient.SendAsync(disableTwoFactorPostRequest);

        //    // Assert
        //    Assert.Equal(_badRequest, disableTwoFactorPostResponse.StatusCode.ToString());
        //}

        //[Fact]
        //public async Task DisableTwoFactor_RedirectsToLogInViewIfAuthenticationFails()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
        //        $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
        //        HttpMethod.Get,
        //        null,
        //        logInPostResponse);
        //    HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
        //    IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
        //    {
        //        { "__RequestVerificationToken", antiForgeryToken }
        //    };
        //    HttpRequestMessage disableTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.DisableTwoFactor)}",
        //        HttpMethod.Post,
        //        formPostBodyData,
        //        manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage disableTwoFactorPostReponse = await _httpClient.SendAsync(disableTwoFactorPostRequest);

        //    // Assert
        //    Assert.Equal("Redirect", disableTwoFactorPostReponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", disableTwoFactorPostReponse.Headers.Location.AbsolutePath);
        //}

        //[Fact]
        //public async Task TestTwoFactor_RedirectsToLogInViewIfAuthenticationFails()
        //{
        //    // Act 
        //    HttpResponseMessage testTwoFactorGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}");

        //    // Assert
        //    Assert.Equal("Redirect", testTwoFactorGetResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", testTwoFactorGetResponse.Headers.Location.AbsolutePath);
        //}

        //[Fact]
        //public async Task TestTwoFactor_ReturnsTestTwoFactorCodeViewWithAntiForgeryCredentialsIfAuthenticationSucceeds()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage TestTwoFactorGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
        //        HttpMethod.Get,
        //        null,
        //        logInPostResponse);

        //    // Act
        //    HttpResponseMessage testTwoFactorGetResponse = await _httpClient.SendAsync(TestTwoFactorGetRequest);

        //    // Assert
        //    Assert.Equal("OK", testTwoFactorGetResponse.StatusCode.ToString());
        //    string html = await testTwoFactorGetResponse.Content.ReadAsStringAsync();
        //    Assert.Contains(Strings.ViewTitle_TestTwoFactor, html);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(testTwoFactorGetResponse);
        //    IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(testTwoFactorGetResponse);
        //    Assert.NotNull(antiForgeryToken);
        //    Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        //}

        //[Fact]
        //public async Task TestTwoFactor_RedirectsToManageAccountAndUpdatesEmailVerifiedAndTwoFactorEnabledIfSuccessful()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    File.WriteAllText(@"Temp\SmtpTest.txt", "");
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(CookiesHelper.ExtractCookiesFromResponse(logInPostResponse));
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    HttpResponseMessage enabletwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    string twoFactorEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
        //    Regex codeMatch = new Regex(@" (\d{6,6})");
        //    string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

        //    // Act
        //    HttpResponseMessage testTwoFactorPostResponse = await TestTwoFactor(code, applicationCookie);

        //    // Assert
        //    Assert.Equal("Redirect", testTwoFactorPostResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.ManageAccount)}", testTwoFactorPostResponse.Headers.Location.ToString());
        //    VakAccount account = await _vakAccountRepository.GetAccountAsync(1);
        //    Assert.True(account.EmailVerified);
        //    Assert.True(account.TwoFactorEnabled);
        //}

        //[Theory]
        //[MemberData(nameof(TestTwoFactorPostData))]
        //public async Task TestTwoFactor_TestTwoFactorViewWithErrorMessageIfModelStateIsInvalidOrCodeIsInvalid(string code)
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    File.WriteAllText(@"Temp\SmtpTest.txt", "");
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(CookiesHelper.ExtractCookiesFromResponse(logInPostResponse));
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    HttpResponseMessage enabletwoFactorPostResponse = await EnableTwoFactor(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    // Act
        //    HttpResponseMessage testTwoFactorPostResponse = await TestTwoFactor(code, applicationCookie);

        //    // Assert
        //    Assert.Equal("OK", testTwoFactorPostResponse.StatusCode.ToString());
        //    string html = await testTwoFactorPostResponse.Content.ReadAsStringAsync();
        //    Assert.Contains(Strings.ViewTitle_TestTwoFactor, html);
        //    Assert.Contains(Strings.ErrorMessage_TwoFactorCode_Invalid, html);
        //}

        //public static IEnumerable<object[]> TestTwoFactorPostData()
        //{
        //    // Invalid format
        //    yield return new object[] { "12345" };
        //    // Invalid code
        //    yield return new object[] { "123456" };
        //}

        //[Fact]
        //public async Task TestTwoFactor_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
        //    {
        //        {"Code", "123456" }
        //    };
        //    HttpRequestMessage testTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
        //        HttpMethod.Post,
        //        formPostBodyData,
        //        logInPostResponse);

        //    // Act
        //    HttpResponseMessage testTwoFactorPostResponse = await _httpClient.SendAsync(testTwoFactorPostRequest);

        //    // Assert
        //    Assert.Equal(_badRequest, testTwoFactorPostResponse.StatusCode.ToString());
        //}

        //[Fact]
        //public async Task TestTwoFactor_RedirectsToLogInViewIfAuthenticationFails()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage testTwoFactorGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
        //        HttpMethod.Get,
        //        null,
        //        logInPostResponse);
        //    HttpResponseMessage testTwoFactorGetResponse = await _httpClient.SendAsync(testTwoFactorGetRequest);

        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(testTwoFactorGetResponse);
        //    IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
        //    {
        //        {"Code", "123456" },
        //        { "__RequestVerificationToken", antiForgeryToken }
        //    };
        //    HttpRequestMessage testTwoFactorPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.TestTwoFactor)}",
        //        HttpMethod.Post,
        //        formPostBodyData,
        //        testTwoFactorGetResponse);

        //    // Act
        //    HttpResponseMessage testTwoFactorPostResponse = await _httpClient.SendAsync(testTwoFactorPostRequest);

        //    // Assert
        //    Assert.Equal("Redirect", testTwoFactorPostResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", testTwoFactorPostResponse.Headers.Location.AbsolutePath);
        //}

        //[Fact]
        //public async Task SendEmailVerificationEmail_RedirectsToSendEmailVerificationEmailConfirmationViewAndSendsEmailVerificationEmailIfSuccessful()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    File.WriteAllText(@"Temp\SmtpTest.txt", "");
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage sendEmailVerificationEmailPostResponse = await SendEmailVerificationEmail(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    // Assert
        //    Assert.Equal("Redirect", sendEmailVerificationEmailPostResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}?Email={email}", sendEmailVerificationEmailPostResponse.Headers.Location.ToString());
        //    string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
        //    Assert.Contains(Strings.Email_Subject_EmailVerification, emailVerificationEmail);
        //    Assert.Contains(email, emailVerificationEmail);
        //    Assert.Matches(Strings.Email_EmailVerification_Message.Replace("{0}", $"http://localhost/{_accountControllerName}/{nameof(AccountController.EmailVerificationConfirmation)}.*?"), emailVerificationEmail);
        //}

        //[Fact]
        //public async Task SendEmailVerificationEmail_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage sendEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmail)}",
        //        HttpMethod.Post,
        //        null,
        //        logInPostResponse);

        //    // Act
        //    HttpResponseMessage sendEmailVerificationEmailPostResponse = await _httpClient.SendAsync(sendEmailVerificationEmailPostRequest);

        //    // Assert
        //    Assert.Equal(_badRequest, sendEmailVerificationEmailPostResponse.StatusCode.ToString());
        //}

        //[Fact]
        //public async Task SendEmailVerificationEmail_RedirectsToLogInViewIfAuthenticationFails()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
        //        $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
        //        HttpMethod.Get,
        //        null,
        //        logInPostResponse);
        //    HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
        //    IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
        //    {
        //        { "__RequestVerificationToken", antiForgeryToken }
        //    };
        //    HttpRequestMessage sendEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmail)}",
        //        HttpMethod.Post,
        //        formPostBodyData,
        //        manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage sendEmailVerificationEmailPostReponse = await _httpClient.SendAsync(sendEmailVerificationEmailPostRequest);

        //    // Assert
        //    Assert.Equal("Redirect", sendEmailVerificationEmailPostReponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", sendEmailVerificationEmailPostReponse.Headers.Location.AbsolutePath);
        //}

        //[Theory]
        //[MemberData(nameof(EmailVerificationConfirmationGetData))]
        //public async Task EmailVerificationConfirmation_ReturnsErrorViewIfAccountIdTokenOrModelStateIsInvalid(string accountId, string token)
        //{
        //    // Arrange
        //    if (accountId == "1")
        //    {
        //        await _resetAccountsTable();
        //        await CreateAccount("test@test.com", "Password");
        //    }

        //    // Act
        //    HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"Account/EmailVerificationConfirmation?Email={accountId}&Token={token}");

        //    // Assert
        //    string html = await httpResponseMessage.Content.ReadAsStringAsync();
        //    Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());

        //    Assert.True(html.Contains(Strings.ViewTitle_Error));
        //}

        //public static IEnumerable<object[]> EmailVerificationConfirmationGetData()
        //{
        //    yield return new object[] { "", "" };
        //    yield return new object[] { "1", "invalidtoken" };
        //}

        //[Fact]
        //public async Task EmailVerificationConfirmation_ReturnsEmailVerificationConfirmationViewIfEmailTokenAndModelStateAreValid()
        //{
        //    // Arrange
        //    string email = "Email1@test.com", password = "Password";
        //    int accountId = 1;

        //    File.WriteAllText(@"Temp\SmtpTest.txt", "");
        //    await _resetAccountsTable();
        //    await SignUp(email, password, password);

        //    string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
        //    Regex tokenRegex = new Regex(@"Token=(.*?)&");
        //    string token = tokenRegex.Match(emailVerificationEmail).Groups[1].Value;

        //    // Act
        //    // TODO why not just grab email verification link?
        //    HttpResponseMessage emailVerificationConfirmationGetResponse = await _httpClient.GetAsync($"Account/EmailVerificationConfirmation?AccountId={accountId}&Token={token}");

        //    // Assert
        //    Assert.True((await _vakAccountRepository.GetAccountAsync(accountId)).EmailVerified);
        //    Assert.Equal("OK", emailVerificationConfirmationGetResponse.StatusCode.ToString());
        //    string html = await emailVerificationConfirmationGetResponse.Content.ReadAsStringAsync();
        //    Assert.True(html.Contains(Strings.ViewTitle_EmailVerificationConfirmation));
        //}

        //[Fact]
        //public async Task SendAlternativeEmailVerificationEmail_RedirectsToSendEmailVerificationEmailConfirmationViewAndSendsAlternativeEmailVerificationEmailIfSuccessful()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password", alternativeEmail = "alt@email.com";

        //    await _resetAccountsTable();
        //    File.WriteAllText(@"Temp\SmtpTest.txt", "");
        //    await CreateAccount(email, password);
        //    await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(1, alternativeEmail);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage sendAlternativeEmailVerificationEmailPostResponse = await SendAlternativeEmailVerificationEmail(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    // Assert
        //    Assert.Equal("Redirect", sendAlternativeEmailVerificationEmailPostResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}", sendAlternativeEmailVerificationEmailPostResponse.Headers.Location.ToString());
        //    string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
        //    Assert.Contains(Strings.Email_Subject_EmailVerification, emailVerificationEmail);
        //    Assert.Contains(alternativeEmail, emailVerificationEmail);
        //    Assert.Matches(Strings.Email_EmailVerification_Message.Replace("{0}", $"http://localhost/{_accountControllerName}/{nameof(AccountController.AlternativeEmailVerificationConfirmation)}.*?"), emailVerificationEmail);
        //}

        //[Fact]
        //public async Task SendAlternativeEmailVerificationEmail_ReturnsBadRequestIfAntiForgeryCredentialsAreInvalid()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage sendAlternativeEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendAlternativeEmailVerificationEmail)}",
        //        HttpMethod.Post,
        //        null,
        //        logInPostResponse);

        //    // Act
        //    HttpResponseMessage sendAlternativeEmailVerificationEmailPostResponse = await _httpClient.SendAsync(sendAlternativeEmailVerificationEmailPostRequest);

        //    // Assert
        //    Assert.Equal(_badRequest, sendAlternativeEmailVerificationEmailPostResponse.StatusCode.ToString());
        //}

        //[Fact]
        //public async Task SendAlternativeEmailVerificationEmail_RedirectsToLogInViewIfAuthenticationFails()
        //{
        //    // Arrange
        //    string email = "email@email.com", password = "Password";

        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage ManageAccountGetRequest = RequestHelper.CreateWithCookiesFromResponse(
        //        $"{_accountControllerName}/{nameof(AccountController.ManageAccount)}",
        //        HttpMethod.Get,
        //        null,
        //        logInPostResponse);
        //    HttpResponseMessage manageAccountGetResponse = await _httpClient.SendAsync(ManageAccountGetRequest);

        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);
        //    IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
        //    {
        //        { "__RequestVerificationToken", antiForgeryToken }
        //    };
        //    HttpRequestMessage sendAlternativeEmailVerificationEmailPostRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendAlternativeEmailVerificationEmail)}",
        //        HttpMethod.Post,
        //        formPostBodyData,
        //        manageAccountGetResponse);

        //    // Act
        //    HttpResponseMessage sendAlternativeEmailVerificationEmailPostReponse = await _httpClient.SendAsync(sendAlternativeEmailVerificationEmailPostRequest);

        //    // Assert
        //    Assert.Equal("Redirect", sendAlternativeEmailVerificationEmailPostReponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", sendAlternativeEmailVerificationEmailPostReponse.Headers.Location.AbsolutePath);
        //}

        //[Theory]
        //[MemberData(nameof(AlternativeEmailVerificationConfirmationGetData))]
        //public async Task AlternativeEmailVerificationConfirmation_ReturnsErrorViewIfAccountIdTokenOrModelStateIsInvalid(string accountId, string token)
        //{
        //    // Arrange
        //    if (accountId == "1")
        //    {
        //        await _resetAccountsTable();
        //        await CreateAccount("test@test.com", "Password");
        //    }

        //    // Act
        //    HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.AlternativeEmailVerificationConfirmation)}?Email={accountId}&Token={token}");

        //    // Assert
        //    string html = await httpResponseMessage.Content.ReadAsStringAsync();
        //    Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());

        //    Assert.True(html.Contains(Strings.ViewTitle_Error));
        //}

        //public static IEnumerable<object[]> AlternativeEmailVerificationConfirmationGetData()
        //{
        //    yield return new object[] { "", "" };
        //    yield return new object[] { "1", "invalidtoken" };
        //}

        //[Fact]
        //public async Task AlternativeEmailVerificationConfirmation_ReturnsAlternativeEmailVerificationConfirmationViewIfEmailTokenAndModelStateAreValid()
        //{
        //    // Arrange
        //    string email = "Email1@test.com", password = "Password", alternativeEmail = "alt@email.com";
        //    await _resetAccountsTable();
        //    File.WriteAllText(@"Temp\SmtpTest.txt", "");
        //    await CreateAccount(email, password);
        //    await _vakAccountRepository.UpdateAccountAlternativeEmailAsync(1, alternativeEmail);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);
        //    IDictionary<string, string> applicationCookie = CookiesHelper.ExtractCookiesFromResponse(logInPostResponse);

        //    HttpResponseMessage manageAccountGetResponse = await ManageAccount(applicationCookie);
        //    IDictionary<string, string> antiForgeryCookie = CookiesHelper.ExtractCookiesFromResponse(manageAccountGetResponse);
        //    string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(manageAccountGetResponse);

        //    HttpResponseMessage sendAlternativeEmailVerificationEmailPostResponse = await SendAlternativeEmailVerificationEmail(applicationCookie, antiForgeryCookie, antiForgeryToken);

        //    string emailVerificationEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
        //    Regex tokenRegex = new Regex(@"Token=(.*?)&");
        //    string token = tokenRegex.Match(emailVerificationEmail).Groups[1].Value;

        //    // Act
        //    // TODO why not just grab email verification link?
        //    HttpResponseMessage emailVerificationConfirmationGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.AlternativeEmailVerificationConfirmation)}?AccountId={1}&Token={token}");

        //    // Assert
        //    Assert.True((await _vakAccountRepository.GetAccountAsync(1)).AlternativeEmailVerified);
        //    Assert.Equal("OK", emailVerificationConfirmationGetResponse.StatusCode.ToString());
        //    string html = await emailVerificationConfirmationGetResponse.Content.ReadAsStringAsync();
        //    Assert.True(html.Contains(Strings.ViewTitle_AlternativeEmailVerificationConfirmation));
        //}

        //[Fact]
        //public async Task SendEmailVerificationEmailConfirmation_ReturnsSendEmailVerificationEmailViewIfAuthenticationSucceeds()
        //{
        //    // Arrange
        //    string email = "Email1@test.com", password = "Password";
        //    await _resetAccountsTable();
        //    await CreateAccount(email, password);

        //    HttpResponseMessage logInPostResponse = await LogIn(email, password);

        //    HttpRequestMessage sendEmailVerificationEmailConfirmationGetRequest = RequestHelper.CreateWithCookiesFromResponse($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}?Email={email}",
        //        HttpMethod.Get, 
        //        null,
        //        logInPostResponse);

        //    // Act
        //    HttpResponseMessage sendEmailVerificationEmailConfirmationGetResponse = await _httpClient.SendAsync(sendEmailVerificationEmailConfirmationGetRequest);

        //    // Assert
        //    string html = await sendEmailVerificationEmailConfirmationGetResponse.Content.ReadAsStringAsync();
        //    Assert.Equal("OK", sendEmailVerificationEmailConfirmationGetResponse.StatusCode.ToString());
        //    Assert.True(html.Contains(Strings.ViewTitle_SendEmailVerificationEmailConfirmation));
        //}

        //[Fact]
        //public async Task SendAlternativeEmailVerificationEmailConfirmation_RedirectsToLogInViewIfAuthenticationFails()
        //{
        //    // Act
        //    HttpResponseMessage sendEmailVerificationEmailConfirmationGetResponse = await _httpClient.GetAsync($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmailConfirmation)}?Email=email@email.com");

        //    // Assert
        //    Assert.Equal("Redirect", sendEmailVerificationEmailConfirmationGetResponse.StatusCode.ToString());
        //    Assert.Equal($"/{_accountControllerName}/{nameof(AccountController.LogIn)}", sendEmailVerificationEmailConfirmationGetResponse.Headers.Location.AbsolutePath);
        //}

        #region Helpers
        public async Task<HttpResponseMessage> ChangeDisplayName(IDictionary<string, string> applicationCookie, string password,
        string newDisplayName)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(ChangeDisplayNameFormModel).Replace("FormModel", "")}",
                HttpMethod.Get);
            // This step is required to generate antiforgery token with serialized user info
            CookiesHelper.PutCookiesOnRequest(getDynamicFormGetRequest, applicationCookie);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(ChangeDisplayNameFormModel.Password), password },
                { nameof(ChangeDisplayNameFormModel.NewDisplayName), newDisplayName }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.ChangeDisplayName)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> ChangeAlternativeEmail(IDictionary<string, string> applicationCookie, string password, 
            string newAlternativeEmail)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(ChangeAlternativeEmailFormModel).Replace("FormModel", "")}",
                HttpMethod.Get);
            // This step is required to generate antiforgery token with serialized user info
            CookiesHelper.PutCookiesOnRequest(getDynamicFormGetRequest, applicationCookie);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(ChangeAlternativeEmailFormModel.Password), password },
                { nameof(ChangeAlternativeEmailFormModel.NewAlternativeEmail), newAlternativeEmail }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.ChangeAlternativeEmail)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> ChangeEmail(IDictionary<string, string> applicationCookie, string password, string newEmail)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(ChangeEmailFormModel).Replace("FormModel", "")}",
                HttpMethod.Get);
            // This step is required to generate antiforgery token with serialized user info
            CookiesHelper.PutCookiesOnRequest(getDynamicFormGetRequest, applicationCookie);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(ChangeEmailFormModel.Password), password },
                { nameof(ChangeEmailFormModel.NewEmail), newEmail }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.ChangeEmail)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> ChangePassword(IDictionary<string, string> applicationCookie, string currentPassword, string newPassword, string confirmNewPassword)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(ChangePasswordFormModel).Replace("FormModel", "")}",
                HttpMethod.Get);
            // Necessary to generate antiforgery token with serialized user info
            CookiesHelper.PutCookiesOnRequest(getDynamicFormGetRequest, applicationCookie);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(ChangePasswordFormModel.CurrentPassword), currentPassword },
                { nameof(ChangePasswordFormModel.NewPassword), newPassword },
                { nameof(ChangePasswordFormModel.ConfirmNewPassword), confirmNewPassword }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.ChangePassword)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, applicationCookie);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> ResetPassword(string email, string token, string newPassword, string confirmNewPassword)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(ResetPasswordFormModel).Replace("FormModel", "")}",
                HttpMethod.Get);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(ResetPasswordFormModel.Email), email},
                { nameof(ResetPasswordFormModel.Token), token },
                { nameof(ResetPasswordFormModel.NewPassword), newPassword },
                { nameof(ResetPasswordFormModel.ConfirmPassword), confirmNewPassword }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.ResetPassword)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SendResetPasswordEmail(string email)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(SendResetPasswordEmailFormModel).Replace("FormModel", "")}", HttpMethod.Get);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SendResetPasswordEmailFormModel.Email), email}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SendResetPasswordEmail)}",
                HttpMethod.Post,
                formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> TwoFactorLogIn(IDictionary<string, string> twoFactorCookie, string code)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(TwoFactorLogInFormModel).Replace("FormModel", "")}", HttpMethod.Get);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(TwoFactorLogInFormModel.Code), code},
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.TwoFactorLogIn)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, twoFactorCookie);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SignUp(string email, string password, string confirmPassword)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(SignUpFormModel).Replace("FormModel", "")}", HttpMethod.Get);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SignUpFormModel.Email), email},
                { nameof(SignUpFormModel.Password), password},
                { nameof(SignUpFormModel.ConfirmPassword),  confirmPassword}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SignUp)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> LogIn(string email, string password)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={nameof(LogInFormModel).Replace("FormModel", "")}", HttpMethod.Get);
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);
            IDictionary<string, string> antiForgeryCookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(LogInFormModel.Email), email},
                { nameof(LogInFormModel.Password), password}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.LogIn)}", HttpMethod.Post, formPostBodyData);
            httpRequestMessage.Headers.Add("X-XSRF-TOKEN", antiForgeryCookies["XSRF-TOKEN"]);
            CookiesHelper.PutCookiesOnRequest(httpRequestMessage, antiForgeryCookies);

            return await _httpClient.SendAsync(httpRequestMessage);
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



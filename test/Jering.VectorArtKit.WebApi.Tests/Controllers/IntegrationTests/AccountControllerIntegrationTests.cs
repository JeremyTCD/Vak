using Jering.Utilities;
using Jering.VectorArtKit.DatabaseInterface;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.FormModels;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.Account;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [Collection("ControllersCollection")]
    public class AccountControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        private VakAccountRepository _vakAccountRepository { get; }
        private VakDbContext _dbContext { get; }

        private string _accountControllerName { get; } = nameof(AccountController).Replace("Controller", "");
        private string _dynamicFormsControllerName { get; } = nameof(DynamicFormController).Replace("Controller", "");
        private const string _badRequest = "BadRequest";
        private const string _unauthorized = "Unauthorized";
        private const string _ok = "OK";
        private string _tempEmailFile { get; } = $"{Environment.GetEnvironmentVariable("TMP")}\\SmtpTest.txt";
        private const string _applicationCookieName = "Jering.Application";
        private const string _twoFactorCookieName = "Jering.TwoFactor";
        private const string _testEmail1 = "test@email1.com";
        private const string _testEmail2 = "test@email2.com";
        private const string _testNewEmail = "testNew@email.com";
        private const string _testAltEmail = "testAlternative@email.com";
        private const string _testNewAltEmail = "testNewAlternative@email.com";
        private const string _testNewPassword = "testNewPassword";
        private const string _testInvalidPassword = "testInvalidPassword";
        private const string _testPassword = "testPassword";
        private const string _testToken = "testToken";
        private const string _testNewDisplayName = "testNewDisplayName";
        private const string _cookieTokenName = "AF-TOKEN";
        private const string _requestTokenName = "XSRF-TOKEN";
        private const string _headerTokenName = "X-XSRF-TOKEN";
        private const bool _testNewTwoFactorEnabled = true;
        private const int _testAccountId = 1;
        private const int _testInvalidAccountId = 2;
        private VakAccount _testAccount;

        public AccountControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
            _dbContext = new VakDbContext(controllersFixture.DbContextOptions);
            _vakAccountRepository = new VakAccountRepository(_dbContext, new TimeService());
            controllersFixture.ResetAccountsTable(_dbContext);
            _testAccount = new VakAccount();
        }

        #region Session
        [Fact]
        public async Task Login_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(null, _testEmail1, _testPassword);

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
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(antiforgeryCookies, "", "");

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
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(antiforgeryCookies, _testEmail1, _testPassword);

            // Assert
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_LogIn_Failed, body.ErrorMessage);
        }

        [Fact]
        public async Task Login_Returns200OkLoginResponseModelApplicationCookieAndAntiForgeryCookiesIfLoginSucceeds()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(antiforgeryCookies, _testEmail1, _testPassword);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(2, cookies.Count());
            Assert.True(cookies.Keys.Contains(_applicationCookieName));
            Assert.True(cookies.Keys.Contains(_requestTokenName));
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_testEmail1, body.Username);
            Assert.False(body.TwoFactorRequired);
            Assert.False(body.IsPersistent);
        }

        [Fact]
        public async Task Login_Returns400BadRequestLoginResponseModelTwoFactorCookieAndSendsTwoFactorEmailIfTwoFactorAuthIsRequired()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            IDictionary<string, string> applicationAndAntiForgeryCookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1,
                _testPassword);
            _testAccount = await _vakAccountRepository.GetAsync(_testAccountId, CancellationToken.None);
            await _vakAccountRepository.UpdateEmailVerifiedAsync(_testAccount, true, CancellationToken.None);
            await SetTwoFactorEnabled(applicationAndAntiForgeryCookies, true);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await LogIn(antiforgeryCookies, _testEmail1, _testPassword);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(1, cookies.Count());
            Assert.True(cookies.Keys.Contains(_twoFactorCookieName));
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_TwoFactorCode, twoFactorEmail);
            Assert.Contains(_testEmail1, twoFactorEmail);
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.True(body.TwoFactorRequired);
            Assert.False(body.IsPersistent);
        }

        [Fact]
        public async Task LogOff_Returns200OkAndTwoFactorCookieAndApplicationCookieIfAuthIsSuccessful()
        {
            //Arrange
            IDictionary<string, string> applicationAndAntiForgeryCookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1,
                _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await LogOff(applicationAndAntiForgeryCookies);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(1, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
            Assert.Equal("", cookies[_applicationCookieName]);
        }

        [Fact]
        public async Task LogOff_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            //Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();

            // Act
            HttpResponseMessage httpResponseMessage = await LogOff(antiforgeryCookies);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task LogOff_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            //Arrange
            IDictionary<string, string> applicationCookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await LogOff(applicationCookie);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            LogInResponseModel body = JsonConvert.DeserializeObject<LogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns200OkAndTwoFactorLogInResponseModelTwoFactorCookieAndApplicationCookieIfLoginSucceeds()
        {
            // Arrange
            IDictionary<string, string> twoFactorAndAntiForgeryCookies = await GetTwoFactorAndAnonymousAntiforgeryCookies(_testEmail1, _testPassword);

            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorLogIn(twoFactorAndAntiForgeryCookies, code);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(3, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
            Assert.Contains(_requestTokenName, cookies.Keys);
            Assert.Contains(_twoFactorCookieName, cookies.Keys);
            Assert.Equal("", cookies[_twoFactorCookieName]);
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns400BadRequestAndTwoFactorResponseModelIfTwoFactorCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetAnonymousAntiforgeryCookies();

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorLogIn(cookies, _testToken);

            // Assert
            TwoFactorLogInResponseModel body = JsonConvert.DeserializeObject<TwoFactorLogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.True(body.ExpiredCredentials);
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns400BadRequestAndTwoFactorResponseModelIfCodeIsInvalidOrExpired()
        {
            // Arrange
            IDictionary<string, string> twoFactorAndAntiForgeryCookies = await GetTwoFactorAndAnonymousAntiforgeryCookies(_testEmail1, _testPassword);
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;
            string testCode = "000000" == code ? "111111" : "000000";

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorLogIn(twoFactorAndAntiForgeryCookies, testCode);

            // Assert
            TwoFactorLogInResponseModel body = JsonConvert.DeserializeObject<TwoFactorLogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_TwoFactorCode_Invalid, (body.ModelState[nameof(TwoFactorLogInFormModel.Code)] as JArray)[0]);
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns400BadRequestAndTwoFactorLogInResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> twoFactorAndAntiForgeryCookies = await GetTwoFactorAndAnonymousAntiforgeryCookies(_testEmail1, _testPassword);
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorLogIn(twoFactorAndAntiForgeryCookies, "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            TwoFactorLogInResponseModel body = JsonConvert.DeserializeObject<TwoFactorLogInResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_TwoFactorCode_Required, (body.ModelState[nameof(TwoFactorLogInFormModel.Code)] as JArray)[0]);
        }

        [Fact]
        public async Task TwoFactorLogIn_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorLogIn(null, "000000");

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }
        #endregion

        #region Account Management
        [Fact]
        public async Task SignUp_Returns400BadRequestAndSignUpResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(antiforgeryCookies, "test", "test1", "test2");

            // Assert
            SetPasswordResponseModel body = JsonConvert.DeserializeObject<SetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
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
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);

            // Assert
            SetPasswordResponseModel body = JsonConvert.DeserializeObject<SetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_InUse, (body.ModelState[nameof(SignUpFormModel.Email)] as JArray)[0]);
        }

        [Fact]
        public async Task SignUp_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(null, _testEmail1, _testPassword, _testPassword);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SignUp_Returns200OkSignUpResponseModelApplicationCookieAntiForgeryCookiesAndSendsEmailVerificationEmailIfRegistrationSucceeds()
        {
            // Arrange
            File.WriteAllText(_tempEmailFile, "");
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();

            // Act
            HttpResponseMessage httpResponseMessage = await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(2, cookies.Count());
            Assert.True(cookies.Keys.Contains(_applicationCookieName));
            Assert.True(cookies.Keys.Contains(_requestTokenName));
            string emailVerificationEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_EmailVerification, emailVerificationEmail);
            Assert.Contains(_testEmail1, emailVerificationEmail);
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns200OkAndSendsResetPasswordEmailIfEmailIsValid()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(antiforgeryCookies, _testEmail1);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_ResetPassword, resetPasswordEmail);
            Assert.Contains(_testEmail1, resetPasswordEmail);
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns400BadRequestAndSendResetPasswordEmailResponseModelIfEmailIsInvalid()
        {
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();

            // Act
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(antiforgeryCookies, _testEmail1);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SendResetPasswordEmailResponseModel body = JsonConvert.DeserializeObject<SendResetPasswordEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_Invalid, (body.ModelState[nameof(SendResetPasswordEmailFormModel.Email)] as JArray)[0]);
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns400BadRequestAndSendResetPasswordEmailResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(antiforgeryCookies, "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SendResetPasswordEmailResponseModel body = JsonConvert.DeserializeObject<SendResetPasswordEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_Required, (body.ModelState[nameof(SendResetPasswordEmailFormModel.Email)] as JArray)[0]);
        }

        [Fact]
        public async Task SendResetPasswordEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SendResetPasswordEmail(null, _testEmail1);

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
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);
            await SendResetPasswordEmail(antiforgeryCookies, _testEmail1);
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);

            // Act
            HttpResponseMessage httpResponseMessage = await ResetPassword(antiforgeryCookies, _testEmail1, token, "", "");

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
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await ResetPassword(antiforgeryCookies, _testEmail1,
                _testToken,
                _testNewPassword,
                _testNewPassword);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            ResetPasswordResponseModel body = JsonConvert.DeserializeObject<ResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.True(body.InvalidToken);
        }

        [Fact]
        public async Task ResetPassword_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await ResetPassword(null, "", "", "", "");

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
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);
            await SendResetPasswordEmail(antiforgeryCookies, _testEmail1);
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);

            // Act
            HttpResponseMessage httpResponseMessage = await ResetPassword(antiforgeryCookies,
                _testEmail1,
                token,
                _testNewPassword,
                _testNewPassword);

            // Assert
            ResetPasswordResponseModel body = JsonConvert.DeserializeObject<ResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            Assert.Equal(_testEmail1, body.Email);
        }

        [Fact]
        public async Task ResetPassword_Returns400BadRequestAndResetPasswordResponseModelIfEmailIsInvalid()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);
            await SendResetPasswordEmail(antiforgeryCookies, _testEmail1);
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);

            // Act
            HttpResponseMessage httpResponseMessage = await ResetPassword(antiforgeryCookies,
                _testEmail2,
                token,
                _testNewPassword,
                _testNewPassword);

            // Assert
            ResetPasswordResponseModel body = JsonConvert.DeserializeObject<ResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.True(body.InvalidEmail);
        }

        [Fact]
        public async Task ResetPassword_Returns400BadRequestAndResetPasswordResponseModelIfNewPasswordIsSameAsCurrentPassword()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail1, _testPassword, _testPassword);
            await SendResetPasswordEmail(antiforgeryCookies, _testEmail1);
            string resetPasswordEmail = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(resetPasswordEmail).Groups[1].Value);

            // Act
            HttpResponseMessage httpResponseMessage = await ResetPassword(antiforgeryCookies,
                _testEmail1,
                token,
                _testPassword,
                _testPassword);

            // Assert
            ResetPasswordResponseModel body = JsonConvert.DeserializeObject<ResetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_NewPassword_MustDiffer, (body.ModelState[nameof(ResetPasswordFormModel.NewPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task GetAccountDetails_Returns200OkAndGetAccountDetailsResponseModelIfAuthSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await GetAccountDetails(cookies);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            GetAccountDetailsResponseModel body = JsonConvert.DeserializeObject<GetAccountDetailsResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Null(body.AltEmail);
            Assert.False(body.AltEmailVerified);
            Assert.Null(body.DisplayName);
            Assert.NotNull(body.DurationSinceLastPasswordChange);
            Assert.Equal(_testEmail1, body.Email);
            Assert.False(body.EmailVerified);
            Assert.False(body.TwoFactorEnabled);
        }

        [Fact]
        public async Task GetAccountDetails_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await GetAccountDetails(null);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task GetAccountDetails_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange 
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await GetAccountDetails(cookie);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetPassword_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SetPassword(null, _testPassword, _testNewPassword, _testNewPassword);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetPassword_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetPassword(cookie, _testPassword, _testNewPassword, _testNewPassword);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetPassword_Returns200OKAndApplicationCookieIfPasswordChangeSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetPassword(cookies, _testPassword, _testNewPassword, _testNewPassword);

            // Assert
            SetPasswordResponseModel body = JsonConvert.DeserializeObject<SetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(1, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
        }

        [Fact]
        public async Task SetPassword_Returns400BadRequestAndSetPasswordResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetPassword(cookies,
                "",
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetPasswordResponseModel body = JsonConvert.DeserializeObject<SetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, (body.ModelState[nameof(SetPasswordFormModel.CurrentPassword)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewPassword_MustDiffer, (body.ModelState[nameof(SetPasswordFormModel.NewPassword)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewPassword_Required, (body.ModelState[nameof(SetPasswordFormModel.NewPassword)] as JArray)[1]);
            Assert.Equal(Strings.ErrorMessage_ConfirmPassword_Required, (body.ModelState[nameof(SetPasswordFormModel.ConfirmNewPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task SetPassword_Returns400BadRequestAndSetPasswordResponseModelIfCurrentPasswordIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetPassword(cookies, _testInvalidPassword, _testNewPassword, _testNewPassword);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetPasswordResponseModel body = JsonConvert.DeserializeObject<SetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid, (body.ModelState[nameof(SetPasswordFormModel.CurrentPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task SetPassword_Returns400BadRequestAndSetPasswordResponseModelIfNewPasswordIsSameAsCurrentPassword()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetPassword(cookies, _testPassword, _testPassword, _testPassword);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetPasswordResponseModel body = JsonConvert.DeserializeObject<SetPasswordResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_NewPassword_MustDiffer, (body.ModelState[nameof(SetPasswordFormModel.NewPassword)] as JArray)[0]);
        }

        [Fact]
        public async Task SetEmail_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SetEmail(null, _testPassword, _testNewEmail);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmail(cookie, _testPassword, _testNewEmail);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetEmail_Returns200OKAndApplicationCookieIfEmailChangeSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmail(cookies, _testPassword, _testNewEmail);

            // Assert
            SetEmailResponseModel body = JsonConvert.DeserializeObject<SetEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(1, cookies.Count);
            Assert.Contains(_applicationCookieName, cookies.Keys);
        }

        [Fact]
        public async Task SetEmail_Returns400BadRequestAndSetEmailResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmail(cookies,
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetEmailResponseModel body = JsonConvert.DeserializeObject<SetEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, (body.ModelState[nameof(SetEmailFormModel.Password)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewEmail_Required, (body.ModelState[nameof(SetEmailFormModel.NewEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task SetEmail_Returns400BadRequestAndSetEmailResponseModelIfPasswordIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmail(cookies, _testInvalidPassword, _testNewEmail);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetEmailResponseModel body = JsonConvert.DeserializeObject<SetEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid, (body.ModelState[nameof(SetEmailFormModel.Password)] as JArray)[0]);
        }

        [Fact]
        public async Task SetEmail_Returns400BadRequestAndSetEmailResponseModelIfNewEmailIsInUse()
        {
            // Arrange
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            await SignUp(antiforgeryCookies, _testEmail2, _testPassword, _testPassword);
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmail(cookies, _testPassword, _testEmail2);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetEmailResponseModel body = JsonConvert.DeserializeObject<SetEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Email_InUse, (body.ModelState[nameof(SetEmailFormModel.NewEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task SetEmail_Returns400BadRequestAndSetEmailResponseModelIfNewEmailIsSameAsCurrentEmail()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmail(cookies, _testPassword, _testEmail1);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetEmailResponseModel body = JsonConvert.DeserializeObject<SetEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_NewEmail_MustDiffer, (body.ModelState[nameof(SetEmailFormModel.NewEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task SetAltEmail_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmail(null, _testPassword, _testNewAltEmail);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetAltEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmail(cookie, _testPassword, _testNewAltEmail);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetAltEmail_Returns200OKIfAltEmailChangeSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmail(cookies, _testPassword, _testNewAltEmail);

            // Assert
            SetAltEmailResponseModel body = JsonConvert.DeserializeObject<SetAltEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task SetAltEmail_Returns400BadRequestAndSetAltEmailResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmail(cookies,
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetAltEmailResponseModel body = JsonConvert.DeserializeObject<SetAltEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, (body.ModelState[nameof(SetAltEmailFormModel.Password)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewAltEmail_Required, (body.ModelState[nameof(SetAltEmailFormModel.NewAltEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task SetAltEmail_Returns400BadRequestAndSetAltEmailResponseModelIfPasswordIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmail(cookies, _testInvalidPassword, _testNewAltEmail);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetAltEmailResponseModel body = JsonConvert.DeserializeObject<SetAltEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid, (body.ModelState[nameof(SetAltEmailFormModel.Password)] as JArray)[0]);
        }

        [Fact]
        public async Task SetAltEmail_Returns400BadRequestAndSetAltEmailResponseModelIfNewAltEmailIsSameAsCurrentAltEmail()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            await SetAltEmail(cookies, _testPassword, _testNewAltEmail);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmail(cookies, _testPassword, _testNewAltEmail);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetAltEmailResponseModel body = JsonConvert.DeserializeObject<SetAltEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_NewEmail_MustDiffer, (body.ModelState[nameof(SetAltEmailFormModel.NewAltEmail)] as JArray)[0]);
        }

        [Fact]
        public async Task SetDisplayName_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SetDisplayName(null, _testPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetDisplayName_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetDisplayName(cookie, _testPassword, _testNewDisplayName);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetDisplayName_Returns200OKIfDisplayNameChangeSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetDisplayName(cookies, _testPassword, _testNewDisplayName);

            // Assert
            SetDisplayNameResponseModel body = JsonConvert.DeserializeObject<SetDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task SetDisplayName_Returns400BadRequestAndSetDisplayNameResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetDisplayName(cookies,
                "",
                "");

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetDisplayNameResponseModel body = JsonConvert.DeserializeObject<SetDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Required, (body.ModelState[nameof(SetDisplayNameFormModel.Password)] as JArray)[0]);
            Assert.Equal(Strings.ErrorMessage_NewDisplayName_Required, (body.ModelState[nameof(SetDisplayNameFormModel.NewDisplayName)] as JArray)[0]);
        }

        [Fact]
        public async Task SetDisplayName_Returns400BadRequestAndSetDisplayNameResponseModelIfPasswordIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetDisplayName(cookies, _testInvalidPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetDisplayNameResponseModel body = JsonConvert.DeserializeObject<SetDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_Password_Invalid, (body.ModelState[nameof(SetDisplayNameFormModel.Password)] as JArray)[0]);
        }

        [Fact]
        public async Task SetDisplayName_Returns400BadRequestAndSetDisplayNameResponseModelIfNewDisplayNameIsInUse()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail2, _testPassword);
            await SetDisplayName(cookies, _testPassword, _testNewDisplayName);
            cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetDisplayName(cookies, _testPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetDisplayNameResponseModel body = JsonConvert.DeserializeObject<SetDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_DisplayName_InUse, (body.ModelState[nameof(SetDisplayNameFormModel.NewDisplayName)] as JArray)[0]);
        }

        [Fact]
        public async Task SetDisplayName_Returns400BadRequestAndSetDisplayNameResponseModelIfNewDisplayNameIsSameAsCurrentDisplayName()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            await SetDisplayName(cookies, _testPassword, _testNewDisplayName);

            // Act
            HttpResponseMessage httpResponseMessage = await SetDisplayName(cookies, _testPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SetDisplayNameResponseModel body = JsonConvert.DeserializeObject<SetDisplayNameResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_NewDisplayName_MustDiffer, (body.ModelState[nameof(SetDisplayNameFormModel.NewDisplayName)] as JArray)[0]);
        }

        [Fact]
        public async Task SetTwoFactorEnabled_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SetTwoFactorEnabled(null, _testNewTwoFactorEnabled);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetTwoFactorEnabled_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetTwoFactorEnabled(cookie, _testNewTwoFactorEnabled);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetTwoFactorEnabled_Returns200OKIfTwoFactorEnabledChangeSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            _testAccount = await _vakAccountRepository.GetAsync(_testAccountId, CancellationToken.None);
            await _vakAccountRepository.UpdateEmailVerifiedAsync(_testAccount, true, CancellationToken.None);

            // Act
            HttpResponseMessage httpResponseMessage = await SetTwoFactorEnabled(cookies, _testNewTwoFactorEnabled);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task SetTwoFactorEnabled_Returns400BadRequestSetTwoFactorEnabledFormModelAndSendsTwoFactorCodeEmailIfAccountEmailIsUnverified()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await SetTwoFactorEnabled(cookies, _testNewTwoFactorEnabled);

            // Assert
            SetTwoFactorEnabledResponseModel body = JsonConvert.DeserializeObject<SetTwoFactorEnabledResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.True(body.EmailUnverified);
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_TwoFactorCode, twoFactorEmail);
            Assert.Contains(_testEmail1, twoFactorEmail);
        }

        [Fact]
        public async Task SetEmailVerified_Returns200OKIfEmailVerifiedChangeSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            await SendEmailVerificationEmail(cookies);
            string email = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(email).Groups[1].Value);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmailVerified(cookies, token);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task SetEmailVerified_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmailVerified(cookie, null);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(
                await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetEmailVerified_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SetEmailVerified(null, null);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetEmailVerified_Returns400BadRequestIfTokenIsInvalidOrExpired()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1,
                _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetEmailVerified(cookies, _testToken);

            // Assert
            SetEmailVerifiedResponseModel body = JsonConvert.DeserializeObject<SetEmailVerifiedResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.True(body.InvalidToken);
        }

        [Fact]
        public async Task SetAltEmailVerified_Returns200OKIfAltEmailVerifiedChangeSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            await SetAltEmail(cookies, _testPassword, _testAltEmail);
            await SendAltEmailVerificationEmail(cookies);
            string email = File.ReadAllText(_tempEmailFile);
            Regex tokenRegex = new Regex(@"token=(.*?);");
            string token = Uri.UnescapeDataString(tokenRegex.Match(email).Groups[1].Value);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmailVerified(cookies, token);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task SetAltEmailVerified_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmailVerified(cookie, null);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(
                await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetAltEmailVerified_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmailVerified(null, null);

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SetAltEmailVerified_Returns400BadRequestIfTokenIsInvalidOrExpired()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1,
                _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await SetAltEmailVerified(cookies, _testToken);

            // Assert
            SetAltEmailVerifiedResponseModel body = JsonConvert.DeserializeObject<SetAltEmailVerifiedResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.ExpectedError);
            Assert.True(body.InvalidToken);
        }

        [Fact]
        public async Task SendEmailVerificationEmail_Returns200OkAndSendsEmailVerificationEmailIfEmailIsSentSuccessfully()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await SendEmailVerificationEmail(cookies);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            string EmailVerificationEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_EmailVerification, EmailVerificationEmail);
            Assert.Contains(_testEmail1, EmailVerificationEmail);
        }

        [Fact]
        public async Task SendEmailVerificationEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Act
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await SendEmailVerificationEmail(cookie);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(
                await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SendEmailVerificationEmail_Returns400BadRequestAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SendEmailVerificationEmail(null);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(
                await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SendAltEmailVerificationEmail_Returns200OkAndSendsAltEmailVerificationEmailIfEmailIsSentSuccessfully()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            await SetAltEmail(cookies, _testPassword, _testAltEmail);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await SendAltEmailVerificationEmail(cookies);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            string AltEmailVerificationEmail = File.ReadAllText(_tempEmailFile);
            Assert.Contains(Strings.Email_Subject_EmailVerification, AltEmailVerificationEmail);
            Assert.Contains(_testAltEmail, AltEmailVerificationEmail);
        }

        [Fact]
        public async Task SendAltEmailVerificationEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Act
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);
            HttpResponseMessage httpResponseMessage = await SendAltEmailVerificationEmail(cookie);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(
                await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SendAltEmailVerificationEmail_Returns400BadRequestAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await SendAltEmailVerificationEmail(null);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(
                await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task SendAltEmailVerificationEmail_Returns400BadRequestAndSendAltEmailVerificationEmailResponseModelIfAccountAltEmailIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            File.WriteAllText(_tempEmailFile, "");

            // Act
            HttpResponseMessage httpResponseMessage = await SendAltEmailVerificationEmail(cookies);

            // Assert
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            SendAltEmailVerificationEmailResponseModel body = JsonConvert.DeserializeObject<SendAltEmailVerificationEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.True(body.InvalidAltEmail);
        }

        [Fact]
        public async Task TwoFactorVerifyEmail_Returns400BadRequestAndTwoFactorVerifyEmailResponseModelIfTokenIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorVerifyEmail(cookies, _testToken);

            // Assert 
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            TwoFactorVerifyEmailResponseModel body = JsonConvert.DeserializeObject<TwoFactorVerifyEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_TwoFactorCode_Invalid, (body.ModelState[nameof(TwoFactorVerifyEmailFormModel.Code)] as JArray)[0]);
        }

        [Fact]
        public async Task TwoFactorVerifyEmail_Returns400BadRequestAndTwoFactorVerifyEmailResponseModelIfModelStateIsInvalid()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorVerifyEmail(cookies, "");

            // Assert 
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            TwoFactorVerifyEmailResponseModel body = JsonConvert.DeserializeObject<TwoFactorVerifyEmailResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.True(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_TwoFactorCode_Required, (body.ModelState[nameof(TwoFactorVerifyEmailFormModel.Code)] as JArray)[0]);
        }

        [Fact]
        public async Task TwoFactorVerifyEmail_Returns200OkIfEmailVerificationSucceeds()
        {
            // Arrange
            IDictionary<string, string> cookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1, _testPassword);
            await SetTwoFactorEnabled(cookies, true);
            string twoFactorEmail = File.ReadAllText(_tempEmailFile);
            Regex codeMatch = new Regex(@" (\d{6,6})");
            string code = codeMatch.Match(twoFactorEmail).Groups[1].Value;

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorVerifyEmail(cookies, code);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
        }

        [Fact]
        public async Task TwoFactorVerifyEmail_Returns401UnauthorizedAndErrorResponseModelIfAuthFails()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorVerifyEmail(null, "");

            // Assert
            Assert.Equal(_unauthorized, httpResponseMessage.StatusCode.ToString());
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task TwoFactorVerifyEmail_Returns400BadRequestAndErrorResponseModelIfAntiForgeryCredentialsAreInvalid()
        {
            // Arrange
            IDictionary<string, string> cookie = await GetApplicationCookie(_testEmail1, _testPassword);

            // Act
            HttpResponseMessage httpResponseMessage = await TwoFactorVerifyEmail(cookie, "");

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }
#endregion

        #region Helpers
        public async Task<HttpResponseMessage> SetTwoFactorEnabled(IDictionary<string, string> cookies,
            bool enabled)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SetTwoFactorEnabledFormModel.Enabled), enabled.ToString() }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SetTwoFactorEnabled)}", HttpMethod.Post, cookies, 
                formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SetEmailVerified(IDictionary<string, string> cookies,
            string token)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SetEmailVerifiedFormModel.Token), token }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SetEmailVerified)}", HttpMethod.Post, cookies,
                formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SetAltEmailVerified(IDictionary<string,string> cookies,
            string token)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SetAltEmailVerifiedFormModel.Token), token }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SetAltEmailVerified)}", HttpMethod.Post, cookies,
                formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SetDisplayName(IDictionary<string, string> cookies, string password,
            string newDisplayName)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SetDisplayNameFormModel.Password), password },
                { nameof(SetDisplayNameFormModel.NewDisplayName), newDisplayName }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SetDisplayName)}", HttpMethod.Post, cookies, formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SetAltEmail(IDictionary<string, string> cookies, string password,
            string newAltEmail)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SetAltEmailFormModel.Password), password },
                { nameof(SetAltEmailFormModel.NewAltEmail), newAltEmail }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SetAltEmail)}", HttpMethod.Post, cookies, formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SetEmail(IDictionary<string, string> cookies, string password, string newEmail)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SetEmailFormModel.Password), password },
                { nameof(SetEmailFormModel.NewEmail), newEmail }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SetEmail)}", HttpMethod.Post, cookies, formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SetPassword(IDictionary<string, string> cookies, string currentPassword,
            string newPassword, string confirmNewPassword)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SetPasswordFormModel.CurrentPassword), currentPassword },
                { nameof(SetPasswordFormModel.NewPassword), newPassword },
                { nameof(SetPasswordFormModel.ConfirmNewPassword), confirmNewPassword }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SetPassword)}", HttpMethod.Post, cookies, formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> ResetPassword(IDictionary<string, string> cookies, string email, string token,
            string newPassword, string confirmNewPassword)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(ResetPasswordFormModel.Email), email},
                { nameof(ResetPasswordFormModel.Token), token },
                { nameof(ResetPasswordFormModel.NewPassword), newPassword },
                { nameof(ResetPasswordFormModel.ConfirmPassword), confirmNewPassword }
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.ResetPassword)}",
                HttpMethod.Post,
                cookies,
                formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SendResetPasswordEmail(IDictionary<string, string> cookies, string email)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SendResetPasswordEmailFormModel.Email), email}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SendResetPasswordEmail)}",
                HttpMethod.Post,
                cookies,
                formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SendEmailVerificationEmail(IDictionary<string, string> cookies)
        {
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SendEmailVerificationEmail)}",
                HttpMethod.Post,
                cookies,
                null);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> SendAltEmailVerificationEmail(IDictionary<string, string> cookies)
        {
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_accountControllerName}/{nameof(AccountController.SendAltEmailVerificationEmail)}",
                HttpMethod.Post,
                cookies,
                null);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> TwoFactorLogIn(IDictionary<string, string> cookies, string code)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(TwoFactorLogInFormModel.Code), code},
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.TwoFactorLogIn)}", HttpMethod.Post, cookies, formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> GetDynamicFormWithAfTokens(string formModelName)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormController.GetDynamicFormWithAfTokens)}?formModelName={formModelName}", HttpMethod.Get, null);

            return await _httpClient.SendAsync(getDynamicFormGetRequest);
        }

        public async Task<HttpResponseMessage> GetDynamicForm(string formModelName)
        {
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormController.GetDynamicForm)}?formModelName={formModelName}", HttpMethod.Get, null);

            return await _httpClient.SendAsync(getDynamicFormGetRequest);
        }

        public async Task<HttpResponseMessage> SignUp(IDictionary<string, string> cookies,
            string email, string password, string confirmPassword)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(SignUpFormModel.Email), email},
                { nameof(SignUpFormModel.Password), password},
                { nameof(SignUpFormModel.ConfirmPassword),  confirmPassword}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.SignUp)}", HttpMethod.Post, cookies, formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> LogIn(IDictionary<string, string> cookies, string email, string password)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(LogInFormModel.Email), email},
                { nameof(LogInFormModel.Password), password}
            };
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.LogIn)}", HttpMethod.Post, cookies, formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> LogOff(IDictionary<string, string> cookies)
        {
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.LogOff)}", HttpMethod.Post, cookies, null);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> GetAccountDetails(IDictionary<string, string> cookies)
        {
            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.GetAccountDetails)}", HttpMethod.Get, cookies, null);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> TwoFactorVerifyEmail(IDictionary<string, string> cookies, string code)
        {
            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { nameof(TwoFactorVerifyEmailFormModel.Code), code}
            };

            HttpRequestMessage httpRequestMessage = RequestHelper.
                Create($"{_accountControllerName}/{nameof(AccountController.TwoFactorVerifyEmail)}", 
                HttpMethod.Post, 
                cookies, 
                formPostBodyData);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<IDictionary<string, string>> GetAnonymousAntiforgeryCookies()
        {
            HttpResponseMessage httpResponseMessage = await GetDynamicFormWithAfTokens(nameof(SignUpFormModel).Replace("FormModel", ""));
            return CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
        }

        public async Task<IDictionary<string, string>> GetApplicationAndAuthenticatedAntiforgeryCookies(string email, string password)
        {
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            HttpResponseMessage httpResponseMessage = await SignUp(antiforgeryCookies, email, password, password);
            IDictionary<string, string> result = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            result.Add(_cookieTokenName, antiforgeryCookies[_cookieTokenName]);

            return result;
        }

        public async Task<IDictionary<string, string>> GetTwoFactorAndAnonymousAntiforgeryCookies(string email, string password)
        {
            IDictionary<string, string> applicationAndAntiForgeryCookies = await GetApplicationAndAuthenticatedAntiforgeryCookies(_testEmail1,
                _testPassword);
            _testAccount = await _vakAccountRepository.GetAsync(_testAccountId, CancellationToken.None);
            await _vakAccountRepository.UpdateEmailVerifiedAsync(_testAccount, true, CancellationToken.None);
            await _vakAccountRepository.UpdateTwoFactorEnabledAsync(_testAccount, true, CancellationToken.None);

            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            HttpResponseMessage httpResponseMessage = await LogIn(antiforgeryCookies, _testEmail1, _testPassword);
            IDictionary<string, string> result = CookiesHelper.
                ExtractCookiesFromResponse(httpResponseMessage);
            result.Add(_cookieTokenName, antiforgeryCookies[_cookieTokenName]);
            result.Add(_requestTokenName, antiforgeryCookies[_requestTokenName]);

            return result;
        }

        public async Task<IDictionary<string, string>> GetApplicationCookie(string email, string password)
        {
            IDictionary<string, string> antiforgeryCookies = await GetAnonymousAntiforgeryCookies();
            HttpResponseMessage httpResponseMessage = await SignUp(antiforgeryCookies, email, password, password);
            IDictionary<string, string> result = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            result.Remove(_requestTokenName);

            return result;
        }
        #endregion
    }
}



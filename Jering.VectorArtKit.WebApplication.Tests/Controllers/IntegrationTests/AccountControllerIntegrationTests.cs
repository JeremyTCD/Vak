using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.Controllers;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using Microsoft.Net.Http.Headers;
using Moq;
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
        private VakAccountRepository vakAccountRepository { get; }
        private Func<Task> _resetAccountsTable { get; }
        public AccountControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
            vakAccountRepository = controllersFixture.VakAccountRepository;
            _resetAccountsTable = controllersFixture.ResetAccountsTable;
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
            Assert.True(html.Contains("<title>Log in - "));
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Theory]
        [MemberData(nameof(LoginPostData))]
        public async Task LoginPost_ReturnsLoginViewWithErrorMessageIfModelStateOrLoginCredentialsAreInvalid(string email, string password)
        {
            // Arrange
            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync("Email1@test.com", "Password1@");
            await vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            HttpResponseMessage loginResponse = await _httpClient.GetAsync("Account/Login");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", email},
                { "Password", password},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            HttpRequestMessage httpRequestMessage = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, formPostBodyData, loginResponse);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            StringOptions viewModelOptions = new StringOptions();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(html.Contains("<title>Log in - "));
            Assert.True(html.Contains(viewModelOptions.Login_Failed));
        }

        public static IEnumerable<object[]> LoginPostData()
        {
            yield return new object[] { "invalidModelState", "invalidModelState" };
            yield return new object[] { "invalid@Credentials", "@InvalidCredentials0"};
        }

        [Fact]
        public async Task LoginPost_RedirectsToHomeIndexViewAndSendsAuthenticationCookieIfLoginSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync("Email1@test.com", "Password1@");
            await vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            HttpResponseMessage loginResponse = await _httpClient.GetAsync("Account/Login");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            HttpRequestMessage httpRequestMessage = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, formPostBodyData, loginResponse);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

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

            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync(email, "Password1@");
            await vakAccountRepository.UpdateAccountTwoFactorEnabledAsync(1, true);
            await vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            File.WriteAllText(@"Temp\SmtpTest.txt", "");

            HttpResponseMessage loginResponse = await _httpClient.GetAsync("Account/Login");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", email},
                { "Password", "Password1@"},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            HttpRequestMessage httpRequestMessage = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, formPostBodyData, loginResponse);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

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
        public async Task SignUpGet_ReturnsSignUpViewWithAntiForgeryTokenAndCookie()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("Account/SignUp");

            // Assert
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(httpResponseMessage);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            string html = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(html.Contains("<title>Sign up - "));
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
                await vakAccountRepository.CreateAccountAsync(email, password);
            }

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

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            string html = await httpResponseMessage.Content.ReadAsStringAsync();
            StringOptions stringOptions = new StringOptions();
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(html.Contains("<title>Sign up - "));
            if (email == "Email1@test.com")
            {
                Assert.True(html.Contains(stringOptions.SignUp_AccountWithEmailExists));
            }
            else
            {
                Assert.True(html.Contains(stringOptions.Email_Invalid));
                Assert.True(html.Contains(stringOptions.Password_TooShort));
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

            HttpResponseMessage signUpGetResponse = await _httpClient.GetAsync("Account/SignUp");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(signUpGetResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "ConfirmPassword", "Password1@" },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            HttpRequestMessage signUpPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/SignUp", HttpMethod.Post, formPostBodyData, signUpGetResponse);

            // Act
            HttpResponseMessage signUpPostResponse = await _httpClient.SendAsync(signUpPostRequest);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(signUpPostResponse);
            Assert.Equal("Jering.Application", cookies.Keys.First());
            Assert.Equal("Redirect", signUpPostResponse.StatusCode.ToString());
            Assert.Equal("/", signUpPostResponse.Headers.Location.ToString());
            StringOptions stringOptions = new StringOptions();
            string confirmEmailEmail = File.ReadAllText(@"Temp\SmtpTest.txt");
            Assert.Contains(stringOptions.ConfirmEmail_Subject, confirmEmailEmail);
            Assert.Contains("Email1@test.com", confirmEmailEmail);
            Assert.Matches(stringOptions.ConfirmEmail_Message.Replace("{0}", $"http://localhost/{nameof(AccountController).Replace("Controller", "")}/{nameof(AccountController.ConfirmEmail)}.*?"), confirmEmailEmail);
        }

        [Fact]
        public async Task LogOffPost_RedirectsToHomeIndexViewAndSendsHeaderToRemoveAllCookiesIfSuccessful()
        {
            //Arrange
            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync("Email1@test.com", "Password1@");
            await vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            HttpResponseMessage loginGetResponse = await _httpClient.GetAsync("Account/Login");

            IDictionary<string, string> loginPostBody = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginGetResponse) }
            };
            HttpRequestMessage loginPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, loginPostBody, loginGetResponse);
            HttpResponseMessage loginPostResponse = await _httpClient.SendAsync(loginPostRequest);

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
            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync("Email1@test.com", "Password1@");
            await vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            HttpResponseMessage loginGetResponse = await _httpClient.GetAsync("Account/Login");

            IDictionary<string, string> loginPostBody = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginGetResponse) }
            };
            HttpRequestMessage loginPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, loginPostBody, loginGetResponse);
            HttpResponseMessage loginPostResponse = await _httpClient.SendAsync(loginPostRequest);

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
            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync("Email1@test.com", "Password1@");
            await vakAccountRepository.UpdateAccountEmailConfirmedAsync(1);

            HttpResponseMessage loginGetResponse = await _httpClient.GetAsync("Account/Login");

            IDictionary<string, string> loginPostBody = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginGetResponse) }
            };
            HttpRequestMessage loginPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, loginPostBody, loginGetResponse);
            HttpResponseMessage loginPostResponse = await _httpClient.SendAsync(loginPostRequest);

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
    }
}



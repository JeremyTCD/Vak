using Jering.VectorArtKit.WebApi.BusinessModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [Collection("ControllersCollection")]
    public class HomeControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        private VakAccountRepository vakAccountRepository { get; }
        private Func<Task> _resetAccountsTable { get; }
        public HomeControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
            vakAccountRepository = controllersFixture.VakAccountRepository;
            _resetAccountsTable = controllersFixture.ResetAccountsTable;
        }

        [Fact]
        public async Task IndexGet_ReturnsHomeIndexViewWithLogOffFormWhenAccountIsLoggedIn()
        {
            //Arrange
            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync("Email1@test.com", "Password1@");
            await vakAccountRepository.UpdateAccountEmailVerifiedAsync(1, true);

            HttpResponseMessage loginGetResponse = await _httpClient.GetAsync("Account/Login");

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginGetResponse) }
            };
            HttpRequestMessage loginPostRequest = RequestHelper.CreateWithCookiesFromResponse("Account/Login", HttpMethod.Post, formPostBodyData, loginGetResponse);
            HttpResponseMessage loginPostResponse = await _httpClient.SendAsync(loginPostRequest);

            HttpRequestMessage indexGetRequest = RequestHelper.CreateWithCookiesFromResponse("Home/Index", HttpMethod.Get, null, loginPostResponse);

            // Act
            HttpResponseMessage indexGetResponse = await _httpClient.SendAsync(indexGetRequest);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(indexGetResponse);
            string html = await indexGetResponse.Content.ReadAsStringAsync();

            Assert.Equal("OK", indexGetResponse.StatusCode.ToString());
            Assert.Contains("Email1@test.com", html);
            Assert.Contains("LogOff", html);
        }

        [Fact]
        public async Task IndexGet_ReturnsHomeIndexViewWithLoginAndSignUpButtonsWhenAccountIsNotLoggedIn()
        {
            // Act
            HttpResponseMessage indexGetResponse = await _httpClient.GetAsync("Home/Index");

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(indexGetResponse);
            string html = await indexGetResponse.Content.ReadAsStringAsync();

            Assert.Equal("OK", indexGetResponse.StatusCode.ToString());
            Assert.Contains("SignUp", html);
            Assert.Contains("Login", html);
        }
    }
}



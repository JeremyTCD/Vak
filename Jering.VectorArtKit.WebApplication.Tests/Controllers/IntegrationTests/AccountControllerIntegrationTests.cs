using Jering.VectorArtKit.WebApplication.BusinessModel;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
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
            Assert.True(html.Contains("<title>Log in - Jering.VectorArtKit.WebApplication</title>"));
            Assert.NotNull(antiForgeryToken);
            Assert.True(cookies.Keys.First().Contains(".AspNetCore.Antiforgery"));
        }

        [Fact]
        public async Task LoginPost_RedirectsToHomeIndexViewAndSendsAuthenticationCookieIfLoginSucceeds()
        {
            // Arrange
            await _resetAccountsTable();
            await vakAccountRepository.CreateAccountAsync("Email1@test.com", "Password1@");

            HttpResponseMessage loginResponse = await _httpClient.GetAsync("Account/Login");
            string antiForgeryToken = await AntiForgeryTokenHelper.ExtractAntiForgeryToken(loginResponse);

            IDictionary<string, string> formPostBodyData = new Dictionary<string, string>
            {
                { "Email", "Email1@test.com"},
                { "Password", "Password1@"},
                { "RememberMe", "true" },
                { "__RequestVerificationToken", antiForgeryToken }
            };

            HttpRequestMessage httpRequestMessage = PostRequestHelper.CreateWithCookiesFromResponse("Account/Login", formPostBodyData, loginResponse);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.True(cookies.ContainsKey(".AspNetCore.Jering.Application"));
            Assert.Equal("Redirect", httpResponseMessage.StatusCode.ToString());
            Assert.Equal("/", httpResponseMessage.Headers.Location.ToString());
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

            HttpRequestMessage httpRequestMessage = PostRequestHelper.Create("Account/Login", formPostBodyData);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal("BadRequest", httpResponseMessage.StatusCode.ToString());
        }


        public class PostRequestHelper
        {
            public static HttpRequestMessage Create(string path, IDictionary<string, string> formPostBodyData)
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, path)
                {
                    Content = new FormUrlEncodedContent(ToFormPostData(formPostBodyData))
                };
                return httpRequestMessage;
            }

            public static List<KeyValuePair<string, string>> ToFormPostData(IDictionary<string, string> formPostBodyData)
            {
                List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
                formPostBodyData.Keys.ToList().ForEach(key =>
                {
                    result.Add(new KeyValuePair<string, string>(key, formPostBodyData[key]));
                });
                return result;
            }

            public static HttpRequestMessage CreateWithCookiesFromResponse(string path, IDictionary<string, string> formPostBodyData,
                HttpResponseMessage response)
            {
                var httpRequestMessage = Create(path, formPostBodyData);
                return CookiesHelper.CopyCookiesFromResponse(httpRequestMessage, response);
            }
        }

        public class AntiForgeryTokenHelper
        {
            public static string ExtractAntiForgeryToken(string htmlResponseText)
            {
                if (htmlResponseText == null) throw new ArgumentNullException("htmlResponseText");

                System.Text.RegularExpressions.Match match = Regex.Match(htmlResponseText, @"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>");
                return match.Success ? match.Groups[1].Captures[0].Value : null;
            }

            public static async Task<string> ExtractAntiForgeryToken(HttpResponseMessage response)
            {
                string responseAsString = await response.Content.ReadAsStringAsync();
                return await Task.FromResult(ExtractAntiForgeryToken(responseAsString));
            }
        }

        public class CookiesHelper
        {
            public static IDictionary<string, string> ExtractCookiesFromResponse(HttpResponseMessage response)
            {
                IDictionary<string, string> result = new Dictionary<string, string>();
                IEnumerable<string> values;
                if (response.Headers.TryGetValues("Set-Cookie", out values))
                {
                    SetCookieHeaderValue.ParseList(values.ToList()).ToList().ForEach(cookie =>
                    {
                        result.Add(cookie.Name, cookie.Value);
                    });
                }
                return result;
            }

            public static HttpRequestMessage PutCookiesOnRequest(HttpRequestMessage request, IDictionary<string, string> cookies)
            {
                cookies.Keys.ToList().ForEach(key =>
                {
                    request.Headers.Add("Cookie", new CookieHeaderValue(key, cookies[key]).ToString());
                });

                return request;
            }

            public static HttpRequestMessage CopyCookiesFromResponse(HttpRequestMessage request, HttpResponseMessage response)
            {
                return PutCookiesOnRequest(request, ExtractCookiesFromResponse(response));
            }
        }
    }
}



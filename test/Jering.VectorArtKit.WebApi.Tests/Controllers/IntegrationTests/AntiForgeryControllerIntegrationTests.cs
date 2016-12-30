using Jering.Utilities;
using Jering.VectorArtKit.DatabaseInterface;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.RequestModels.Account;
using Jering.VectorArtKit.WebApi.RequestModels.DynamicForm;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.DynamicForm;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [Collection("ControllersCollection")]
    public class AntiForgeryControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        private string _antiForgeryControllerName = nameof(AntiForgeryController).Replace("Controller", "");
        private string _ok { get; } = "OK";

        public AntiForgeryControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
        }

        [Fact]
        public async Task GetAntiForgeryTokens_Returns200OkAndAntiForgeryCookies()
        {
            //Arrange
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_antiForgeryControllerName}/" +
                $"{nameof(AntiForgeryController.GetAntiForgeryTokens)}", HttpMethod.Get, null);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(2, cookies.Count());
            Assert.True(cookies.Keys.Contains("AF-TOKEN"));
            Assert.True(cookies.Keys.Contains("XSRF-TOKEN"));
        }
    }
}



using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Hosting;
using Jering.VectorArtKit.WebApi;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using Jering.VectorArtKit.WebApi.Resources;
using Microsoft.AspNetCore.Authorization;

namespace Jering.VectorArtKit.WebApplication.Controllers.IntegrationTests.Controllers.IntegrationTests
{
    public class ErrorHandlingIntegrationTests : IClassFixture<ErrorHandlingFixture>
    {
        private HttpClient _httpClient { get; set; }
        private TestServer _testServcer { get; set; }

        public ErrorHandlingIntegrationTests(ErrorHandlingFixture fixture)
        {
            _httpClient = fixture.HttpClient;
            _testServcer = fixture.TestServer;
        }

        [Fact]
        public async Task ErrorHandling_StatusCodePages_AddsErrorMessageBodyTo500InternalServerErrorResponseWithNoBody()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("ErrorHandlingTest/InternalServerError");

            // Assert
            dynamic modelState = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("InternalServerError", httpResponseMessage.StatusCode.ToString());
            Assert.Equal(modelState.unexpectedError, Strings.ErrorMessage_UnexpectedError);
        }

        [Fact]
        public async Task ErrorHandling_StatusCodePages_AddsErrorMessageBodyTo404NotFoundResponseWithNoBody()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("ErrorHandlingTest/NotFound");

            // Assert
            dynamic modelState = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("NotFound", httpResponseMessage.StatusCode.ToString());
            Assert.Equal(modelState.unexpectedError, Strings.ErrorMessage_UnexpectedError);
        }

        [Fact]
        public async Task ErrorHandling_StatusCodePages_AddsErrorMessageBodyTo401UnauthorizedResponseWithNoBody()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("ErrorHandlingTest/Authorize");

            // Assert
            dynamic modelState = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("Unauthorized", httpResponseMessage.StatusCode.ToString());
            Assert.Equal(modelState.unexpectedError, Strings.ErrorMessage_UnexpectedError);
        }

        [Fact]
        public async Task ErrorHandling_StatusCodePages_AddsErrorMessageBodyTo400BadRequestResponseWithNoBody()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("ErrorHandlingTest/ValidateAntiForgeryToken");

            // Assert
            dynamic modelState = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("BadRequest", httpResponseMessage.StatusCode.ToString());
            Assert.Equal(modelState.unexpectedError, Strings.ErrorMessage_UnexpectedError);
        }

        [Fact]
        public async Task ErrorHandling_ExceptionHandler_CatchesExceptionAndSetsStatusCodeTo500InternalServerError()
        {
            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("ErrorHandlingTest/Exception");

            // Assert
            dynamic modelState = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("InternalServerError", httpResponseMessage.StatusCode.ToString());
            Assert.Equal(modelState.unexpectedError, Strings.ErrorMessage_UnexpectedError);
        }
    }

    public class ErrorHandlingFixture : IDisposable
    {
        public TestServer TestServer;
        public HttpClient HttpClient;
        public IConfigurationRoot ConfigurationRoot { get; }

        public ErrorHandlingFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("project.json");
            ConfigurationRoot = configurationBuilder.Build();

            string testProjectPath = PlatformServices.Default.Application.ApplicationBasePath;

            TestServer = new TestServer(new WebHostBuilder().
                UseEnvironment("Production").
                UseContentRoot(testProjectPath).
                ConfigureServices(InitializeServices).
                UseStartup(typeof(Startup))
            );
            HttpClient = TestServer.CreateClient();
            HttpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        }

        public void Dispose()
        {
            TestServer.Dispose();
            HttpClient.Dispose();
        }

        private void InitializeServices(IServiceCollection services)
        {
            Assembly startupAssembly = typeof(Startup).GetTypeInfo().Assembly;

            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));
            manager.ApplicationParts.Add(new AssemblyPart(typeof(ErrorHandlingTestController).GetTypeInfo().Assembly));

            services.AddSingleton(manager);
        }
    }

    public class ErrorHandlingTestController : Controller
    {
        /// <summary>
        /// Returns empty 500 internal server error.
        /// </summary>
        /// <returns>
        /// 500 internal server error with no body.
        /// </returns>
        public IActionResult InternalServerError()
        {
            return StatusCode(500);
        }

        /// <summary>
        /// Throws exception.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public IActionResult Exception()
        {
            throw new Exception();
        }

        /// <summary>
        /// Requires authentication.
        /// </summary>
        [Authorize]
        public IActionResult Authorize()
        {
            return Ok();
        }

        /// <summary>
        /// Requires anti-forgery token.
        /// </summary>
        /// <exception cref="Exception"
        [ValidateAntiForgeryToken]
        public IActionResult ValidateAntiForgeryToken()
        {
            return Ok();
        }
    }
}

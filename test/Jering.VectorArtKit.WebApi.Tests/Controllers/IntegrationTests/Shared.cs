using Jering.VectorArtKit.DatabaseInterface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [CollectionDefinition("ControllersCollection")]
    public class ControllersCollection : ICollectionFixture<ControllersFixture>
    {

    }

    public class ControllersFixture : IDisposable 
    {
        public TestServer TestServer;
        public HttpClient HttpClient;
        public IConfigurationRoot ConfigurationRoot { get; }
        public DbContextOptions<VakDbContext> DbContextOptions { get; }

        public ControllersFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("project.json");
            ConfigurationRoot = configurationBuilder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<VakDbContext>();
            optionsBuilder.UseSqlServer(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);

            DbContextOptions = optionsBuilder.Options;

            string testProjectPath = PlatformServices.Default.Application.ApplicationBasePath;
            string webApplicationProjectPath = Path.GetFullPath(Path.Combine(testProjectPath, @"..\..\..\..\..\src\Jering.VectorArtKit.WebApi"));

            TestServer = new TestServer(new WebHostBuilder().
                UseEnvironment("Development").
                UseContentRoot(webApplicationProjectPath).
                UseStartup<Startup>());
            HttpClient = TestServer.CreateClient();
        }

        public void Dispose()
        {
            TestServer.Dispose();
            HttpClient.Dispose();
        }

        public void ResetAccountsTable(VakDbContext dbContext)
        {
            dbContext.Database.ExecuteSqlCommand("Delete from [dbo].[Accounts]; DBCC CHECKIDENT('[dbo].[Accounts]', RESEED, 0);");
        }
    }

    #region Helpers
    public class RequestHelper
    {
        public static HttpRequestMessage Create(string path, HttpMethod requestMethod, IDictionary<string, string> formPostBodyData = null)
        {
            if (formPostBodyData == null) {
                formPostBodyData = new Dictionary<string, string>();
            }

            var httpRequestMessage = new HttpRequestMessage(requestMethod, path)
            {
                Content = new StringContent(ToJson(formPostBodyData), Encoding.UTF8, "application/json")
            };

            httpRequestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

            return httpRequestMessage;
        }

        public static HttpRequestMessage Create(string path, HttpMethod requestMethod, 
            IDictionary<string, string> cookies = null,
            IDictionary<string, string> formPostBodyData = null)
        {
            HttpRequestMessage httpRequestMessage = Create(path, requestMethod, formPostBodyData);

            if (cookies != null)
            {
                if (cookies.Keys.Contains("XSRF-TOKEN"))
                {
                    httpRequestMessage.Headers.Add("X-XSRF-TOKEN", cookies["XSRF-TOKEN"]);
                }
                CookiesHelper.PutCookiesOnRequest(httpRequestMessage, cookies);
            }

            return httpRequestMessage;
        }

        public static string ToJson(IDictionary<string, string> formPostBodyData)
        {
            return JsonConvert.SerializeObject(formPostBodyData);
        }

        public static HttpRequestMessage CreateWithCookiesFromResponse(string path, HttpMethod requestMethod, IDictionary<string, string> formPostBodyData,
            HttpResponseMessage response)
        {
            var httpRequestMessage = Create(path, requestMethod, formPostBodyData);
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
    #endregion
}

using System;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Microsoft.Extensions.PlatformAbstractions;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jering.VectorArtKit.WebApplication.Tests.Controllers.IntegrationTests
{
    [CollectionDefinition("ControllersCollection")]
    public class ControllersCollection : ICollectionFixture<ControllersFixture>
    {

    }

    public class ControllersFixture : IDisposable 
    {
        public TestServer TestServer;
        public HttpClient HttpClient;
        public VakAccountRepository VakAccountRepository { get; }
        public SqlConnection SqlConnection { get; }
        public IConfigurationRoot ConfigurationRoot { get; }

        public ControllersFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("project.json");
            ConfigurationRoot = configurationBuilder.Build();
            SqlConnection = new SqlConnection(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);

            string testProjectPath = PlatformServices.Default.Application.ApplicationBasePath;
            string webApplicationProjectPath = Path.GetFullPath(Path.Combine(testProjectPath, @"..\..\..\..\Jering.VectorArtKit.WebApplication"));

            VakAccountRepository = new VakAccountRepository(SqlConnection);

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

        public async Task ResetAccountsTable()
        {
            await SqlConnection.ExecuteAsync("Delete from [dbo].[Accounts];" +
                "DBCC CHECKIDENT('[dbo].[Accounts]', RESEED, 0);", commandType: CommandType.Text);
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

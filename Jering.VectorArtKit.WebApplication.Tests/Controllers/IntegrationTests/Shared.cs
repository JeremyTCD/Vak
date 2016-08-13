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
}

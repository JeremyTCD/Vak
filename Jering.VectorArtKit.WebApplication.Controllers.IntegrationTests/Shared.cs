using System;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;

namespace Jering.VectorArtKit.WebApplication.Controllers.IntegrationTests
{
    [CollectionDefinition("ControllersCollection")]
    public class ControllersCollection : ICollectionFixture<ControllersFixture>
    {

    }

    public class ControllersFixture : IDisposable 
    {
        public TestServer _testServer;
        public HttpClient _httpClient;

        public ControllersFixture()
        {
            _testServer = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _httpClient = _testServer.CreateClient();
        }

        public void Dispose()
        {
            _testServer.Dispose();
            _httpClient.Dispose();
        }
    }
}

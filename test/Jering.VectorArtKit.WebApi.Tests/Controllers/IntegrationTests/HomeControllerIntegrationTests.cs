using Jering.Utilities;
using Jering.VectorArtKit.DatabaseInterface;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [Collection("ControllersCollection")]
    public class HomeControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        private VakAccountRepository _vakAccountRepository { get; }
        public HomeControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
            VakDbContext dbContext = new VakDbContext(controllersFixture.DbContextOptions);
            _vakAccountRepository = new VakAccountRepository(dbContext, new TimeService());
            controllersFixture.ResetAccountsTable(dbContext);
        }
    }
}



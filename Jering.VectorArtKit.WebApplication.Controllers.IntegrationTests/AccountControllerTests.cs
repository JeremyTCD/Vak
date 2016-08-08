using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApplication.Controllers.IntegrationTests
{
    [Collection("ControllersFixture")]
    public class AccountControllerTests
    {
        private HttpClient _httpClient { get; }

        public AccountControllerTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture._httpClient;
        }
    }
}

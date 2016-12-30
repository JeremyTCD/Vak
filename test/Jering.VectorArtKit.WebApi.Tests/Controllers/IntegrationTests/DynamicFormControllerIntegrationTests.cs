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
    public class DynamicFormControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        private VakAccountRepository _vakAccountRepository { get; }
        private string _dynamicFormControllerName = nameof(DynamicFormController).Replace("Controller", "");
        private string _notFound { get; } = "NotFound";
        private string _badRequest { get; } = "BadRequest";
        private string _ok { get; } = "OK";

        private const string _testEmail = "test@email.com";
        private const string _testPassword = "testPassword";
        private const string _testPasswordHash = "testPasswordHash";
        private const string _testDisplayName = "testDisplayName";
        private const string _testFormModelName = "testFormModelName";

        public DynamicFormControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
            VakDbContext dbContext = new VakDbContext(controllersFixture.DbContextOptions);
            _vakAccountRepository = new VakAccountRepository(dbContext, new TimeService());
            controllersFixture.ResetAccountsTable(dbContext);
        }

        [Fact]
        public async Task GetDynamicForm_Returns200OkAndGetDynamicFormResponseModelIfFormModelNameIsTheNameOfAnExistingFormModel()
        {
            //Arrange
            string signUpRequestModelName = nameof(SignUpRequestModel);
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/" +
                $"{nameof(DynamicFormController.GetDynamicForm)}?" +
                $"{nameof(GetDynamicFormRequestModel.requestModelName)}={signUpRequestModelName}", HttpMethod.Get, null);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            GetDynamicFormResponseModel body = JsonConvert.DeserializeObject<GetDynamicFormResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.NotNull(body);
            Assert.Equal(3, body.DynamicFormData.DynamicControlData.Count);
        }

        [Fact]
        public async Task GetDynamicForm_Returns404NotFoundAndErrorResponseModelIfFormModelNameIsNotTheNameOfAnExistingFormModel()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/" +
                $"{nameof(DynamicFormController.GetDynamicForm)}?{nameof(GetDynamicFormRequestModel.requestModelName)}={_testFormModelName}", HttpMethod.Get, null);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_notFound, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);

        }
    }
}



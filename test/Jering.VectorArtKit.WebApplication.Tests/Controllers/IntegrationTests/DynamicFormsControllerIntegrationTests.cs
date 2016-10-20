using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Controllers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApi.Tests.Controllers.IntegrationTests
{
    [Collection("ControllersCollection")]
    public class DynamicFormsControllerIntegrationTests
    {
        private HttpClient _httpClient { get; }
        public DynamicFormsControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;
        }

        [Fact]
        public async Task GetDynamicForm_ReturnsJsonRepresentationOfDynamicFormForSpecifiedFormModel()
        {
            //Arrange
            string testFormModel = "SignUp";
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"api/{nameof(DynamicFormsController).Replace("Controller", "")}/" + 
                $"{nameof(DynamicFormsController.GetDynamicForm)}?viewModelName={testFormModel}", HttpMethod.Get);

            // Act
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);

            // Assert
            string json = await getDynamicFormGetResponse.Content.ReadAsStringAsync();
            DynamicFormData dynamicForm = JsonConvert.DeserializeObject<DynamicFormData>(json);
            Assert.Equal(3, dynamicForm.DynamicControlDatas.Count);
        }
    }
}



using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Controllers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
        public async Task GetDynamicForm_Returns200OkWithJsonRepresentationOfDynamicFormAndValidationForgeryCookiesIfFormModelNameIsTheNameOfAnExistingFormModel()
        {
            //Arrange
            string testFormModel = "SignUp";
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{nameof(DynamicFormsController).Replace("Controller", "")}/" + 
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={testFormModel}", HttpMethod.Get);

            // Act
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);

            // Assert
            Assert.Equal("OK", getDynamicFormGetResponse.StatusCode.ToString());
            string json = await getDynamicFormGetResponse.Content.ReadAsStringAsync();
            DynamicFormData dynamicForm = JsonConvert.DeserializeObject<DynamicFormData>(json);
            Assert.NotNull(dynamicForm);
            Assert.Equal(3, dynamicForm.DynamicControlDatas.Count);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(getDynamicFormGetResponse);
            Assert.Equal(2, cookies.Count());
            Assert.True(cookies.Keys.Contains("AF-TOKEN"));
            Assert.True(cookies.Keys.Contains("XSRF-TOKEN"));
        }

        [Fact]
        public async Task GetDynamicForm_Returns404NotFoundIfFormModelNameIsNotTheNameOfAnExistingFormModel()
        {
            //Arrange
            string testFormModel = "Dummy";
            HttpRequestMessage getDynamicFormGetRequest = RequestHelper.Create($"{nameof(DynamicFormsController).Replace("Controller", "")}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={testFormModel}", HttpMethod.Get);

            // Act
            HttpResponseMessage getDynamicFormGetResponse = await _httpClient.SendAsync(getDynamicFormGetRequest);

            // Assert
            Assert.Equal("NotFound", getDynamicFormGetResponse.StatusCode.ToString());
        }
    }
}



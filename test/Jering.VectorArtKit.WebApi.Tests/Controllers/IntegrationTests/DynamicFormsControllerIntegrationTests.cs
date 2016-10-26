using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
        private VakAccountRepository _vakAccountRepository { get; }
        private Func<Task> _resetAccountsTable { get; }
        private string _dynamicFormsControllerName = nameof(DynamicFormsController).Replace("Controller", "");
        public DynamicFormsControllerIntegrationTests(ControllersFixture controllersFixture)
        {
            _httpClient = controllersFixture.HttpClient;     
            _vakAccountRepository = controllersFixture.VakAccountRepository;
            _resetAccountsTable = controllersFixture.ResetAccountsTable;
        }

        [Fact]
        public async Task GetDynamicForm_Returns200OkWithJsonRepresentationOfDynamicFormAndValidationForgeryCookiesIfFormModelNameIsTheNameOfAnExistingFormModel()
        {
            //Arrange
            string testFormModel = "SignUp";
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormsControllerName}/" + 
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={testFormModel}", HttpMethod.Get);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            DynamicFormData dynamicForm = JsonConvert.DeserializeObject<DynamicFormData>(json);
            Assert.NotNull(dynamicForm);
            Assert.Equal(3, dynamicForm.DynamicControlDatas.Count);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(2, cookies.Count());
            Assert.True(cookies.Keys.Contains("AF-TOKEN"));
            Assert.True(cookies.Keys.Contains("XSRF-TOKEN"));
        }

        [Fact]
        public async Task GetDynamicForm_Returns404NotFoundIfFormModelNameIsNotTheNameOfAnExistingFormModel()
        {
            // Arrange
            string testFormModel = "Dummy";
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormsControllerName}/" +
                $"{nameof(DynamicFormsController.GetDynamicForm)}?formModelName={testFormModel}", HttpMethod.Get);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            dynamic body = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("NotFound", httpResponseMessage.StatusCode.ToString());
            Assert.Equal(body.errorMessage, Strings.ErrorMessage_UnexpectedError);
        }

        [Fact]
        public async Task ValidateEmailNotInUse_Returns400BadRequestIfValueIsNull()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormsControllerName}/{nameof(DynamicFormsController.ValidateEmailNotInUse)}", HttpMethod.Get);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            dynamic body = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("BadRequest", httpResponseMessage.StatusCode.ToString());
            Assert.Equal(body.errorMessage, Strings.ErrorMessage_UnexpectedError);
        }

        [Fact]
        public async Task ValidateEmailNotInUse_Returns200OkWithJsonBodyWithValidPropertySetToFalseIfEmailIsInUse()
        {
            // Arrange
            string testEmail = "test@email.com";
            await _resetAccountsTable();
            await _vakAccountRepository.CreateAccountAsync(testEmail, "testPassword");
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormsControllerName}/{nameof(DynamicFormsController.ValidateEmailNotInUse)}?value={testEmail}", HttpMethod.Get);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            dynamic body = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.False(body.valid);
        }

        [Fact]
        public async Task ValidateEmailNotInUse_Returns200OkWithJsonBodyWithValidPropertySetToTrueIfEmailIsNotInUse()
        {
            // Arrange
            await _resetAccountsTable();
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormsControllerName}/{nameof(DynamicFormsController.ValidateEmailNotInUse)}?value=test@email.com", HttpMethod.Get);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            dynamic body = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync(), new ExpandoObjectConverter());
            Assert.Equal("OK", httpResponseMessage.StatusCode.ToString());
            Assert.True(body.valid);
        }
    }
}



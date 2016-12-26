﻿using Jering.DynamicForms;
using Jering.Utilities;
using Jering.VectorArtKit.DatabaseInterface;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.RequestModels.DynamicForm;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.DynamicForms;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
        public async Task GetDynamicForm_Returns200OkAntiForgeryTokensIfGetAfTokensIsTrueAndGetDynamicFormModelIfFormModelNameIsTheNameOfAnExistingFormModel()
        {
            //Arrange
            string signUpRequestModelName = "SignUp";
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/" +
                $"{nameof(DynamicFormController.GetDynamicForm)}?" + 
                $"{nameof(GetDynamicFormRequestModel.requestModelName)}={signUpRequestModelName}" +
                $"&{nameof(GetDynamicFormRequestModel.getAfTokens)}={true}", HttpMethod.Get, null);

            // Act
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            DynamicFormResponseModel body = JsonConvert.DeserializeObject<DynamicFormResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.NotNull(body);
            Assert.Equal(3, body.DynamicControlResponseModels.Count);
            IDictionary<string, string> cookies = CookiesHelper.ExtractCookiesFromResponse(httpResponseMessage);
            Assert.Equal(2, cookies.Count());
            Assert.True(cookies.Keys.Contains("AF-TOKEN"));
            Assert.True(cookies.Keys.Contains("XSRF-TOKEN"));
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

        [Fact]
        public async Task ValidateEmailNotInUse_Returns400BadRequestAndErrorResponseModelIfValueIsNull()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/{nameof(DynamicFormController.ValidateEmailNotInUse)}", HttpMethod.Get, null);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ValidateEmailNotInUse_Returns200OkAndValidateResponseModelIfEmailIsInUse()
        {
            // Arrange
            await _vakAccountRepository.CreateAsync(_testEmail, _testPasswordHash, CancellationToken.None);
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/{nameof(DynamicFormController.ValidateEmailNotInUse)}?value={_testEmail}", HttpMethod.Get, null);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ValidateResponseModel body = JsonConvert.DeserializeObject<ValidateResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.Valid);
        }

        [Fact]
        public async Task ValidateEmailNotInUse_Returns200OkAndValidateResponseModelIfEmailIsNotInUse()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/{nameof(DynamicFormController.ValidateEmailNotInUse)}?value={_testEmail}", HttpMethod.Get, null);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ValidateResponseModel body = JsonConvert.DeserializeObject<ValidateResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.Valid);
        }

        [Fact]
        public async Task ValidateDisplayNameNotInUse_Returns400BadRequestAndErrorResponseModelIfValueIsNull()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/{nameof(DynamicFormController.ValidateDisplayNameNotInUse)}", HttpMethod.Get, null);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ErrorResponseModel body = JsonConvert.DeserializeObject<ErrorResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_badRequest, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.ExpectedError);
            Assert.Equal(Strings.ErrorMessage_UnexpectedError, body.ErrorMessage);
        }

        [Fact]
        public async Task ValidateDisplayNameNotInUse_Returns200OkAndValidateResponseModelIfDisplayNameIsInUse()
        {
            // Arrange
            VakAccount account = await _vakAccountRepository.CreateAsync(_testEmail, _testPasswordHash, CancellationToken.None);
            await _vakAccountRepository.UpdateDisplayNameAsync(account, _testDisplayName, CancellationToken.None);
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/{nameof(DynamicFormController.ValidateDisplayNameNotInUse)}?value={_testDisplayName}", HttpMethod.Get, null);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ValidateResponseModel body = JsonConvert.DeserializeObject<ValidateResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            Assert.False(body.Valid);
        }

        [Fact]
        public async Task ValidateDisplayNameNotInUse_Returns200OkAndValidateResponseModelIfDisplayNameIsNotInUse()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = RequestHelper.Create($"{_dynamicFormControllerName}/{nameof(DynamicFormController.ValidateDisplayNameNotInUse)}?value={_testDisplayName}", HttpMethod.Get, null);

            // Act 
            HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);

            // Assert
            ValidateResponseModel body = JsonConvert.DeserializeObject<ValidateResponseModel>(await httpResponseMessage.Content.ReadAsStringAsync());
            Assert.Equal(_ok, httpResponseMessage.StatusCode.ToString());
            Assert.True(body.Valid);
        }
    }
}



using Jering.DynamicForms;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using System.Reflection;
using Jering.DataAnnotations;
using Jering.DynamicForms.Tests.Resources;

namespace Jering.DynamicForms.Tests
{
    public class DynamicFormsServicesUnitTests
    {
        [Fact]
        public void ConvertToDynamicInput_ConvertsPropertyWithDynamicInputAttributeIntoDynamicInput()
        {
            // Arrange
            DynamicFormsServices dynamicFormServices = new DynamicFormsServices();

            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Email));

            // Act
            DynamicInput result = dynamicFormServices.ConvertToDynamicInput(propertyInfo);

            // Assert
            Assert.Equal("Email", result.Name);
            Assert.NotNull(result.ValidatorData);
            Assert.False(result.RenderLabel);
            Assert.Equal("email", result.Type);
            Assert.Equal(0, result.Order);
            Assert.Equal("input", result.TagName);
            Assert.Equal(DummyStrings.Dummy, result.InitialValue);
            Assert.Equal(DummyStrings.Dummy, result.DisplayName);
            Assert.Equal(DummyStrings.Dummy, result.Placeholder);
        }

        [Fact]
        public void ConvertToDynamicInput_ReturnsNullForPropertyWithNoDynamicInputAttribute()
        {
            // Arrange
            DynamicFormsServices dynamicFormServices = new DynamicFormsServices();

            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Password));

            // Act
            DynamicInput result = dynamicFormServices.ConvertToDynamicInput(propertyInfo);

            // Assert
            Assert.Null(result);
        }
    }

    public class DummyFormModel
    {
        [ValidateMatches("Password", nameof(DummyStrings.Dummy), typeof(DummyStrings))]
        [DynamicInput(false, "email", nameof(DummyStrings.Dummy), typeof(DummyStrings), "input", 0, nameof(DummyStrings.Dummy), nameof(DummyStrings.Dummy))]
        public string Email { get; set; }

        public string Password { get; set; }
        
        public string ConfirmPassword { get; set; }
    }
}

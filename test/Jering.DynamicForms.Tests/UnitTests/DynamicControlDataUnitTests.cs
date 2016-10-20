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
    public class DynamicControlDataUnitTests
    {
        [Fact]
        public void FromPropertyInfo_CreatesDynamicControlDataFromPropertyInfo()
        {
            // Arrange
            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Email));

            // Act
            DynamicControlData result = DynamicControlData.FromPropertyInfo(propertyInfo);

            // Assert
            Assert.Equal("Email", result.Name);
            Assert.Equal(0, result.Order);
            Assert.Equal("input", result.TagName);
            Assert.NotNull(result.ValidatorDatas);
            Assert.Equal("validateMatches", result.ValidatorDatas[0].Name);
            Assert.Equal(DummyStrings.Dummy, result.DisplayName);
            Assert.NotNull(result.Properties);
            Assert.Equal(result.Properties["type"], "email");
            Assert.Equal(result.Properties["placeholder"], DummyStrings.Dummy);
        }

        [Fact]
        public void FromPropertyInfo_ReturnsNullForPropertyInfoWithNoDynamicControlAttribute()
        {
            // Arrange
            DynamicFormsServices dynamicFormServices = new DynamicFormsServices();

            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Password));

            // Act
            DynamicControlData result = DynamicControlData.FromPropertyInfo(propertyInfo);

            // Assert
            Assert.Null(result);
        }
    }

    public class DummyFormModel
    {
        [ValidateMatches("Password", nameof(DummyStrings.Dummy), typeof(DummyStrings))]
        [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
        [DynamicControlProperty("type", "email")]
        [DynamicControlProperty("placeholder", propertyValueResourceName: nameof(DummyStrings.Dummy), resourceType: typeof(DummyStrings))]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}

using Jering.DataAnnotations;
using Jering.DynamicForms.Tests.Resources;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DynamicFormsBuilderUnitTests
    {
        [Fact]
        public void BuildDynamicControlResponseModel_CreatesDynamicControlResponseModelFromPropertyInfo()
        {
            // Arrange
            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Email));
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();

            // Act
            DynamicControlResponseModel result = dynamicFormsBuilder.BuildDynamicControlResponseModel(propertyInfo);

            // Assert
            Assert.Equal("Email", result.Name);
            Assert.Equal(0, result.Order);
            Assert.Equal("input", result.TagName);
            Assert.NotNull(result.ValidatorResponseModels);
            Assert.Equal(result.ValidatorResponseModels.Count, 1);
            Assert.Equal("validateMatches", result.ValidatorResponseModels[0].Name);
            Assert.NotNull(result.AsyncValidatorResponseModel);
            Assert.Equal("asyncValidateEmailNotInUse", result.AsyncValidatorResponseModel.Name);
            Assert.Equal(DummyStrings.Dummy, result.DisplayName);
            Assert.NotNull(result.Properties);
            Assert.Equal(result.Properties["type"], "email");
            Assert.Equal(result.Properties["placeholder"], DummyStrings.Dummy);
        }

        [Fact]
        public void BuildDynamicControlResponseModel_ReturnsNullForPropertyInfoWithNoDynamicControlAttribute()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Password));

            // Act
            DynamicControlResponseModel result = dynamicFormsBuilder.BuildDynamicControlResponseModel(propertyInfo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BuildDynamicControlResponseModel_ThrowsArgumentExceptionIfPropertyInfoHasMoreThanOneAsyncValidationAttributes()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.SecondaryEmail));

            // Act and Assert
            Assert.Throws<ArgumentException>(() => dynamicFormsBuilder.BuildDynamicControlResponseModel(propertyInfo));
        }

        [Fact]
        public void BuildDynamicFormResponseModel_CreatesDynamicFormResponseModelFromTypeInfo()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            Mock<DynamicFormsBuilder> mockBuilder = new Mock<DynamicFormsBuilder>();
            mockBuilder.Setup(b => b.BuildDynamicControlResponseModel(It.IsAny<PropertyInfo>())).Returns(new DynamicControlResponseModel());
            mockBuilder.CallBase = true;

            // Act
            DynamicFormResponseModel result = mockBuilder.Object.BuildDynamicFormResponseModel(typeof(DummyFormModel).GetTypeInfo());

            // Assert
            Assert.NotNull(result);
            mockBuilder.VerifyAll();
            Assert.Equal(3, result.DynamicControlResponseModels.Count);
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
            Assert.Equal(DummyStrings.Dummy, result.ButtonText);
        }

        [Fact]
        public void BuildDynamicFormResponseModel_ThrowsArgumentExceptionIfTypeInfoDoesNotHaveADynamicFormAttribute()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => dynamicFormsBuilder.BuildDynamicFormResponseModel(typeof(DummyFormModel_NoDynamicFormAttribute).GetTypeInfo()));
        }

        [Fact]
        public void BuildDynamicControlValidatorResponseModel_CreatesDynamicControlValidatorResponseModelFromValidationAttribute()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            DummyValidationAttribute dummyValidationAttribute = new DummyValidationAttribute("dummyString", 0, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            ValidatorResponseModel result = dynamicFormsBuilder.BuildDynamicControlValidatorResponseModel(dummyValidationAttribute);

            // Assert
            Assert.Equal("dummyValidation", result.Name);
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
            Assert.Equal(2, result.Options.Count);
            Assert.Equal(result.Options["DummyStringProperty"], "dummyString");
            Assert.Equal(result.Options["DummyIntProperty"], "0");
        }

        [Fact]
        public void BuildDynamicControlValidatorResponseModel_CreatesDynamicControlValidatorResponseModelFromAsyncValidationAttribute()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            DummyAsyncValidationAttribute dummyValidationAttribute = new DummyAsyncValidationAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings), "testController", "testAction");

            // Act
            ValidatorResponseModel result = dynamicFormsBuilder.BuildDynamicControlValidatorResponseModel(dummyValidationAttribute);

            // Assert
            Assert.Equal("dummyAsyncValidation", result.Name);
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
            Assert.Equal(1, result.Options.Count);
            Assert.Equal(result.Options[nameof(AsyncValidationAttribute.RelativeUrl)], "test/testAction");
        }

        private class DummyFormModel_NoDynamicFormAttribute
        {

        }

        [DynamicForm(nameof(DummyStrings.Dummy), nameof(DummyStrings.Dummy), typeof(DummyStrings))]
        private class DummyFormModel
        {
            [ValidateMatches("Password", nameof(DummyStrings.Dummy), typeof(DummyStrings))]
            [AsyncValidateEmailNotInUse(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
            [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
            [DynamicControlProperty("type", "email")]
            [DynamicControlProperty("placeholder", propertyValueResourceName: nameof(DummyStrings.Dummy), resourceType: typeof(DummyStrings))]
            public string Email { get; set; }

            [AsyncValidateEmailNotInUse(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
            [DummyAsyncValidationAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
            [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
            public string SecondaryEmail { get; set; }

            public string Password { get; set; }
        }

        private class DummyValidationAttribute : ValidationAttribute
        {
            public string DummyStringProperty { get; set; }
            public int DummyIntProperty { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="resourceName"></param>
            /// <param name="resourceType"></param>
            public DummyValidationAttribute(string dummyStringProperty, int dummyIntProperty, string resourceName, Type resourceType)
            {
                DummyStringProperty = dummyStringProperty;
                DummyIntProperty = dummyIntProperty;
                ErrorMessageResourceName = resourceName;
                ErrorMessageResourceType = resourceType;
            }
        }

        private class DummyAsyncValidationAttribute : AsyncValidationAttribute
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="resourceName"></param>
            /// <param name="resourceType"></param>
            /// <param name="controller"></param>
            /// <param name="action"></param>
            public DummyAsyncValidationAttribute(string resourceName, Type resourceType, string controller, string action) : base(resourceName, resourceType, controller, action)
            {
            }
        }
    }
}

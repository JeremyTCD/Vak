using Jering.DataAnnotations;
using Jering.DynamicForms.Tests.Resources;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DynamicFormBuilderUnitTests
    {
        [Fact]
        public void BuildDynamicControlData_CreatesDynamicControlDataFromPropertyInfo()
        {
            // Arrange
            PropertyInfo propertyInfo = typeof(DummyRequestModel).GetProperty(nameof(DummyRequestModel.Email));
            DynamicFormBuilder dynamicFormsBuilder = new DynamicFormBuilder();

            // Act
            DynamicControlData result = dynamicFormsBuilder.BuildDynamicControlData(propertyInfo);

            // Assert
            Assert.Equal("email", result.Name);
            Assert.Equal(0, result.Order);
            Assert.Equal("input", result.TagName);
            Assert.NotNull(result.ValidatorData);
            Assert.Equal(result.ValidatorData.Count, 1);
            Assert.Equal("validateMatches", result.ValidatorData[0].Name);
            Assert.NotNull(result.AsyncValidatorData);
            Assert.Equal("asyncValidate", result.AsyncValidatorData.Name);
            Assert.Equal(DummyStrings.Dummy, result.DisplayName);
            Assert.NotNull(result.Properties);
            Assert.Equal(result.Properties["type"], "email");
            Assert.Equal(result.Properties["placeholder"], DummyStrings.Dummy);
        }

        [Fact]
        public void BuildDynamicControlData_ReturnsNullForPropertyInfoWithNoDynamicControlAttribute()
        {
            // Arrange
            DynamicFormBuilder dynamicFormsBuilder = new DynamicFormBuilder();
            PropertyInfo propertyInfo = typeof(DummyRequestModel).GetProperty(nameof(DummyRequestModel.Password));

            // Act
            DynamicControlData result = dynamicFormsBuilder.BuildDynamicControlData(propertyInfo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BuildDynamicFormData_CreatesDynamicFormDataFromTypeInfo()
        {
            // Arrange
            DynamicFormBuilder dynamicFormsBuilder = new DynamicFormBuilder();
            Mock<DynamicFormBuilder> mockBuilder = new Mock<DynamicFormBuilder>();
            mockBuilder.Setup(b => b.BuildDynamicControlData(It.IsAny<PropertyInfo>())).Returns(new DynamicControlData());
            mockBuilder.CallBase = true;

            // Act
            DynamicFormData result = mockBuilder.Object.BuildDynamicFormData(typeof(DummyRequestModel).GetTypeInfo());

            // Assert
            Assert.NotNull(result);
            mockBuilder.VerifyAll();
            Assert.Equal(3, result.DynamicControlData.Count);
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
            Assert.Equal(DummyStrings.Dummy, result.ButtonText);
        }

        [Fact]
        public void BuildDynamicFormData_ThrowsArgumentExceptionIfTypeInfoDoesNotHaveADynamicFormAttribute()
        {
            // Arrange
            DynamicFormBuilder dynamicFormsBuilder = new DynamicFormBuilder();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => dynamicFormsBuilder.BuildDynamicFormData(typeof(DummyRequestModel_NoDynamicFormAttribute).GetTypeInfo()));
        }

        [Fact]
        public void BuildDynamicControlValidatorData_CreatesDynamicControlValidatorDataFromValidationAttribute()
        {
            // Arrange
            DynamicFormBuilder dynamicFormsBuilder = new DynamicFormBuilder();
            DummyValidationAttribute dummyValidationAttribute = new DummyValidationAttribute("dummyString", 0, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            ValidatorData result = dynamicFormsBuilder.BuildDynamicControlValidatorData(dummyValidationAttribute);

            // Assert
            Assert.Equal("dummyValidation", result.Name);
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
            Assert.Equal(2, result.Options.Count);
            Assert.Equal(result.Options["DummyStringProperty"], "dummyString");
            Assert.Equal(result.Options["DummyIntProperty"], "0");
        }

        [Fact]
        public void BuildDynamicControlValidatorData_CreatesDynamicControlValidatorDataFromAsyncValidationAttribute()
        {
            // Arrange
            DynamicFormBuilder dynamicFormsBuilder = new DynamicFormBuilder();
            AsyncValidateAttribute dummyValidationAttribute = new AsyncValidateAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings), "testController", "testAction");

            // Act
            ValidatorData result = dynamicFormsBuilder.BuildDynamicControlValidatorData(dummyValidationAttribute);

            // Assert
            Assert.Equal("asyncValidate", result.Name);
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
            Assert.Equal(1, result.Options.Count);
            Assert.Equal(result.Options[nameof(AsyncValidateAttribute.RelativeUrl)], "test/testAction");
        }

        private class DummyRequestModel_NoDynamicFormAttribute
        {

        }

        [DynamicForm(nameof(DummyStrings.Dummy), nameof(DummyStrings.Dummy), typeof(DummyStrings))]
        private class DummyRequestModel
        {
            [ValidateMatches("Password", nameof(DummyStrings.Dummy), typeof(DummyStrings))]
            [AsyncValidate(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
            [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
            [DynamicControlProperty("type", "email")]
            [DynamicControlProperty("placeholder", propertyValueResourceName: nameof(DummyStrings.Dummy), resourceType: typeof(DummyStrings))]
            public string Email { get; set; }

            [AsyncValidate(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
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
    }
}

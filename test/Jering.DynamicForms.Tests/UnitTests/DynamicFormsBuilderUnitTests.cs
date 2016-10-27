using Jering.DynamicForms.Tests.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DynamicFormsBuilderUnitTests
    {
        [Fact]
        public void BuildDynamicControlData_CreatesDynamicControlDataFromPropertyInfo()
        {
            // Arrange
            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Email));
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();

            // Act
            DynamicControlData result = dynamicFormsBuilder.BuildDynamicControlData(propertyInfo);

            // Assert
            Assert.Equal("Email", result.Name);
            Assert.Equal(0, result.Order);
            Assert.Equal("input", result.TagName);
            Assert.NotNull(result.ValidatorData);
            Assert.Equal(result.ValidatorData.Count, 1);
            Assert.Equal("validateMatches", result.ValidatorData[0].Name);
            Assert.NotNull(result.AsyncValidatorData);
            Assert.Equal("asyncValidateEmailNotInUse", result.AsyncValidatorData.Name);
            Assert.Equal(DummyStrings.Dummy, result.DisplayName);
            Assert.NotNull(result.Properties);
            Assert.Equal(result.Properties["type"], "email");
            Assert.Equal(result.Properties["placeholder"], DummyStrings.Dummy);
        }

        [Fact]
        public void BuildDynamicControlData_ReturnsNullForPropertyInfoWithNoDynamicControlAttribute()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            PropertyInfo propertyInfo = typeof(DummyFormModel).GetProperty(nameof(DummyFormModel.Password));

            // Act
            DynamicControlData result = dynamicFormsBuilder.BuildDynamicControlData(propertyInfo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BuildDynamicFormData_CreatesDynamicFormDataFromDynamicFormAttribute()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            DynamicFormAttribute dynamicFormAttribute = new DynamicFormAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            DynamicFormData result = dynamicFormsBuilder.BuildDynamicFormData(dynamicFormAttribute);

            // Assert
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
        }

        [Fact]
        public void BuildDynamicControlValidatorData_CreatesDynamicControlValidatorDataFromValidationAttribute()
        {
            // Arrange
            DynamicFormsBuilder dynamicFormsBuilder = new DynamicFormsBuilder();
            DummyValidationAttribute dummyValidationAttribute = new DummyValidationAttribute("dummyString", 0, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            DynamicControlValidatorData result = dynamicFormsBuilder.BuildDynamicControlValidatorData(dummyValidationAttribute);

            // Assert
            Assert.Equal("dummyValidation", result.Name);
            Assert.Equal(DummyStrings.Dummy, result.ErrorMessage);
            Assert.Equal(2, result.Options.Count);
            Assert.Equal(result.Options["DummyStringProperty"], "dummyString");
            Assert.Equal(result.Options["DummyIntProperty"], "0");
        }
    }

    public class DummyValidationAttribute : ValidationAttribute
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

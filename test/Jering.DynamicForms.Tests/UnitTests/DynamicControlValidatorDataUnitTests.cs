using Jering.DynamicForms;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using System.Reflection;
using Jering.DataAnnotations;
using Jering.DynamicForms.Tests.Resources;
using System.ComponentModel.DataAnnotations;

namespace Jering.DynamicForms.Tests
{
    public class DynamicControlValidatorDataUnitTests
    {
        [Fact]
        public void FromValidationAttribute_CreatesDynamicControlValidatorDataFromValidationAttribute()
        {
            // Arrange
            DummyValidationAttribute dummyValidationAttribute = new DummyValidationAttribute("dummyString", 0, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            DynamicControlValidatorData result = DynamicControlValidatorData.FromValidationAttribute(dummyValidationAttribute);

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
        public string DummyStringProperty {get;set;}
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

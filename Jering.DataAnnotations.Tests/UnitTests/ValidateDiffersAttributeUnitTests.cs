
using Jering.DataAnnotations.Tests.Resources;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateDiffersAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object model, object actualValue)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(model, null, null);
            ValidateDiffersAttribute validateDiffersAttribute = new ValidateDiffersAttribute("ExpectedValue", nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateDiffersAttribute.GetValidationResult(actualValue, validationContext);

            // Assert
            DummyModel dummyModel = model as DummyModel;
            if (dummyModel != null && dummyModel.ExpectedValue != actualValue as string)
            {
                Assert.Null(validationResult);
            }
            else
            {
                Assert.Equal(DummyStrings.Dummy, validationResult.ErrorMessage);
            }
        }

        public static IEnumerable<object[]> IsValidData()
        {
            yield return new object[] { new DummyModelEmpty(), "" };
            yield return new object[] { new DummyModel { ExpectedValue = "differs" }, 0 };
            yield return new object[] { new DummyModel { ExpectedValue = "does not differ" }, "does not differ" };
        }

        private class DummyModelEmpty : IXunitSerializable
        {
            public void Deserialize(IXunitSerializationInfo info) { }

            public void Serialize(IXunitSerializationInfo info) { }
        }

        private class DummyModel : IXunitSerializable
        {
            public string ExpectedValue { get; set; }

            public void Deserialize(IXunitSerializationInfo info)
            {
                ExpectedValue = info.GetValue<string>("ExpectedValue");
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("ExpectedValue", ExpectedValue);
            }
        }
    }
}

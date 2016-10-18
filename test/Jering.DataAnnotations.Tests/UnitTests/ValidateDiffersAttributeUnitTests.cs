using Jering.DataAnnotations.Tests.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateDiffersAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidReturnsNullData))]
        public void IsValid_GetValidationResult_ReturnsNullWhenValuesDifferOrEitherValueIsNull(object model, object actualValue)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(model, null, null);
            ValidateDiffersAttribute validateDiffersAttribute = new ValidateDiffersAttribute("ExpectedValue", nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateDiffersAttribute.GetValidationResult(actualValue, validationContext);

            // Assert
            Assert.Null(validationResult);
        }

        public static IEnumerable<object[]> IsValidReturnsNullData()
        {
            yield return new object[] { new DummyModel { ExpectedValue = null }, null };
            yield return new object[] { new DummyModel { ExpectedValue = "test1" }, "test2" };
        }

        [Theory]
        [MemberData(nameof(IsValidReturnsValidationResultData))]
        public void IsValid_GetValidationResult_ReturnsValidationResultWhenValuesDoNotDiffer(object model, object actualValue)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(model, null, null);
            ValidateDiffersAttribute validateDiffersAttribute = new ValidateDiffersAttribute("ExpectedValue", nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateDiffersAttribute.GetValidationResult(actualValue, validationContext);

            // Assert
            Assert.Equal(DummyStrings.Dummy, validationResult.ErrorMessage);
        }

        public static IEnumerable<object[]> IsValidReturnsValidationResultData()
        {
            yield return new object[] { new DummyModel { ExpectedValue = "does not differ" }, "does not differ" };
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

using Jering.DataAnnotations.Tests.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateAllDigitsAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidReturnsNullData))]
        public void IsValid_GetValidationResult_ReturnsNullWhenValueIsNullOrAnEmptyStringOrContainsOnlyDigits(object testObject)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateAllDigitsAttribute validateAllDigitsAttribute = new ValidateAllDigitsAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateAllDigitsAttribute.GetValidationResult(testObject, validationContext);

            // Assert
            Assert.Null(validationResult);
        }

        public static IEnumerable<object[]> IsValidReturnsNullData()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { "0" };
            yield return new object[] { "12345" };
        }

        [Theory]
        [MemberData(nameof(IsValidReturnsValidationResultData))]
        public void IsValid_GetValidationResult_ReturnsValidationResultWhenValueDoesNotContainOnlyDigits(object testObject)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateAllDigitsAttribute validateAllDigitsAttribute = new ValidateAllDigitsAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateAllDigitsAttribute.GetValidationResult(testObject, validationContext);

            // Assert
            Assert.Equal(DummyStrings.Dummy, validationResult.ErrorMessage);
        }

        public static IEnumerable<object[]> IsValidReturnsValidationResultData()
        {
            yield return new object[] { "-1" };
            yield return new object[] { "1.1" };
            yield return new object[] { "a12345" };
            yield return new object[] { "test" };
        }
    }
}

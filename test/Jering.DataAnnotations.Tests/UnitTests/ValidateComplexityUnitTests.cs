using Jering.DataAnnotations.Tests.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateComplexityAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidReturnsNullData))]
        public void IsValid_GetValidationResult_ReturnsNullWhenStringIsSufficientlyComplexOrNullOrAnEmptyString(object testObject)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateComplexityAttribute validateComplexityAttribute = new ValidateComplexityAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateComplexityAttribute.GetValidationResult(testObject, validationContext);

            // Assert
            Assert.Null(validationResult);
        }

        public static IEnumerable<object[]> IsValidReturnsNullData()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { "aaabbb00" };
            yield return new object[] { "aaAA11@@" };
            yield return new object[] { "1234567890123" };
        }

        [Theory]
        [MemberData(nameof(IsValidReturnsValidationResultData))]
        public void IsValid_GetValidationResult_ReturnsValidationResultWhenStringIsNotSufficientlyComplex(object testObject)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateComplexityAttribute validateComplexityAttribute = new ValidateComplexityAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateComplexityAttribute.GetValidationResult(testObject, validationContext);

            // Assert
            Assert.Equal(DummyStrings.Dummy, validationResult.ErrorMessage);
        }

        public static IEnumerable<object[]> IsValidReturnsValidationResultData()
        {
            yield return new object[] { "aaaaaaaa" };
            yield return new object[] { "@!#$%^&*" };
            yield return new object[] { "123456789012" };
        }
    }
}

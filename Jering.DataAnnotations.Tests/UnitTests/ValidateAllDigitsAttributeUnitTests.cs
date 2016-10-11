using Jering.DataAnnotations.Tests.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateAllDigitsAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object testObject)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateAllDigitsAttribute validateAllDigitsAttribute = new ValidateAllDigitsAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateAllDigitsAttribute.GetValidationResult(testObject, validationContext);

            // Assert
            if (testObject as string == "123456")
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
            yield return new object[] { 0 };
            yield return new object[] { "ab09-+" };
            yield return new object[] { "123456" };
        }
    }
}

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
    public class ValidateLengthAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object testObject)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateLengthAttribute validateLengthAttribute = new ValidateLengthAttribute(6, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateLengthAttribute.GetValidationResult(testObject, validationContext);

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
            yield return new object[] { "1234" };
            yield return new object[] { "123456" };
            yield return new object[] { "12345678" };
        }
    }
}

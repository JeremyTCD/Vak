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
    public class ValidateValueAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object expectedValue, object actualValue)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateValueAttribute validateValueAttribute = new ValidateValueAttribute(expectedValue, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateValueAttribute.GetValidationResult(actualValue, validationContext);

            // Assert
            if (Equals(expectedValue, actualValue))
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
            yield return new object[] { true, true };
            yield return new object[] { true, false };
            yield return new object[] { 0, 0 };
            yield return new object[] { 0, 1 };
            yield return new object[] { "a", "a" };
            yield return new object[] { "a", "b" };
        }
    }
}

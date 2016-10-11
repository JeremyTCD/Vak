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
    public class ValidateRequiredAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object value, bool success)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateRequiredAttribute validateRequiredAttribute = new ValidateRequiredAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateRequiredAttribute.GetValidationResult(value, validationContext);

            // Assert
            if (success)
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
            yield return new object[] { null, false };
            yield return new object[] { "", false };
            yield return new object[] { " ", false };
            yield return new object[] { 0, true };
            yield return new object[] { "test", true };
        }
    }
}

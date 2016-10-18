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
        [MemberData(nameof(IsValidReturnsNullData))]
        public void IsValid_GetValidationResult_ReturnsNullWhenValueHasSpecifiedLengthOrIsNullOrAnEmptyString(object value)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateLengthAttribute validateLengthAttribute = new ValidateLengthAttribute(8, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateLengthAttribute.GetValidationResult(value, validationContext);

            // Assert
            Assert.Null(validationResult);
        }

        public static IEnumerable<object[]> IsValidReturnsNullData()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { "testtest" };
        }

        [Theory]
        [MemberData(nameof(IsValidReturnsValidationResultData))]
        public void IsValid_GetValidationResult_ReturnsValidationResultWhenValueDoesNotHaveSpecifiedLength(object value)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateLengthAttribute validateLengthAttribute = new ValidateLengthAttribute(8, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateLengthAttribute.GetValidationResult(value, validationContext);

            // Assert
            Assert.Equal(DummyStrings.Dummy, validationResult.ErrorMessage);
        }

        public static IEnumerable<object[]> IsValidReturnsValidationResultData()
        {
            yield return new object[] { "test" };
        }
    }
}

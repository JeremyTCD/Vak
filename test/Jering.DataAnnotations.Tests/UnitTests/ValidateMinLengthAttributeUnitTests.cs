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
    public class ValidateMinLengthAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidReturnsNullData))]
        public void IsValid_GetValidationResult_ReturnsNullWhenValueLengthIsGreaterThanOrEqualToSpecifiedMinimumOrValueIsNullOrValueIsAnEmptyString(object value)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateMinLengthAttribute validateMinLengthAttribute = new ValidateMinLengthAttribute(8, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateMinLengthAttribute.GetValidationResult(value, validationContext);

            // Assert
            Assert.Null(validationResult);
        }

        public static IEnumerable<object[]> IsValidReturnsNullData()
        {
            yield return new object[] { null };
            yield return new object[] { "testtest" };
            yield return new object[] { "testtesttest" };
        }

        [Theory]
        [MemberData(nameof(IsValidReturnsValidationResultData))]
        public void IsValid_GetValidationResult_ReturnsValidationResultWhenValueLengthIsLessThanSpecifiedMinimum(object value)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateMinLengthAttribute validateMinLengthAttribute = new ValidateMinLengthAttribute(8, nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateMinLengthAttribute.GetValidationResult(value, validationContext);

            // Assert
            Assert.Equal(DummyStrings.Dummy, validationResult.ErrorMessage);
        }

        public static IEnumerable<object[]> IsValidReturnsValidationResultData()
        {
            yield return new object[] { "test" };
        }
    }
}

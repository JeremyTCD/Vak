using Jering.DataAnnotations.Tests.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateEmailAddressAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidReturnsNullData))]
        public void IsValid_GetValidationResult_ReturnsNullWhenValueIsAnEmailAddressOrNullOrAnEmptyString(object value)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateEmailAddressAttribute validateEmailAddressAttribute = new ValidateEmailAddressAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateEmailAddressAttribute.GetValidationResult(value, validationContext);

            // Assert
            Assert.Null(validationResult); 
        }

        public static IEnumerable<object[]> IsValidReturnsNullData ()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { "test@test.com"};
            yield return new object[] { "test@test" };
        }

        [Theory]
        [MemberData(nameof(IsValidReturnsValidationObjectData))]
        public void IsValid_GetValidationResult_ReturnsValidationObjectWhenValueIsNotAnEmailAddress(object value)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateEmailAddressAttribute validateEmailAddressAttribute = new ValidateEmailAddressAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateEmailAddressAttribute.GetValidationResult(value, validationContext);

            // Assert
            Assert.Equal(DummyStrings.Dummy, validationResult.ErrorMessage);
        }

        public static IEnumerable<object[]> IsValidReturnsValidationObjectData()
        {
            yield return new object[] { "test@" };
            yield return new object[] { "@test" };
            yield return new object[] { "test@test@test" };
            yield return new object[] { "test" };
        }
    }
}

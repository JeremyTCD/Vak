using Jering.DataAnnotations.Tests.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateEmailAddressAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object dummyEmail, string outputString)
        {
            // Arrange
            ValidationContext validationContext = new ValidationContext(new { }, null, null);
            ValidateEmailAddressAttribute validateEmailAddressAttribute = new ValidateEmailAddressAttribute(nameof(DummyStrings.Dummy), typeof(DummyStrings));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateEmailAddressAttribute.GetValidationResult(dummyEmail, validationContext);

            // Assert
            if(dummyEmail as string == "test@domain.com")
            {
                Assert.Null(validationResult);
            }
            else
            {
                Assert.Equal(outputString, validationResult.ErrorMessage);
            }           
        }

        public static IEnumerable<object[]> IsValidData ()
        {
            DummyStrings DummyStrings = new DummyStrings();

            yield return new object[] { "@test", DummyStrings.Dummy };
            yield return new object[] { "test@", DummyStrings.Dummy };
            yield return new object[] { "test", DummyStrings.Dummy };
            yield return new object[] { 0, DummyStrings.Dummy };
            yield return new object[] { "test@domain.com", null };
        }
    }
}

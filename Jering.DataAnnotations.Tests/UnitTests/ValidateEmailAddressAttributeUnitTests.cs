using Jering.DataAnnotations;
using Microsoft.Extensions.Options;
using Moq;
using System;
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
            Mock<IOptions<DummyOptions>> mockOptions = new Mock<IOptions<DummyOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new DummyOptions());

            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<DummyOptions>))).Returns(mockOptions.Object);

            ValidationContext validationContext = new ValidationContext(new { }, mockServiceProvider.Object, null);

            ValidateEmailAddressAttribute validateEmailAddressAttribute = new ValidateEmailAddressAttribute(nameof(DummyOptions.dummyString), typeof(DummyOptions));

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
            DummyOptions DummyOptions = new DummyOptions();

            yield return new object[] { "@test", DummyOptions.dummyString };
            yield return new object[] { "test@", DummyOptions.dummyString };
            yield return new object[] { "test", DummyOptions.dummyString };
            yield return new object[] { 0, DummyOptions.dummyString };
            yield return new object[] { "test@domain.com", null };
        }
    }
}

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
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(string testString)
        {
            // Arrange
            DummyOptions dummyOptions = new DummyOptions();

            Mock<IOptions<DummyOptions>> mockOptions = new Mock<IOptions<DummyOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new DummyOptions());

            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<DummyOptions>))).Returns(mockOptions.Object);

            ValidationContext validationContext = new ValidationContext(new { }, mockServiceProvider.Object, null);

            ValidateMinLengthAttribute validateMinLengthAttribute = new ValidateMinLengthAttribute(8, nameof(dummyOptions.dummyString), typeof(DummyOptions));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateMinLengthAttribute.GetValidationResult(testString, validationContext);

            // Assert
            if (testString == "longEnough")
            {
                Assert.Null(validationResult);
            }
            else
            {
                Assert.Equal(dummyOptions.dummyString, validationResult.ErrorMessage);
            }
        }

        public static IEnumerable<object[]> IsValidData()
        {
            yield return new object[] { 0 };
            yield return new object[] { "short" };
            yield return new object[] { "longEnough" };
        }
    }
}

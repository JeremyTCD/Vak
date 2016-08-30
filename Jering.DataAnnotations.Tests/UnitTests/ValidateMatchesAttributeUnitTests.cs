
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;

namespace Jering.DataAnnotations.Tests.UnitTests
{
    public class ValidateMatchesAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object model, object actualValue)
        {
            // Arrange
            DummyOptions dummyOptions = new DummyOptions();

            Mock<IOptions<DummyOptions>> mockOptions = new Mock<IOptions<DummyOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new DummyOptions());

            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<DummyOptions>))).Returns(mockOptions.Object);

            ValidationContext validationContext = new ValidationContext(model, mockServiceProvider.Object, null);

            ValidateMatchesAttribute validateMatchesAttribute = new ValidateMatchesAttribute("ExpectedValue", nameof(dummyOptions.dummyString), typeof(DummyOptions));

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateMatchesAttribute.GetValidationResult(actualValue, validationContext);

            // Assert
            DummyModel dummyModel = model as DummyModel;
            if (dummyModel != null && dummyModel.ExpectedValue == actualValue as string)
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
            yield return new object[] { new DummyModelEmpty(), "" };
            yield return new object[] { new DummyModel { ExpectedValue = "does not match" }, 0 };
            yield return new object[] { new DummyModel { ExpectedValue = "matches" }, "matches" };
        }

        private class DummyModelEmpty : IXunitSerializable
        {
            public void Deserialize(IXunitSerializationInfo info) { }

            public void Serialize(IXunitSerializationInfo info) { }
        }

        private class DummyModel : IXunitSerializable
        {
            public string ExpectedValue { get; set; }

            public void Deserialize(IXunitSerializationInfo info)
            {
                ExpectedValue = info.GetValue<string>("ExpectedValue");
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("ExpectedValue", ExpectedValue);
            }
        }
    }
}

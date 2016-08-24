using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;

namespace Jering.VectorArtKit.WebApplication.Tests.ViewModels.UnitTests
{
    public class ValidateAsConfirmPasswordAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object model, object confirmPassword)
        {
            // Arrange
            ViewModelOptions viewModelOptions = new ViewModelOptions();

            Mock<IOptions<ViewModelOptions>> mockViewModelOptions = new Mock<IOptions<ViewModelOptions>>();
            mockViewModelOptions.Setup(o => o.Value).Returns(new ViewModelOptions());

            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<ViewModelOptions>))).Returns(mockViewModelOptions.Object);

            ValidationContext validationContext = new ValidationContext(model, mockServiceProvider.Object, null);

            ValidateAsConfirmPasswordAttribute validateAsConfirmPasswordAttribute = new ValidateAsConfirmPasswordAttribute("Password");

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateAsConfirmPasswordAttribute.GetValidationResult(confirmPassword, validationContext);

            // Assert
            DummyModelWithPassword dummyModel = model as DummyModelWithPassword;
            if (dummyModel != null && dummyModel.Password == confirmPassword as string)
            {
                Assert.Null(validationResult);
            }
            else
            {
                Assert.Equal(viewModelOptions.ConfirmPassword_DoesNotMatchPassword, validationResult.ErrorMessage);
            }
        }

        public static IEnumerable<object[]> IsValidData()
        {
            yield return new object[] { new DummyModelEmpty(), "" };
            yield return new object[] { new DummyModelWithPassword { Password = "does not match" }, "" };
            yield return new object[] { new DummyModelWithPassword { Password = "does not match" }, 0 };
            yield return new object[] { new DummyModelWithPassword { Password = "matches" }, "matches" };
        }

        private class DummyModelEmpty : IXunitSerializable
        {
            public void Deserialize(IXunitSerializationInfo info) { }

            public void Serialize(IXunitSerializationInfo info) { }
        }

        private class DummyModelWithPassword : IXunitSerializable
        {
            public string Password { get; set; }

            public void Deserialize(IXunitSerializationInfo info)
            {
                Password = info.GetValue<string>("Password");
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("Password", Password);
            }
        }
    }
}

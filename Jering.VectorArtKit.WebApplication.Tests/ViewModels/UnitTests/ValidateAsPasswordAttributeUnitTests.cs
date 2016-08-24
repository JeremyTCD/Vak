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
using Xunit.Extensions;

namespace Jering.VectorArtKit.WebApplication.Tests.ViewModels.UnitTests
{
    public class ValidateAsPasswordAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResultsIfPasswordIsInvalid(object dummyPassword, ValidatePasswordResult validatePasswordResult, string outputString)
        {
            // Arrange
            Mock<IOptions<ViewModelOptions>> mockViewModelOptions = new Mock<IOptions<ViewModelOptions>>();
            mockViewModelOptions.Setup(o => o.Value).Returns(new ViewModelOptions());

            Mock<IPasswordValidationService> mockPasswordValidationService = new Mock<IPasswordValidationService>();
            mockPasswordValidationService.Setup(p => p.ValidatePassword(It.IsAny<string>())).Returns(validatePasswordResult);

            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IPasswordValidationService))).Returns(mockPasswordValidationService.Object);
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<ViewModelOptions>))).Returns(mockViewModelOptions.Object);

            ValidationContext validationContext = new ValidationContext(dummyPassword, mockServiceProvider.Object, null);

            ValidateAsPasswordAttribute validateAsPasswordAttribute = new ValidateAsPasswordAttribute();

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateAsPasswordAttribute.GetValidationResult(dummyPassword, validationContext);

            // Assert
            Assert.Equal(outputString, validationResult.ErrorMessage);
        }

        public static IEnumerable<object[]> IsValidData ()
        {
            ViewModelOptions viewModelOptions = new ViewModelOptions();

            yield return new object[] { "", ValidatePasswordResult.TooShort, viewModelOptions.Password_TooShort };
            yield return new object[] { "", ValidatePasswordResult.LowercaseRequired, viewModelOptions.Password_LowercaseRequired };
            yield return new object[] { "", ValidatePasswordResult.DigitRequired, viewModelOptions.Password_DigitRequired };
            yield return new object[] { "", ValidatePasswordResult.NonAlphanumericRequired, viewModelOptions.Password_NonAlphaNumericRequired };
            yield return new object[] { "", ValidatePasswordResult.UppercaseRequired, viewModelOptions.Password_UppercaseRequired };
            yield return new object[] { 0, null, viewModelOptions.Password_Error };
        }

        [Fact]
        public void IsValid_GetValidationResult_ReturnsNullIfValidationIsPasswordIsValid()
        {
            // Arrange
            ViewModelOptions viewModelOptions = new ViewModelOptions();

            Mock<IOptions<ViewModelOptions>> mockViewModelOptions = new Mock<IOptions<ViewModelOptions>>();
            mockViewModelOptions.Setup(o => o.Value).Returns(viewModelOptions);

            Mock<IPasswordValidationService> mockPasswordValidationService = new Mock<IPasswordValidationService>();
            mockPasswordValidationService.Setup(p => p.ValidatePassword("")).Returns(ValidatePasswordResult.Valid);

            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IPasswordValidationService))).Returns(mockPasswordValidationService.Object);
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<ViewModelOptions>))).Returns(mockViewModelOptions.Object);

            ValidationContext validationContext = new ValidationContext("", mockServiceProvider.Object, null);

            ValidateAsPasswordAttribute validateAsPasswordAttribute = new ValidateAsPasswordAttribute();

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateAsPasswordAttribute.GetValidationResult("", validationContext);

            // Assert
            Assert.Null(validationResult);
        }
    }
}

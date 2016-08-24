using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Jering.AccountManagement.DatabaseInterface.Dapper;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class PasswordValidationServicesUnitTests
    {
        [Theory]
        [InlineData("x", ValidatePasswordResult.TooShort)]
        [InlineData("xxxxxxxx", ValidatePasswordResult.NonAlphanumericRequired)]
        [InlineData("@xxxxxxx", ValidatePasswordResult.DigitRequired)]
        [InlineData("@XXXXXX0", ValidatePasswordResult.LowercaseRequired)]
        [InlineData("@xxxxxx0", ValidatePasswordResult.UppercaseRequired)]
        [InlineData("@Xxxxxx0", ValidatePasswordResult.Valid)]
        public void ValidatePassword_ReturnsCorrectValidatePasswordResults(string password, ValidatePasswordResult expectedResult)
        {
            // Arrange
            Mock<IOptions<PasswordOptions>> mockOptions = new Mock<IOptions<PasswordOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new PasswordOptions());

            PasswordValidationService passwordValidationService = new PasswordValidationService(mockOptions.Object);

            // Act
            ValidatePasswordResult validatePasswordResult = passwordValidationService.ValidatePassword(password);

            // Assert
            Assert.Equal(expectedResult, validatePasswordResult);
        }
    }
}

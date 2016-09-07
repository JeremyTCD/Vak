using Jering.AccountManagement.Security;
using Jering.AccountManagement.DatabaseInterface;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class TotpTokenServiceUnitTests
    {

        [Fact]
        public async Task GenerateToken_GeneratesTokenTest()
        {
            // Arrange
            Mock<Account> mockAccount = new Mock<Account>();
            mockAccount.SetupGet(a => a.Email).Returns("Email");
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(Guid.Empty);

            TotpTokenService<Account> totpTokenService = new TotpTokenService<Account>();

            // Act
            string token = await totpTokenService.GenerateTokenAsync("", mockAccount.Object);

            // Assert
            Assert.NotEqual(null, token);
            Assert.Equal(6, token.Length);
            int num;
            Assert.True(int.TryParse(token, out num));
            mockAccount.VerifyAll();
        }

        [Fact]
        public async Task ValidateToken_ReturnsTrueIfTokenIsValidTest()
        {
            // Arrange
            TotpTokenService<Account> totpTokenService = new TotpTokenService<Account>();
            Account account = new Account() { Email = "Email", SecurityStamp = Guid.Empty };
            string token = await totpTokenService.GenerateTokenAsync("", account);

            Mock<Account> mockAccount = new Mock<Account>();
            mockAccount.SetupGet(a => a.Email).Returns(account.Email);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(account.SecurityStamp);

            // Act
            bool result = await totpTokenService.ValidateTokenAsync("", token, mockAccount.Object);

            // Assert
            Assert.True(result);
            mockAccount.VerifyAll();
        }

        [Fact]
        public async Task ValidateToken_ReturnsFalseIfTokenIsInvalidTest()
        {
            // Arrange
            TotpTokenService<Account> totpTokenService = new TotpTokenService<Account>();
            Account account = new Account() { Email = "Email", SecurityStamp = Guid.Empty };

            Mock<Account> mockAccount = new Mock<Account>();
            mockAccount.SetupGet(a => a.Email).Returns(account.Email);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(account.SecurityStamp);

            // Act
            string token = await totpTokenService.GenerateTokenAsync("invalid", account);
            bool result = await totpTokenService.ValidateTokenAsync("", token, mockAccount.Object);

            // Assert
            Assert.False(result);
        }
    }
}

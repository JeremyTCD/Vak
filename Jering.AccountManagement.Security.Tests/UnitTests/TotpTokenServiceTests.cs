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
    public class TotpTokenServiceTests
    {

        [Fact]
        public async Task GenerateToken_GeneratesTokenTest()
        {
            // Arrange
            Mock<TestAccount> mockAccount = new Mock<TestAccount>();
            mockAccount.SetupGet(a => a.Email).Returns("Email");
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(Guid.Empty);

            TotpTokenService<TestAccount> totpTokenService = new TotpTokenService<TestAccount>();

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
            TotpTokenService<TestAccount> totpTokenService = new TotpTokenService<TestAccount>();
            TestAccount account = new TestAccount() { Email = "Email", SecurityStamp = Guid.Empty };
            string token = await totpTokenService.GenerateTokenAsync("", account);

            Mock<TestAccount> mockAccount = new Mock<TestAccount>();
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
            TotpTokenService<TestAccount> totpTokenService = new TotpTokenService<TestAccount>();
            TestAccount account = new TestAccount() { Email = "Email", SecurityStamp = Guid.Empty };

            Mock<TestAccount> mockAccount = new Mock<TestAccount>();
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

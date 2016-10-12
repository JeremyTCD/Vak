using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class DataProtectionTokenServiceUnitTests
    {
        [Fact]
        public async Task GenerateToken_GeneratesTokenTest()
        {
            // Arrange
            string keyStoreFolder = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                "DataProtectionTokenServiceTestKeys");

            IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(keyStoreFolder));

            Mock<Account> mockAccount = new Mock<Account>();
            mockAccount.SetupGet(a => a.AccountId).Returns(1);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(Guid.Empty);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();

            DataProtectionTokenService<Account> dataProtectionTokenService = new DataProtectionTokenService<Account>(dataProtectionProvider, mockOptions.Object);

            // Act
            string token = await dataProtectionTokenService.GenerateTokenAsync("", mockAccount.Object);

            // Assert
            Assert.NotEqual(null, token);
            mockAccount.VerifyAll();
        }

        [Fact]
        public async Task ValidateToken_ReturnsTrueIfTokenIsValidTest()
        {
            // Arrange
            string keyStoreFolder = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                "DataProtectionTokenServiceTestKeys");

            IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(keyStoreFolder));

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            AccountSecurityOptions dataProtectionServiceOptions = new AccountSecurityOptions();
            mockOptions.Setup(o => o.Value).Returns(dataProtectionServiceOptions);

            DataProtectionTokenService<Account> dataProtectionTokenService = new DataProtectionTokenService<Account>(dataProtectionProvider, mockOptions.Object);

            Account account = new Account() { AccountId = 1, SecurityStamp = Guid.Empty };
            string token = await dataProtectionTokenService.GenerateTokenAsync("", account);

            Mock<Account> mockAccount = new Mock<Account>();
            mockAccount.SetupGet(a => a.AccountId).Returns(account.AccountId);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(account.SecurityStamp);

            // Act
            bool result = await dataProtectionTokenService.ValidateTokenAsync("", token, mockAccount.Object);

            // Assert
            Assert.True(result);
            mockAccount.VerifyAll();
        }

        [Fact]
        public async Task ValidateToken_ReturnsFalseIfTokenIsInvalidTest()
        {
            // Arrange
            string keyStoreFolder = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                "DataProtectionTokenServiceTestKeys");

            IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(keyStoreFolder));

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            AccountSecurityOptions dataProtectionServiceOptions = new AccountSecurityOptions();
            mockOptions.Setup(o => o.Value).Returns(dataProtectionServiceOptions);

            DataProtectionTokenService<Account> dataProtectionTokenService = new DataProtectionTokenService<Account>(dataProtectionProvider, mockOptions.Object);

            Account account = new Account() { AccountId = 1, SecurityStamp = Guid.Empty };
            Mock<Account> mockAccount = new Mock<Account>();
            mockAccount.SetupGet(a => a.AccountId).Returns(account.AccountId);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(Guid.NewGuid());

            // Act
            string token = await dataProtectionTokenService.GenerateTokenAsync("", account);
            bool result = await dataProtectionTokenService.ValidateTokenAsync("", token, mockAccount.Object);

            // Assert
            Assert.False(result);
            mockAccount.VerifyAll();
        }
    }
}

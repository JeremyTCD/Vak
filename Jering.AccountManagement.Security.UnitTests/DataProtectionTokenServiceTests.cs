using Jering.AccountManagement.Security;
using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Microsoft.Extensions.Options;

namespace Jering.AccountManagement.Security.UnitTests
{
    public class DataProtectionTokenServiceTests
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

            Mock<TestAccount> mockAccount = new Mock<TestAccount>();
            mockAccount.SetupGet(a => a.AccountId).Returns(1);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(Guid.Empty);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();

            DataProtectionTokenService<TestAccount> dataProtectionTokenService = new DataProtectionTokenService<TestAccount>(dataProtectionProvider, mockOptions.Object);

            // Act
            string token = await dataProtectionTokenService.GenerateToken("", mockAccount.Object);

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

            DataProtectionTokenService<TestAccount> dataProtectionTokenService = new DataProtectionTokenService<TestAccount>(dataProtectionProvider, mockOptions.Object);

            TestAccount account = new TestAccount() { AccountId = 1, SecurityStamp = Guid.Empty };
            string token = await dataProtectionTokenService.GenerateToken("", account);

            Mock<TestAccount> mockAccount = new Mock<TestAccount>();
            mockAccount.SetupGet(a => a.AccountId).Returns(account.AccountId);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(account.SecurityStamp);

            // Act
            bool result = await dataProtectionTokenService.ValidateToken("", token, mockAccount.Object);

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

            DataProtectionTokenService<TestAccount> dataProtectionTokenService = new DataProtectionTokenService<TestAccount>(dataProtectionProvider, mockOptions.Object);

            TestAccount account = new TestAccount() { AccountId = 1, SecurityStamp = Guid.Empty };
            Mock<TestAccount> mockAccount = new Mock<TestAccount>();
            mockAccount.SetupGet(a => a.AccountId).Returns(account.AccountId);
            mockAccount.SetupGet(a => a.SecurityStamp).Returns(Guid.NewGuid());

            // Act
            string token = await dataProtectionTokenService.GenerateToken("", account);
            bool result = await dataProtectionTokenService.ValidateToken("", token, mockAccount.Object);

            // Assert
            Assert.False(result);
            mockAccount.VerifyAll();
        }
    }
}

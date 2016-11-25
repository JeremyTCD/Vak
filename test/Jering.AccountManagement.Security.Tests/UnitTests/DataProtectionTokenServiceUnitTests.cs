using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Jering.AccountManagement.DatabaseInterface;
using Jering.Utilities;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class DataProtectionTokenServiceUnitTests
    {
        private string _validPurpose = "validPurpose";
        private string _invalidPurpose = "invalidPurpose";
        private int _testAccountId = 1;
        private Account _testAccount;
        private TimeService _testTimeService = new TimeService();

        public DataProtectionTokenServiceUnitTests()
        {
            _testAccount = new Account()
            {
                AccountId = _testAccountId,
                SecurityStamp = Guid.Empty
            };
        }

        [Fact]
        public void GenerateToken_GeneratesTokenTest()
        {
            // Arrange
            string keyStoreFolder = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                "DataProtectionTokenServiceTestKeys");

            IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(keyStoreFolder));

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();

            DataProtectionTokenService<Account> dataProtectionTokenService = 
                new DataProtectionTokenService<Account>(dataProtectionProvider, mockOptions.Object, _testTimeService);

            // Act
            string token = dataProtectionTokenService.GenerateToken(_validPurpose, _testAccount);

            // Assert
            Assert.NotEqual(null, token);
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultValidIfTokenIsValidTest()
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

            DataProtectionTokenService<Account> dataProtectionTokenService = 
                new DataProtectionTokenService<Account>(dataProtectionProvider, mockOptions.Object, _testTimeService);

            string token = dataProtectionTokenService.GenerateToken(_validPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_validPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Valid);
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultInvalidIfTokenIsInvalidTest()
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

            DataProtectionTokenService<Account> dataProtectionTokenService = 
                new DataProtectionTokenService<Account>(dataProtectionProvider, mockOptions.Object, _testTimeService);

            string token = dataProtectionTokenService.GenerateToken(_invalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_validPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Invalid);
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultExpiredIfTokenIsExpiredTest()
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

            Mock<ITimeService> mockTimeService = new Mock<ITimeService>();
            mockTimeService.SetupSequence(t => t.UtcNow).
                Returns(DateTimeOffset.MinValue).
                Returns(DateTimeOffset.UtcNow);

            DataProtectionTokenService<Account> dataProtectionTokenService =
                new DataProtectionTokenService<Account>(dataProtectionProvider, 
                    mockOptions.Object, 
                    mockTimeService.Object);

            string token = dataProtectionTokenService.GenerateToken(_invalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_validPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Expired);
        }
    }
}

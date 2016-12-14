using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Jering.Accounts.DatabaseInterface;
using Jering.Utilities;

namespace Jering.Security.Tests.UnitTests
{
    public class DataProtectionTokenServiceUnitTests
    {
        private string _testValidPurpose = "testValidPurpose";
        private string _testInvalidPurpose = "testInvalidPurpose";
        private int _testAccountId = 1;
        private Account _testAccount;
        private TimeService _testTimeService = new TimeService();
        private string _keyStoreFolder = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                "DataProtectionTokenServiceTestKeys");
        private IDataProtectionProvider _dataProtectionProvider;
        TokenServiceOptions _tokenServiceOptions = new TokenServiceOptions();

        public DataProtectionTokenServiceUnitTests()
        {
            _testAccount = new Account()
            {
                AccountId = _testAccountId,
                SecurityStamp = Guid.Empty
            };

            _dataProtectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(_keyStoreFolder));
        }

        [Fact]
        public void GenerateToken_GeneratesTokenTest()
        {
            // Arrange
            DataProtectionTokenService<Account> dataProtectionTokenService = 
                new DataProtectionTokenService<Account>(_dataProtectionProvider, 
                CreateTokenServiceOptions().Object, 
                _testTimeService);

            // Act
            string token = dataProtectionTokenService.GenerateToken(_testValidPurpose, _testAccount);

            // Assert
            Assert.NotNull(token);
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultValidIfTokenIsValidTest()
        {
            // Arrange
            DataProtectionTokenService<Account> dataProtectionTokenService = 
                new DataProtectionTokenService<Account>(_dataProtectionProvider, 
                CreateTokenServiceOptions().Object, 
                _testTimeService);

            string token = dataProtectionTokenService.GenerateToken(_testValidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_testValidPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Valid);
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultInvalidIfTokenIsInvalidTest()
        {
            // Arrange
            DataProtectionTokenService<Account> dataProtectionTokenService = 
                new DataProtectionTokenService<Account>(_dataProtectionProvider, 
                CreateTokenServiceOptions().Object, 
                _testTimeService);

            string token = dataProtectionTokenService.GenerateToken(_testInvalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_testValidPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Invalid);
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultExpiredIfTokenIsExpiredTest()
        {
            // Arrange
            Mock<IOptions<TokenServiceOptions>> mockOptions = new Mock<IOptions<TokenServiceOptions>>();
            TokenServiceOptions dataProtectionServiceOptions = new TokenServiceOptions();
            mockOptions.Setup(o => o.Value).Returns(dataProtectionServiceOptions);

            Mock<ITimeService> mockTimeService = new Mock<ITimeService>();
            mockTimeService.SetupSequence(t => t.UtcNow).
                Returns(DateTimeOffset.MinValue).
                Returns(DateTimeOffset.UtcNow);

            DataProtectionTokenService<Account> dataProtectionTokenService =
                new DataProtectionTokenService<Account>(_dataProtectionProvider, 
                    mockOptions.Object, 
                    mockTimeService.Object);

            string token = dataProtectionTokenService.GenerateToken(_testInvalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_testValidPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Expired);
        }

        public Mock<IOptions<TokenServiceOptions>> CreateTokenServiceOptions()
        {
            Mock<IOptions<TokenServiceOptions>> mockOptions = new Mock<IOptions<TokenServiceOptions>>();
            TokenServiceOptions dataProtectionServiceOptions = new TokenServiceOptions();
            mockOptions.Setup(o => o.Value).Returns(dataProtectionServiceOptions);

            return mockOptions;
        }
    }
}

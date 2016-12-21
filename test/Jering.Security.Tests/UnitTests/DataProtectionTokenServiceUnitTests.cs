using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using Xunit;
using Jering.Utilities;
using Jering.Accounts.DatabaseInterface.EfCore;
using Jering.Accounts.DatabaseInterface;

namespace Jering.Security.Tests.UnitTests
{
    public class DataProtectionTokenServiceUnitTests
    {
        private string _testValidPurpose = "testValidPurpose";
        private string _testInvalidPurpose = "testInvalidPurpose";
        private int _testAccountId = 1;
        private StubAccount _testAccount;
        private TimeService _testTimeService = new TimeService();
        private string _keyStoreFolder = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                "DataProtectionTokenServiceTestKeys");
        private IDataProtectionProvider _dataProtectionProvider;
        TokenServiceOptions _tokenServiceOptions = new TokenServiceOptions();

        public DataProtectionTokenServiceUnitTests()
        {
            _testAccount = new StubAccount()
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
            DataProtectionTokenService<StubAccount> dataProtectionTokenService =
                new DataProtectionTokenService<StubAccount>(_dataProtectionProvider,
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
            DataProtectionTokenService<StubAccount> dataProtectionTokenService =
                new DataProtectionTokenService<StubAccount>(_dataProtectionProvider,
                CreateTokenServiceOptions().Object,
                _testTimeService);

            string token = dataProtectionTokenService.GenerateToken(_testValidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_testValidPurpose, token, _testAccount);

            // Assert
            Assert.Equal(ValidateTokenResult.Valid, result);
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultInvalidIfTokenIsInvalidTest()
        {
            // Arrange
            DataProtectionTokenService<StubAccount> dataProtectionTokenService =
                new DataProtectionTokenService<StubAccount>(_dataProtectionProvider,
                CreateTokenServiceOptions().Object,
                _testTimeService);

            string token = dataProtectionTokenService.GenerateToken(_testInvalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_testValidPurpose, token, _testAccount);

            // Assert
            Assert.Equal(ValidateTokenResult.Invalid, result);
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

            DataProtectionTokenService<StubAccount> dataProtectionTokenService =
                new DataProtectionTokenService<StubAccount>(_dataProtectionProvider,
                    mockOptions.Object,
                    mockTimeService.Object);

            string token = dataProtectionTokenService.GenerateToken(_testInvalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = dataProtectionTokenService.ValidateToken(_testValidPurpose, token, _testAccount);

            // Assert
            Assert.Equal(ValidateTokenResult.Expired, result);
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

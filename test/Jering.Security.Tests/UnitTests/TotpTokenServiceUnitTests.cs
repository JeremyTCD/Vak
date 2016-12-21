using Jering.Accounts.DatabaseInterface;
using Jering.Accounts.DatabaseInterface.EfCore;
using System;
using Xunit;

namespace Jering.Security.Tests.UnitTests
{
    public class TotpTokenServiceUnitTests
    {
        private string _validPurpose = "validPurpose";
        private string _invalidPurpose = "invalidPurpose";
        private string _testEmail = "testEmail";
        private StubAccount _testAccount;

        public TotpTokenServiceUnitTests()
        {
            _testAccount = new StubAccount()
            {
                Email = _testEmail,
                SecurityStamp = Guid.Empty
            };
        }

        [Fact]
        public void GenerateToken_GeneratesTokenTest()
        {
            // Arrange
            TotpTokenService<StubAccount> totpTokenService = new TotpTokenService<StubAccount>();

            // Act
            string token = totpTokenService.GenerateToken(_validPurpose, _testAccount);

            // Assert
            Assert.NotEqual(null, token);
            Assert.Equal(6, token.Length);
            int num;
            Assert.True(int.TryParse(token, out num));
        }

        [Fact]
        public void ValidateToken_ReturnsValidateTokenResultValidIfTokenIsValid()
        {
            // Arrange
            TotpTokenService<StubAccount> totpTokenService = new TotpTokenService<StubAccount>();
            string token = totpTokenService.GenerateToken(_validPurpose, _testAccount);

            // Act
            ValidateTokenResult result = totpTokenService.ValidateToken(_validPurpose, token, _testAccount);

            // Assert
            Assert.Equal(ValidateTokenResult.Valid, result);
        }

        [Fact]
        public void ValidateToken_ReturnsFalseIfTokenIsInvalidTest()
        {
            // Arrange
            TotpTokenService<StubAccount> totpTokenService = new TotpTokenService<StubAccount>();
            string token = totpTokenService.GenerateToken(_invalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = totpTokenService.ValidateToken(_validPurpose, token, _testAccount);

            // Assert
            Assert.Equal(ValidateTokenResult.Invalid, result);
        }
    }
}

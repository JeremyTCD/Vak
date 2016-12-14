using Jering.Security;
using Jering.Accounts.DatabaseInterface;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jering.Security.Tests.UnitTests
{
    public class TotpTokenServiceUnitTests
    {
        private string _validPurpose = "validPurpose";
        private string _invalidPurpose = "invalidPurpose";
        private string _testEmail = "testEmail";
        private Account _testAccount;

        public TotpTokenServiceUnitTests()
        {
            _testAccount = new Account()
            {
                Email = _testEmail,
                SecurityStamp = Guid.Empty
            };
        }

        [Fact]
        public void GenerateToken_GeneratesTokenTest()
        {
            // Arrange
            TotpTokenService<Account> totpTokenService = new TotpTokenService<Account>();

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
            TotpTokenService<Account> totpTokenService = new TotpTokenService<Account>();
            string token = totpTokenService.GenerateToken(_validPurpose, _testAccount);

            // Act
            ValidateTokenResult result = totpTokenService.ValidateToken(_validPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Valid);
        }

        [Fact]
        public void ValidateToken_ReturnsFalseIfTokenIsInvalidTest()
        {
            // Arrange
            TotpTokenService<Account> totpTokenService = new TotpTokenService<Account>();
            string token = totpTokenService.GenerateToken(_invalidPurpose, _testAccount);

            // Act
            ValidateTokenResult result = totpTokenService.ValidateToken(_validPurpose, token, _testAccount);

            // Assert
            Assert.True(result.Invalid);
        }
    }
}

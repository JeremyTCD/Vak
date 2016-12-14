using Jering.Accounts.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Jering.Security.Tests.UnitTests
{
    public class ClaimsPrincipalServiceUnitTests
    {
        private string _testAuthScheme = "testAuthScheme";
        private string _testEmail1 = "test@email1";
        private string _testEmail2 = "test@email2";
        private int _testAccountId1 = 1;
        private int _testAccountId2 = 2;
        private int _testClaimId1 = 1;
        private int _testClaimId2 = 2;
        private string _testClaimType1 = "testClaimType1";
        private string _testClaimType2 = "testClaimType2";
        private string _testClaimValue1 = "testClaimValue1";
        private string _testClaimValue2 = "testClaimValue2";
        private int _testRoleId = 1;
        private string _testRoleName = "testRoleName";

        [Fact]
        public async Task CreateClaimsPrincipalAsync_CreatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            Account account = new Account() {AccountId = _testAccountId1, Email = _testEmail1, SecurityStamp = Guid.NewGuid() };
            Role role = new Role() { RoleId = _testRoleId, Name = _testRoleName };
            List<Role> accountRoles = new List<Role>() { role };
            Jering.Accounts.DatabaseInterface.Claim accountClaim = new Jering.Accounts.DatabaseInterface.Claim() { ClaimId = _testClaimId1, Type = _testClaimType1, Value = _testClaimValue1 };
            List<Jering.Accounts.DatabaseInterface.Claim> accountClaims = new List<Jering.Accounts.DatabaseInterface.Claim>() { accountClaim };
            mockAccountRepository.
                Setup(a => a.GetAccountRolesAsync(It.Is<int>(i => i == _testAccountId1))).
                ReturnsAsync(accountRoles);
            mockAccountRepository.
                Setup(a => a.GetAccountClaimsAsync(It.Is<int>(i => i == _testAccountId1))).
                ReturnsAsync(accountClaims);

            Mock<IRoleRepository> mockRoleRepository = new Mock<IRoleRepository>();
            Jering.Accounts.DatabaseInterface.Claim roleClaim = new Jering.Accounts.DatabaseInterface.Claim() { ClaimId = _testClaimId2, Type = _testClaimType2, Value = _testClaimValue2 };
            List<Jering.Accounts.DatabaseInterface.Claim> roleClaims = new List<Jering.Accounts.DatabaseInterface.Claim>() { roleClaim };
            mockRoleRepository.
                Setup(m => m.GetRoleClaimsAsync(It.Is<int>(i => i == _testRoleId))).
                ReturnsAsync(roleClaims);

            Mock<IOptions<ClaimsOptions>> mockOptions = new Mock<IOptions<ClaimsOptions>>();
            ClaimsOptions claimsOptions = new ClaimsOptions();
            mockOptions.
                Setup(o => o.Value).
                Returns(claimsOptions);

            ClaimsPrincipalService<Account> claimsPrincipalService = new ClaimsPrincipalService<Account>(mockAccountRepository.Object, 
                mockRoleRepository.Object,
                mockOptions.Object);

            AuthenticationProperties authProperties = new AuthenticationProperties { IsPersistent = true };

            // Act
            ClaimsPrincipal claimsPrincipal = await claimsPrincipalService.CreateClaimsPrincipalAsync(account,
                _testAuthScheme,
                authProperties);

            // Assert
            ClaimsIdentity claimsIdentity = claimsPrincipal.Identities.First();
            Assert.NotNull(claimsIdentity);
            Assert.Equal(1, claimsPrincipal.Identities.Count());
            Assert.Equal(_testAuthScheme, claimsIdentity.AuthenticationType);
            List<System.Security.Claims.Claim> claims = claimsIdentity.Claims.ToList();
            Assert.NotNull(claims);
            Assert.True(claims.Any(c => c.Type == claimsOptions.AccountIdClaimType && c.Value == _testAccountId1.ToString()));
            Assert.True(claims.Any(c => c.Type == claimsOptions.UsernameClaimType && c.Value == _testEmail1));
            Assert.True(claims.Any(c => c.Type == claimsOptions.SecurityStampClaimType && c.Value == account.SecurityStamp.ToString()));
            Assert.True(claims.Any(c => c.Type == claimsOptions.RoleClaimType && c.Value == _testRoleName));
            Assert.True(claims.Any(c => c.Type == _testClaimType1 && c.Value == _testClaimValue1));
            Assert.True(claims.Any(c => c.Type == _testClaimType2 && c.Value == _testClaimValue2));
            Assert.True(claims.Any(c => c.Type == claimsOptions.IsPersistenClaimType && c.Value == authProperties.IsPersistent.ToString()));
            mockAccountRepository.VerifyAll();
            mockRoleRepository.VerifyAll();
            mockOptions.VerifyAll();
        }

        [Fact]
        public void UpdateClaimsPrincipal_UpdatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<IOptions<ClaimsOptions>> mockOptions = new Mock<IOptions<ClaimsOptions>>();
            ClaimsOptions claimsOptions = new ClaimsOptions();
            mockOptions.Setup(o => o.Value).Returns(claimsOptions);

            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(claimsOptions.AccountIdClaimType, _testAccountId1.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(claimsOptions.UsernameClaimType, _testEmail1));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(claimsOptions.SecurityStampClaimType, Guid.NewGuid().ToString()));
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            Account account = new Account() { AccountId = _testAccountId1, Email = _testEmail2, SecurityStamp = Guid.NewGuid() };

            ClaimsPrincipalService<Account> claimsPrincipalService = new ClaimsPrincipalService<Account>(null, null, mockOptions.Object);

            // Act
            claimsPrincipalService.UpdateClaimsPrincipal(account, claimsPrincipal);

            // Assert
            Assert.Equal(_testEmail2, claimsPrincipal.FindFirst(claimsOptions.UsernameClaimType).Value);
            Assert.Equal(account.SecurityStamp.ToString(), claimsPrincipal.FindFirst(claimsOptions.SecurityStampClaimType).Value);
        }

        [Fact]
        public void UpdateClaimsPrincipal_ThrowsArgumentExceptionIfAccountAndClaimsPrincipalHaveDifferentAccountIdValues()
        {
            // Arrange
            Mock<IOptions<ClaimsOptions>> mockOptions = new Mock<IOptions<ClaimsOptions>>();
            ClaimsOptions claimsOptions = new ClaimsOptions();
            mockOptions.Setup(o => o.Value).Returns(claimsOptions);

            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(claimsOptions.AccountIdClaimType, _testAccountId1.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(claimsOptions.UsernameClaimType, _testEmail1));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(claimsOptions.SecurityStampClaimType, Guid.NewGuid().ToString()));
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            Account account = new Account() { AccountId = _testAccountId2, Email = _testEmail2, SecurityStamp = Guid.NewGuid() };

            ClaimsPrincipalService<Account> claimsPrincipalService = new ClaimsPrincipalService<Account>(null, null, mockOptions.Object);

            // Act and Assert
            Assert.Throws<ArgumentException>(() => claimsPrincipalService.UpdateClaimsPrincipal(account, claimsPrincipal));
        }

        [Fact]
        public void CreateClaimsPrincipal_CreatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<IOptions<ClaimsOptions>> mockOptions = new Mock<IOptions<ClaimsOptions>>();
            ClaimsOptions claimsOptions = new ClaimsOptions();
            mockOptions.Setup(o => o.Value).Returns(claimsOptions);

            ClaimsPrincipalService<Account> claimsPrincipalFactory = new ClaimsPrincipalService<Account>(null, null, mockOptions.Object);

            // Act
            ClaimsPrincipal claimsPrincipal = claimsPrincipalFactory.CreateClaimsPrincipal(_testAccountId1, _testAuthScheme);

            // Assert
            ClaimsIdentity claimsIdentity = claimsPrincipal.Identities.First();
            Assert.NotNull(claimsIdentity);
            Assert.Equal(1, claimsPrincipal.Identities.Count());
            Assert.Equal(_testAuthScheme, claimsIdentity.AuthenticationType);
            List<System.Security.Claims.Claim> claims = claimsIdentity.Claims.ToList();
            Assert.NotNull(claims);
            Assert.True(claims.Any(c => c.Type == claimsOptions.AccountIdClaimType && c.Value == _testAccountId1.ToString()));
            mockOptions.VerifyAll();
        }
    }
}

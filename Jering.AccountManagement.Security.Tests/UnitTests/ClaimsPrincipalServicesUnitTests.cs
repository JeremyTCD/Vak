using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Jering.AccountManagement.DatabaseInterface.Dapper;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class ClaimsPrincipalServicesUnitTests
    {
        [Fact]
        public async Task CreateAccountClaimsPrincipalAsync_CreatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<TestAccountRepository<TestAccount>> mockAccountRepository = new Mock<TestAccountRepository<TestAccount>>(null);
            TestAccount account = new TestAccount() {AccountId = 1, Email = "Email@Jering.com", SecurityStamp = Guid.NewGuid() };
            Role role = new Role() { RoleId = 1, Name = "Name1" };
            List<Role> accountRoles = new List<Role>() { role };
            DatabaseInterface.Claim accountClaim = new DatabaseInterface.Claim() { ClaimId = 1, Type = "Type1", Value = "Value1" };
            List<DatabaseInterface.Claim> accountClaims = new List<DatabaseInterface.Claim>() { accountClaim };
            mockAccountRepository.Setup(a => a.GetAccountRolesAsync(account.AccountId)).ReturnsAsync(accountRoles);
            mockAccountRepository.Setup(a => a.GetAccountClaimsAsync(account.AccountId)).ReturnsAsync(accountClaims);

            Mock<DapperRoleRepository> mockRoleRepository = new Mock<DapperRoleRepository>(null);
            DatabaseInterface.Claim roleClaim = new DatabaseInterface.Claim() { ClaimId = 2, Type = "Type2", Value = "Value2" };
            List<DatabaseInterface.Claim> roleClaims = new List<DatabaseInterface.Claim>() { roleClaim };
            mockRoleRepository.Setup(m => m.GetRoleClaimsAsync(role.RoleId)).ReturnsAsync(roleClaims);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            AccountSecurityOptions securityOptions = new AccountSecurityOptions();
            mockOptions.Setup(o => o.Value).Returns(securityOptions);

            ClaimsPrincipalServices<TestAccount> claimsPrincipalFactory = new ClaimsPrincipalServices<TestAccount>(mockAccountRepository.Object, mockRoleRepository.Object, mockOptions.Object);

            // Act
            ClaimsPrincipal claimsPrincipal = await claimsPrincipalFactory.CreateClaimsPrincipalAsync(account, securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);

            // Assert
            ClaimsIdentity claimsIdentity = claimsPrincipal.Identities.First();
            Assert.NotNull(claimsIdentity);
            Assert.Equal(1, claimsPrincipal.Identities.Count());
            Assert.Equal(securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme, claimsIdentity.AuthenticationType);
            List<System.Security.Claims.Claim> claims = claimsIdentity.Claims.ToList();
            Assert.NotNull(claims);
            Assert.True(claims.Any(c => c.Type == securityOptions.ClaimsOptions.AccountIdClaimType && c.Value == account.AccountId.ToString()));
            Assert.True(claims.Any(c => c.Type == securityOptions.ClaimsOptions.UsernameClaimType && c.Value == account.Email));
            Assert.True(claims.Any(c => c.Type == securityOptions.ClaimsOptions.SecurityStampClaimType && c.Value == account.SecurityStamp.ToString()));
            Assert.True(claims.Any(c => c.Type == securityOptions.ClaimsOptions.RoleClaimType && c.Value == role.Name));
            Assert.True(claims.Any(c => c.Type == roleClaim.Type && c.Value == roleClaim.Value));
            Assert.True(claims.Any(c => c.Type == accountClaim.Type && c.Value == accountClaim.Value));
            mockAccountRepository.VerifyAll();
            mockRoleRepository.VerifyAll();
            mockOptions.VerifyAll();
        }

        [Fact]
        public void CreateAccountIdClaimsPrincipal_CreatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            AccountSecurityOptions securityOptions = new AccountSecurityOptions();
            mockOptions.Setup(o => o.Value).Returns(securityOptions);

            ClaimsPrincipalServices<TestAccount> claimsPrincipalFactory = new ClaimsPrincipalServices<TestAccount>(null, null, mockOptions.Object);

            // Act
            ClaimsPrincipal claimsPrincipal = claimsPrincipalFactory.CreateClaimsPrincipal(0, securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);

            // Assert
            ClaimsIdentity claimsIdentity = claimsPrincipal.Identities.First();
            Assert.NotNull(claimsIdentity);
            Assert.Equal(1, claimsPrincipal.Identities.Count());
            Assert.Equal(securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme, claimsIdentity.AuthenticationType);
            List<System.Security.Claims.Claim> claims = claimsIdentity.Claims.ToList();
            Assert.NotNull(claims);
            Assert.True(claims.Any(c => c.Type == securityOptions.ClaimsOptions.AccountIdClaimType && c.Value == 0.ToString()));
            mockOptions.VerifyAll();
        }
    }
}

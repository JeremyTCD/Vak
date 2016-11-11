using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.DatabaseInterface.Dapper;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class ClaimsPrincipalServiceUnitTests
    {
        [Fact]
        public async Task CreateClaimsPrincipalAsync_CreatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            Account account = new Account() {AccountId = 1, Email = "Email@Jering.com", SecurityStamp = Guid.NewGuid() };
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

            ClaimsPrincipalService<Account> claimsPrincipalService = new ClaimsPrincipalService<Account>(mockAccountRepository.Object, mockRoleRepository.Object, mockOptions.Object);

            AuthenticationProperties authenticationProperties = new AuthenticationProperties { IsPersistent = true };

            // Act
            ClaimsPrincipal claimsPrincipal = await claimsPrincipalService.CreateClaimsPrincipalAsync(account, 
                securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                authenticationProperties);

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
            Assert.True(claims.Any(c => c.Type == securityOptions.ClaimsOptions.IsPersistenClaimType && c.Value == authenticationProperties.IsPersistent.ToString()));
            mockAccountRepository.VerifyAll();
            mockRoleRepository.VerifyAll();
            mockOptions.VerifyAll();
        }

        [Fact]
        public void UpdateClaimsPrincipal_UpdatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            AccountSecurityOptions securityOptions = new AccountSecurityOptions();
            mockOptions.Setup(o => o.Value).Returns(securityOptions);

            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(securityOptions.ClaimsOptions.AccountIdClaimType, "1"));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(securityOptions.ClaimsOptions.UsernameClaimType, "initial@test.com"));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(securityOptions.ClaimsOptions.SecurityStampClaimType, Guid.NewGuid().ToString()));
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            Account account = new Account() { AccountId = 1, Email = "final@test.com", SecurityStamp = Guid.NewGuid() };

            ClaimsPrincipalService<Account> claimsPrincipalService = new ClaimsPrincipalService<Account>(null, null, mockOptions.Object);

            // Act
            claimsPrincipalService.UpdateClaimsPrincipal(account, claimsPrincipal);

            // Assert
            Assert.Equal("final@test.com", claimsPrincipal.FindFirst(securityOptions.ClaimsOptions.UsernameClaimType).Value);
            Assert.Equal(account.SecurityStamp.ToString(), claimsPrincipal.FindFirst(securityOptions.ClaimsOptions.SecurityStampClaimType).Value);
        }

        [Fact]
        public void UpdateClaimsPrincipal_ThrowsArgumentExceptionIfAccountAndClaimsPrincipalHaveDifferentAccountIdValues()
        {
            // Arrange
            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            AccountSecurityOptions securityOptions = new AccountSecurityOptions();
            mockOptions.Setup(o => o.Value).Returns(securityOptions);

            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(securityOptions.ClaimsOptions.AccountIdClaimType, "1"));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(securityOptions.ClaimsOptions.UsernameClaimType, "initial@test.com"));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(securityOptions.ClaimsOptions.SecurityStampClaimType, Guid.NewGuid().ToString()));
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            Account account = new Account() { AccountId = 2, Email = "final@test.com", SecurityStamp = Guid.NewGuid() };

            ClaimsPrincipalService<Account> claimsPrincipalService = new ClaimsPrincipalService<Account>(null, null, mockOptions.Object);

            // Act and Assert
            Assert.Throws<ArgumentException>(() => claimsPrincipalService.UpdateClaimsPrincipal(account, claimsPrincipal));
        }

        [Fact]
        public void CreateClaimsPrincipal_CreatesClaimsPrincipalTest()
        {
            // Arrange
            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            AccountSecurityOptions securityOptions = new AccountSecurityOptions();
            mockOptions.Setup(o => o.Value).Returns(securityOptions);

            ClaimsPrincipalService<Account> claimsPrincipalFactory = new ClaimsPrincipalService<Account>(null, null, mockOptions.Object);

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Jering.Vak.DatabaseInterface;
using Jering.Vak.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Jering.Vak.Authentication.Test
{
    public class ClaimsPrincipalFactoryTests
    {
        [Fact]
        public async Task CreateAsyncTest()
        {
            // Arrange
            Mock<AccountRepository> mockAccountRepository = new Mock<AccountRepository>(null);
            Account account = new Account() {AccountId = 1, Email = "Email@Jering.com", SecurityStamp = "SecurityStamp1" };
            Role role = new Role() { RoleId = 1, Name = "Name1" };
            List<Role> accountRoles = new List<Role>() { role };
            DatabaseInterface.Claim accountClaim = new DatabaseInterface.Claim(1, "Type1", "Value1");
            List<DatabaseInterface.Claim> accountClaims = new List<DatabaseInterface.Claim>() { accountClaim };
            mockAccountRepository.Setup(a => a.GetAccountRolesAsync(account.AccountId)).ReturnsAsync(accountRoles);
            mockAccountRepository.Setup(a => a.GetAccountClaimsAsync(account.AccountId)).ReturnsAsync(accountClaims);

            Mock<RoleRepository> mockRoleRepository = new Mock<RoleRepository>(null);
            DatabaseInterface.Claim roleClaim = new DatabaseInterface.Claim(2, "Type2", "Value2");
            List<DatabaseInterface.Claim> roleClaims = new List<DatabaseInterface.Claim>() { roleClaim };
            mockRoleRepository.Setup(m => m.GetRoleClaimsAsync(role.RoleId)).ReturnsAsync(roleClaims);

            Mock<IOptions<IdentityOptions>> mockOptions = new Mock<IOptions<IdentityOptions>>();
            IdentityOptions identityOptions = new IdentityOptions();
            mockOptions.Setup(o => o.Value).Returns(identityOptions);

            ClaimsPrincipalFactory claimsPrincipalFactory = new ClaimsPrincipalFactory(mockAccountRepository.Object, mockRoleRepository.Object, mockOptions.Object);

            // Act
            ClaimsPrincipal claimsPrincipal = await claimsPrincipalFactory.CreateAsync(account);

            // Assert
            ClaimsIdentity claimsIdentity = claimsPrincipal.Identities.First();
            Assert.NotNull(claimsIdentity);
            Assert.Equal(1, claimsPrincipal.Identities.Count());
            Assert.Equal(identityOptions.Cookies.ApplicationCookieAuthenticationScheme, claimsIdentity.AuthenticationType);
            List<System.Security.Claims.Claim> claims = claimsIdentity.Claims.ToList();
            Assert.NotNull(claims);
            Assert.True(claims.Any(c => c.Type == identityOptions.ClaimsIdentity.UserIdClaimType && c.Value == account.AccountId.ToString()));
            Assert.True(claims.Any(c => c.Type == identityOptions.ClaimsIdentity.UserNameClaimType && c.Value == account.Email));
            Assert.True(claims.Any(c => c.Type == identityOptions.ClaimsIdentity.SecurityStampClaimType && c.Value == account.SecurityStamp));
            Assert.True(claims.Any(c => c.Type == identityOptions.ClaimsIdentity.RoleClaimType && c.Value == role.Name));
            Assert.True(claims.Any(c => c.Type == roleClaim.Type && c.Value == roleClaim.Value));
            Assert.True(claims.Any(c => c.Type == accountClaim.Type && c.Value == accountClaim.Value));
            mockAccountRepository.VerifyAll();
            mockRoleRepository.VerifyAll();
            mockOptions.VerifyAll();
        }
    }
}

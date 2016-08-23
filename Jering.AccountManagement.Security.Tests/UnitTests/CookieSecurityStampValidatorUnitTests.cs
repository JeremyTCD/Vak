using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Jering.AccountManagement.Security.UnitTests.UnitTests
{
    public class CookieSecurityStampValidatorUnitTests
    {
        //[Fact]
        //public async Task ValidateAsync_SignsOutAccountIfSecurityStampIsInvalid()
        //{
        //    // Arrange
        //    Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
        //    mockOptions.Setup(o => o.Value).Returns(new AccountSecurityOptions());

        //    Mock<ClaimsPrincipalServices<Account>> mockClaimsPrincipalServices = new Mock<ClaimsPrincipalServices<Account>>(null, null, null);
        //    mockClaimsPrincipalServices.Setup(c => c.CreateAccount(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>())).Returns(new Account() { SecurityStamp = Guid.NewGuid() });

        //    Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
        //    mockAccountRepository.Setup(a => a.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(new Account() { SecurityStamp = Guid.NewGuid() });

        //    Mock<IAccountSecurityServices<Account>> mockAccountSecurityServices = new Mock<IAccountSecurityServices<Account>>();
        //    mockAccountSecurityServices.Setup(a => a.SignOutAsync()).Verifiable();

        //    // Cannot be mocked
        //    Mock<CookieValidatePrincipalContext> mockContext = new Mock<CookieValidatePrincipalContext>(null, null, null);
        //    mockContext.Setup(c => c.RejectPrincipal()).Verifiable();

        //    CookieSecurityStampValidator<Account> cookieSecurityStampValidator = new CookieSecurityStampValidator<Account>(mockOptions.Object, mockAccountRepository.Object, mockAccountSecurityServices.Object, mockClaimsPrincipalServices.Object);

        //    // Act
        //    await cookieSecurityStampValidator.ValidateAsync(mockContext.Object);

        //    // Assert
        //    mockOptions.VerifyAll();
        //    mockClaimsPrincipalServices.VerifyAll();
        //    mockAccountSecurityServices.VerifyAll();
        //    mockAccountRepository.VerifyAll();
        //    mockContext.VerifyAll();
        //}
    }
}

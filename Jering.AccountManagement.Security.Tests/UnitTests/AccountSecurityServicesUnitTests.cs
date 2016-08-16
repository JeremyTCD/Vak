using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Authentication.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class AccountSecurityServicesUnitTests
    {
        [Fact]
        public async Task ApplicationPasswordSignInAsync_ReturnsApplicationSignInResultFailedIfCredentialsAreInvalid()
        {
            // Arrange
            Mock<IAccountRepository<IAccount>> mockAccountRepository = new Mock<IAccountRepository<IAccount>>();
            mockAccountRepository.Setup(a => a.GetAccountByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);
            AccountSecurityServices<IAccount> accountSecurityServices = new AccountSecurityServices<IAccount>(null, null, null, mockAccountRepository.Object, null);

            // Act
            ApplicationSignInResult applicationSignInResult = await accountSecurityServices.ApplicationPasswordSignInAsync("", "", null);

            // Assert
            Assert.Equal(ApplicationSignInResult.Failed, applicationSignInResult);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task ApplicationPasswordSignInAsync_ReturnsApplicationSignInResultSucceededIfSignInSucceeds()
        {
            // Arrange
            Account account = new Account() { TwoFactorEnabled = false };
            Mock<IAccountRepository<IAccount>> mockAccountRepository = new Mock<IAccountRepository<IAccount>>();
            mockAccountRepository.Setup(a => a.GetAccountByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(account);
            Mock<AccountSecurityServices<IAccount>> mockAccountSecurityServices = new Mock<AccountSecurityServices<IAccount>>(null, null, null, mockAccountRepository.Object, null);
            mockAccountSecurityServices.Setup(a => a.ApplicationPasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), null)).CallBase();

            // Act
            ApplicationSignInResult applicationSignInResult = await mockAccountSecurityServices.Object.ApplicationPasswordSignInAsync("", "", null);

            // Assert
            Assert.Equal(ApplicationSignInResult.Succeeded, applicationSignInResult);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task ApplicationPasswordSignInAsync_ReturnsApplicationSignInResultTwoFactorRequiredIfTwoFactorIsRequired()
        {
            // Arrange
            Account account = new Account() { TwoFactorEnabled = true };
            Mock<IAccountRepository<IAccount>> mockAccountRepository = new Mock<IAccountRepository<IAccount>>();
            mockAccountRepository.Setup(a => a.GetAccountByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(account);
            Mock<AccountSecurityServices<IAccount>> mockAccountSecurityServices = new Mock<AccountSecurityServices<IAccount>>(null, null, null, mockAccountRepository.Object, null);
            mockAccountSecurityServices.Setup(a => a.ApplicationPasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), null)).CallBase();

            // Act
            ApplicationSignInResult applicationSignInResult = await mockAccountSecurityServices.Object.ApplicationPasswordSignInAsync("", "", null);

            // Assert
            Assert.Equal(ApplicationSignInResult.TwoFactorRequired, applicationSignInResult);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsNullIfTwoFactorCookieDoesNotExist()
        {
            // Arrange
            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockAuthenticationManager.Setup(a => a.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(null);

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(a => a.Authentication).Returns(mockAuthenticationManager.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new AccountSecurityOptions());

            AccountSecurityServices<Account> accountSecurityServices = new AccountSecurityServices<Account>(
                null, 
                mockHttpContextAccessor.Object, 
                mockOptions.Object, 
                null, 
                null);

            // Act
            Account account = await accountSecurityServices.GetTwoFactorAccountAsync();

            // Assert
            Assert.Null(account);
            mockHttpContext.VerifyAll();
            mockAuthenticationManager.VerifyAll();
            mockHttpContextAccessor.VerifyAll();
            mockOptions.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsNullIfTwoFactorCookieDoesNotHaveAnAccountIdClaim()
        {
            // Arrange
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();

            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockAuthenticationManager.Setup(a => a.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(claimsPrincipal);

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(a => a.Authentication).Returns(mockAuthenticationManager.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new AccountSecurityOptions());

            AccountSecurityServices<Account> accountSecurityServices = new AccountSecurityServices<Account>(
                null,
                mockHttpContextAccessor.Object,
                mockOptions.Object,
                null,
                null);

            // Act
            Account account = await accountSecurityServices.GetTwoFactorAccountAsync();

            // Assert
            Assert.Null(account);
            mockHttpContext.VerifyAll();
            mockAuthenticationManager.VerifyAll();
            mockHttpContextAccessor.VerifyAll();
            mockOptions.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsAccountIfTwoFactorCookieExistsAndHasAccountIdClaim()
        {
            // Arrange
            Account account = new Account() { AccountId = 1 };

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new AccountSecurityOptions());

            ClaimsPrincipalFactory<IAccount> claimsPrincipalFactory = new ClaimsPrincipalFactory<IAccount>(null, null, mockOptions.Object);
            ClaimsPrincipal claimsPrincipal = await claimsPrincipalFactory.CreateAccountIdClaimsPrincipalAsync(account.AccountId, "");

            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockAuthenticationManager.Setup(a => a.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(claimsPrincipal);

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(a => a.Authentication).Returns(mockAuthenticationManager.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.Setup(a => a.GetAccountAsync(account.AccountId)).ReturnsAsync(account);

            AccountSecurityServices<Account> accountSecurityServices = new AccountSecurityServices<Account>(
                null,
                mockHttpContextAccessor.Object,
                mockOptions.Object,
                mockAccountRepository.Object,
                null);

            // Act
            Account retrievedAccount = await accountSecurityServices.GetTwoFactorAccountAsync();

            // Assert
            Assert.NotNull(retrievedAccount);
            mockHttpContext.VerifyAll();
            mockAuthenticationManager.VerifyAll();
            mockHttpContextAccessor.VerifyAll();
            mockOptions.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorSignInAsync_ReturnsTwoFactorSignInResultFailedIfUnableToRetrieveTwoFactorAccount()
        {
            // Arrange
            Mock<AccountSecurityServices<Account>> mockAccountSecurityServices = new Mock<AccountSecurityServices<Account>>(null, null, null, null, null);
            mockAccountSecurityServices.Setup(a => a.GetTwoFactorAccountAsync()).ReturnsAsync(null);
            mockAccountSecurityServices.CallBase = true;

            // Act
            TwoFactorSignInResult twoFactorSignInResult = await mockAccountSecurityServices.Object.TwoFactorSignInAsync(null, false);

            // Assert
            Assert.Equal(TwoFactorSignInResult.Failed, twoFactorSignInResult);
            mockAccountSecurityServices.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorSignInAsync_ReturnsTwoFactorSignInResultSucceededIfTokenIsValid()
        {
            // Arrange           
            Mock<TotpTokenService<Account>> mockTotpTokenService = new Mock<TotpTokenService<Account>>();
            mockTotpTokenService.Setup(m => m.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Account>())).ReturnsAsync(true);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new AccountSecurityOptions());

            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(a => a.Authentication).Returns(mockAuthenticationManager.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            Mock<AccountSecurityServices<Account>> mockAccountSecurityServices = new Mock<AccountSecurityServices<Account>>(
                null, 
                mockHttpContextAccessor.Object, 
                mockOptions.Object, 
                null, 
                null);
            mockAccountSecurityServices.CallBase = true;
            mockAccountSecurityServices.Setup(a => a.GetTwoFactorAccountAsync()).ReturnsAsync(new Account());
            mockAccountSecurityServices.Setup(a => a.ApplicationSignInAsync(It.IsAny<Account>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);
            mockAccountSecurityServices.Object.RegisterTokenProvider(TokenServiceOptions.TotpTokenService, mockTotpTokenService.Object);

            // Act
            TwoFactorSignInResult twoFactorSignInResult = await mockAccountSecurityServices.Object.TwoFactorSignInAsync("", false);

            // Assert
            Assert.Equal(TwoFactorSignInResult.Succeeded, twoFactorSignInResult);
            mockAccountSecurityServices.VerifyAll();
            mockHttpContextAccessor.VerifyAll();
            mockHttpContext.VerifyAll();
            mockAuthenticationManager.VerifyAll();
            mockOptions.VerifyAll();
            mockTotpTokenService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorSignInAsync_ReturnsTwoFactorSignInResultFailedIfTokenIsInvalid()
        {
            // Arrange           
            Mock<TotpTokenService<Account>> mockTotpTokenService = new Mock<TotpTokenService<Account>>();
            mockTotpTokenService.Setup(m => m.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Account>())).ReturnsAsync(false);

            Mock<AccountSecurityServices<Account>> mockAccountSecurityServices = new Mock<AccountSecurityServices<Account>>(
                null,
                null,
                null,
                null,
                null);
            mockAccountSecurityServices.CallBase = true;
            mockAccountSecurityServices.Setup(a => a.GetTwoFactorAccountAsync()).ReturnsAsync(new Account());
            mockAccountSecurityServices.Object.RegisterTokenProvider(TokenServiceOptions.TotpTokenService, mockTotpTokenService.Object);

            // Act
            TwoFactorSignInResult twoFactorSignInResult = await mockAccountSecurityServices.Object.TwoFactorSignInAsync("", false);

            // Assert
            Assert.Equal(TwoFactorSignInResult.Failed, twoFactorSignInResult);
            mockAccountSecurityServices.VerifyAll();
            mockTotpTokenService.VerifyAll();
        }
    }
}

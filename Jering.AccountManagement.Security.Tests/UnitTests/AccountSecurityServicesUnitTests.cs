using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
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
            PasswordSignInResult<IAccount> applicationSignInResult = await accountSecurityServices.PasswordSignInAsync("", "", null);

            // Assert
            Assert.True(applicationSignInResult.Failed);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task ApplicationPasswordSignInAsync_ReturnsApplicationSignInResultSucceededIfSignInSucceeds()
        {
            // Arrange
            Account account = new Account() { EmailVerified = true, TwoFactorEnabled = false };
            Mock<IAccountRepository<IAccount>> mockAccountRepository = new Mock<IAccountRepository<IAccount>>();
            mockAccountRepository.Setup(a => a.GetAccountByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(account);
            Mock<AccountSecurityServices<IAccount>> mockAccountSecurityServices = new Mock<AccountSecurityServices<IAccount>>(null, null, null, mockAccountRepository.Object, null);
            mockAccountSecurityServices.Setup(a => a.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), null)).CallBase();

            // Act
            PasswordSignInResult<IAccount> applicationSignInResult = await mockAccountSecurityServices.Object.PasswordSignInAsync("", "", null);

            // Assert
            Assert.True(applicationSignInResult.Succeeded);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task ApplicationPasswordSignInAsync_ReturnsApplicationSignInResultTwoFactorRequiredIfTwoFactorIsRequired()
        {
            // Arrange
            Account account = new Account() { EmailVerified = true, TwoFactorEnabled = true };
            Mock<IAccountRepository<IAccount>> mockAccountRepository = new Mock<IAccountRepository<IAccount>>();
            mockAccountRepository.Setup(a => a.GetAccountByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(account);
            Mock<AccountSecurityServices<IAccount>> mockAccountSecurityServices = new Mock<AccountSecurityServices<IAccount>>(null, null, null, mockAccountRepository.Object, null);
            mockAccountSecurityServices.Setup(a => a.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), null)).CallBase();

            // Act
            PasswordSignInResult<IAccount> applicationSignInResult = await mockAccountSecurityServices.Object.PasswordSignInAsync("", "", null);

            // Assert
            Assert.True(applicationSignInResult.TwoFactorRequired);
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

            ClaimsPrincipalServices<IAccount> claimsPrincipalFactory = new ClaimsPrincipalServices<IAccount>(null, null, mockOptions.Object);
            ClaimsPrincipal claimsPrincipal = claimsPrincipalFactory.CreateClaimsPrincipal(account.AccountId, "");

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
            Assert.True(twoFactorSignInResult.Failed);
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
            mockAccountSecurityServices.Setup(a => a.SignInAsync(It.IsAny<Account>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);
            mockAccountSecurityServices.Object.RegisterTokenProvider(TokenServiceOptions.TotpTokenService, mockTotpTokenService.Object);

            // Act
            TwoFactorSignInResult twoFactorSignInResult = await mockAccountSecurityServices.Object.TwoFactorSignInAsync("", false);

            // Assert
            Assert.True(twoFactorSignInResult.Succeeded);
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
            Assert.True(twoFactorSignInResult.Failed);
            mockAccountSecurityServices.VerifyAll();
            mockTotpTokenService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailResultFailedIfUnableToRetrieveEmailConfirmationAccount()
        {
            // Arrange
            Mock<AccountSecurityServices<Account>> mockAccountSecurityService = new Mock<AccountSecurityServices<Account>>(null, null, null, null, null);
            mockAccountSecurityService.Setup(a => a.GetSignedInAccountAsync()).ReturnsAsync(null);
            mockAccountSecurityService.CallBase = true;

            // Act
            ConfirmEmailResult emailConfirmationResult = await mockAccountSecurityService.Object.ConfirmEmailAsync(null);

            // Assert
            Assert.Equal(ConfirmEmailResult.Failed, emailConfirmationResult);
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailResultInvalidTokenIfTokenIsInvalid()
        {
            // Arrange
            Mock<ITokenService<Account>> mockTokenService = new Mock<ITokenService<Account>>();
            mockTokenService.Setup(t => t.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Account>())).ReturnsAsync(false);

            Mock<AccountSecurityServices<Account>> mockAccountSecurityService = new Mock<AccountSecurityServices<Account>>(null, null, null, null, null);
            mockAccountSecurityService.Setup(a => a.GetSignedInAccountAsync()).ReturnsAsync(new Account());
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.Object.RegisterTokenProvider(TokenServiceOptions.DataProtectionTokenService, mockTokenService.Object);

            // Act
            ConfirmEmailResult emailConfirmationResult = await mockAccountSecurityService.Object.ConfirmEmailAsync(null);

            // Assert
            Assert.Equal(ConfirmEmailResult.InvalidToken, emailConfirmationResult);
            mockTokenService.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailFailedIfUnableToUpdateAccountEmailConfirmed()
        {
            // Arrange
            Mock<ITokenService<Account>> mockTokenService = new Mock<ITokenService<Account>>();
            mockTokenService.Setup(t => t.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Account>())).ReturnsAsync(true);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new AccountSecurityOptions());

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.Setup(a => a.UpdateAccountEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(false);

            Mock<AccountSecurityServices<Account>> mockAccountSecurityService = new Mock<AccountSecurityServices<Account>>(null,
                null,
                mockOptions.Object,
                mockAccountRepository.Object,
                null);
            mockAccountSecurityService.Setup(a => a.GetSignedInAccountAsync()).ReturnsAsync(new Account());
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.Object.RegisterTokenProvider(TokenServiceOptions.DataProtectionTokenService, mockTokenService.Object);

            // Act
            ConfirmEmailResult emailConfirmationResult = await mockAccountSecurityService.Object.ConfirmEmailAsync(null);

            // Assert
            Assert.Equal(ConfirmEmailResult.Failed, emailConfirmationResult);
            mockTokenService.VerifyAll();
            mockOptions.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailSucceededIfTokenIsValidAndEmailConfirmedUpdatesSuccessfully()
        {
            // Arrange
            Mock<ITokenService<Account>> mockTokenService = new Mock<ITokenService<Account>>();
            mockTokenService.Setup(t => t.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Account>())).ReturnsAsync(true);

            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new AccountSecurityOptions());

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.Setup(a => a.UpdateAccountEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(true);

            Mock<AccountSecurityServices<Account>> mockAccountSecurityService = new Mock<AccountSecurityServices<Account>>(null,
                null,
                mockOptions.Object,
                mockAccountRepository.Object,
                null);
            mockAccountSecurityService.Setup(a => a.GetSignedInAccountAsync()).ReturnsAsync(new Account());
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.Object.RegisterTokenProvider(TokenServiceOptions.DataProtectionTokenService, mockTokenService.Object);

            // Act
            ConfirmEmailResult emailConfirmationResult = await mockAccountSecurityService.Object.ConfirmEmailAsync(null);

            // Assert
            Assert.Equal(ConfirmEmailResult.Succeeded, emailConfirmationResult);
            mockTokenService.VerifyAll();
            mockOptions.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Theory]
        [MemberData(nameof(UpdateAccountEmailAsyncData))]
        public async Task UpdateAccountEmailAsync_ReturnsCorrectResults(bool updateSuccessful, bool updateThrowsException)
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            if (updateThrowsException)
                mockAccountRepository.Setup(a => a.UpdateAccountEmail(It.IsAny<int>(), It.IsAny<string>())).Throws(GetSqlException(51000));
            else
                mockAccountRepository.Setup(a => a.UpdateAccountEmail(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(updateSuccessful);

            Mock<AccountSecurityServices<Account>> mockAccountSecurityService = new Mock<AccountSecurityServices<Account>>(null,
                null,
                null,
                mockAccountRepository.Object,
                null);
            if (updateSuccessful)
                mockAccountSecurityService.Setup(a => a.RefreshSignInAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
            mockAccountSecurityService.CallBase = true;

            // Act
            UpdateAccountEmailResult result = await mockAccountSecurityService.Object.UpdateAccountEmailAsync(0, "");

            // Assert
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();

            if (updateThrowsException)
                Assert.True(result.EmailInUse);
            else if (updateSuccessful)
                Assert.True(result.Succeeded);
            else
                Assert.True(result.Failed);
        }

        public static IEnumerable<object[]> UpdateAccountEmailAsyncData()
        {
            yield return new object[] { true, false };
            yield return new object[] { false, false };
            yield return new object[] { false, true };
        }

        [Theory]
        [MemberData(nameof(UpdateAccountPasswordHashAsyncData))]
        public async Task UpdateAccountPasswordHashAsync_ReturnsCorrectResults(bool updateSuccessful)
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.Setup(a => a.UpdateAccountPasswordHashAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(updateSuccessful);

            Mock<AccountSecurityServices<Account>> mockAccountSecurityService = new Mock<AccountSecurityServices<Account>>(null,
                null,
                null,
                mockAccountRepository.Object,
                null);
            if (updateSuccessful)
                mockAccountSecurityService.Setup(a => a.RefreshSignInAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
            mockAccountSecurityService.CallBase = true;

            // Act
            UpdateAccountPasswordHashResult result = await mockAccountSecurityService.Object.UpdateAccountPasswordHashAsync(0, "");

            // Assert
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();

            if (updateSuccessful)
                Assert.True(result.Succeeded);
            else
                Assert.True(result.Failed);
        }

        public static IEnumerable<object[]> UpdateAccountPasswordHashAsyncData()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        #region Helpers
        //Modified code from http://blog.jonathanchannon.com/2014/01/02/unit-testing-with-sqlexception/
        private SqlException GetSqlException(int number)
        {
            SqlErrorCollection collection = Construct<SqlErrorCollection>();
            SqlError error = Construct<SqlError>(number, (byte)2, (byte)3, "server name", "error message", "proc", 100, (uint)1, null);

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(collection, new object[] { error });

            MethodInfo createExceptionMethodInfo = typeof(SqlException).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)[0];

            SqlException sqlException = createExceptionMethodInfo.Invoke(null, new object[] { collection, "11.0.0" }) as SqlException;

            return sqlException;
        }

        private T Construct<T>(params object[] parameters)
        {
            ConstructorInfo[] constructors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            return (T)constructors[0].Invoke(parameters);
        }
        #endregion
    }
}

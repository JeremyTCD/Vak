using Jering.AccountManagement.DatabaseInterface;
using Jering.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class AccountSecurityServiceUnitTests : IDisposable
    {
        private AccountSecurityService<Account> _testAccountSecurityService =
            new AccountSecurityService<Account>(null, null, null, null, null, null);

        private const string _testToken = "testToken";
        private const string _testEmail = "testEmail";
        private const string _testEmailSubject = "testEmailSubject";
        private const string _testLinkDomain = "testLinkDomain";
        private const string _testPassword = "testPassword";
        private const int _testAccountId = 1;
        private AccountSecurityOptions _testAccountSecurityOptions;
        private Account _testAccount;

        public AccountSecurityServiceUnitTests()
        {
            _testAccountSecurityOptions = new AccountSecurityOptions();
            _testAccount = new Account();
        }

        [Fact]
        public async Task PasswordLogInAsync_ReturnsPasswordLogInResultCredentialsInvalidIfCredentialsAreInvalid()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailAndPasswordAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<string>(s => s == _testPassword))).
                ReturnsAsync(null);
            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null, null, null, mockAccountRepository.Object, null, null);

            // Act
            PasswordLogInResult<Account> result = await accountSecurityService.PasswordLogInAsync(_testEmail, _testPassword, null);

            // Assert
            Assert.True(result.InvalidCredentials);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task PasswordLogInAsync_ReturnsPasswordLogInResultSucceededIfLogInSucceeds()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            _testAccount.TwoFactorEnabled = false;
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailAndPasswordAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<string>(s => s == _testPassword))).
                ReturnsAsync(_testAccount);
            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null, null, null, mockAccountRepository.Object, null, null);
            mockAccountSecurityService.
                Setup(a => a.LogInAsync(It.Is<Account>(acc => acc == _testAccount), null)).
                Returns(Task.FromResult(0));
            mockAccountSecurityService.CallBase = true;

            // Act
            PasswordLogInResult<Account> result = await mockAccountSecurityService.Object.PasswordLogInAsync(_testEmail, _testPassword, null);

            // Assert
            Assert.True(result.Succeeded);
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task PasswordLogInAsync_ReturnsPasswordLogInResultTwoFactorRequiredIfTwoFactorIsRequired()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            _testAccount.TwoFactorEnabled = true;
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailAndPasswordAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<string>(s => s == _testPassword))).
                ReturnsAsync(_testAccount);
            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null, null, null, mockAccountRepository.Object, null, null);

            // Act
            PasswordLogInResult<Account> result = await accountSecurityService.PasswordLogInAsync(_testEmail, _testPassword, null);

            // Assert
            Assert.True(result.TwoFactorRequired);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsNullIfTwoFactorCookieIsInvalid()
        {
            // Arrange
            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockAuthenticationManager.
                Setup(a => a.AuthenticateAsync(It.Is<string>(s => s == _testAccountSecurityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme))).
                ReturnsAsync(null);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(
                null,
                CreateMockHttpContextAccessor(mockAuthenticationManager).Object,
                CreateMockAccountSecurityOptionsAccessor().Object,
                null,
                null,
                null);

            // Act
            Account account = await accountSecurityService.GetTwoFactorAccountAsync();

            // Assert
            Assert.Null(account);
            mockAuthenticationManager.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsNullIfTwoFactorCookieDoesNotHaveAnAccountIdClaim()
        {
            // Arrange
            Mock<ClaimsPrincipal> mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.
                Setup(c => c.FindFirst(It.Is<string>(s => s == _testAccountSecurityOptions.ClaimsOptions.AccountIdClaimType))).
                Returns<Account>(null);

            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockAuthenticationManager.
                Setup(a => a.AuthenticateAsync(It.IsAny<string>())).
                ReturnsAsync(mockClaimsPrincipal.Object);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(
                null,
                CreateMockHttpContextAccessor(mockAuthenticationManager).Object,
                CreateMockAccountSecurityOptionsAccessor().Object,
                null,
                null,
                null);

            // Act
            Account account = await accountSecurityService.GetTwoFactorAccountAsync();

            // Assert
            Assert.Null(account);
            mockClaimsPrincipal.VerifyAll();
            mockAuthenticationManager.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsAccountIfTwoFactorCookieExistsAndHasAccountIdClaim()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<ClaimsPrincipal> mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.
                Setup(c => c.FindFirst(It.Is<string>(s => s == _testAccountSecurityOptions.ClaimsOptions.AccountIdClaimType))).
                Returns(new System.Security.Claims.Claim("", _testAccount.AccountId.ToString()));

            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockAuthenticationManager.
                Setup(a => a.AuthenticateAsync(It.IsAny<string>())).
                ReturnsAsync(mockClaimsPrincipal.Object);

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountAsync(It.Is<int>(i => i == _testAccount.AccountId))).
                ReturnsAsync(_testAccount);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(
                null,
                CreateMockHttpContextAccessor(mockAuthenticationManager).Object,
                CreateMockAccountSecurityOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            Account retrievedAccount = await accountSecurityService.GetTwoFactorAccountAsync();

            // Assert
            Assert.NotNull(retrievedAccount);
            mockAuthenticationManager.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorLogInAsync_ReturnsTwoFactorLogInResultNotLoggedInIfUnableToRetrieveTwoFactorAccount()
        {
            // Arrange
            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null, null, null, null, null, null);
            mockAccountSecurityService.
                Setup(a => a.GetTwoFactorAccountAsync()).
                ReturnsAsync(null);
            mockAccountSecurityService.CallBase = true;

            // Act
            TwoFactorLogInResult<Account> result = await mockAccountSecurityService.
                Object.
                TwoFactorLogInAsync(_testToken, false);

            // Assert
            Assert.True(result.NotLoggedIn);
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorLogInAsync_ReturnsTwoFactorLogInResultSucceededIfTokenIsValid()
        {
            // Arrange           
            bool testIsPersistent = false;
            Mock<TotpTokenService<Account>> mockTotpTokenService = new Mock<TotpTokenService<Account>>();
            mockTotpTokenService.
                Setup(m => m.ValidateToken(It.Is<string>(s => s == _testAccountSecurityService.TwoFactorTokenPurpose),
                    It.Is<string>(s => s == _testToken),
                    It.Is<Account>(a => a == _testAccount))).
                Returns(ValidateTokenResult.GetValidResult());

            Mock<AuthenticationManager> mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockAuthenticationManager.
                Setup(a => a.SignOutAsync(It.Is<string>(s => s == _testAccountSecurityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme))).
                Returns(Task.FromResult(0));

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(
                null,
                CreateMockHttpContextAccessor(mockAuthenticationManager).Object,
                CreateMockAccountSecurityOptionsAccessor().Object,
                null,
                null,
                null);
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.
                Setup(a => a.LogInAsync(It.Is<Account>(acc => acc == _testAccount),
                    It.Is<AuthenticationProperties>(ap => ap.IsPersistent == testIsPersistent))).
                Returns(Task.CompletedTask);
            mockAccountSecurityService.
                Setup(a => a.GetTwoFactorAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountSecurityService.Object.RegisterTokenProvider(TokenServiceOptions.TotpTokenService, mockTotpTokenService.Object);

            // Act
            TwoFactorLogInResult<Account> twoFactorLogInResult = await mockAccountSecurityService.Object.TwoFactorLogInAsync(_testToken, testIsPersistent);

            // Assert
            Assert.True(twoFactorLogInResult.Succeeded);
            mockAccountSecurityService.VerifyAll();
            mockAuthenticationManager.VerifyAll();
            mockTotpTokenService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorLogInAsync_ReturnsTwoFactorLogInResultInvalidTokenIfTokenIsInvalid()
        {
            // Arrange           
            Mock<TotpTokenService<Account>> mockTotpTokenService = new Mock<TotpTokenService<Account>>();
            mockTotpTokenService.
                Setup(m => m.ValidateToken(It.Is<string>(s => s == _testAccountSecurityService.TwoFactorTokenPurpose),
                    It.Is<string>(s => s == _testToken),
                    It.Is<Account>(a => a == _testAccount))).
                Returns(ValidateTokenResult.GetInvalidResult());

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.
                Setup(a => a.GetTwoFactorAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountSecurityService.
                Object.
                RegisterTokenProvider(TokenServiceOptions.TotpTokenService, mockTotpTokenService.Object);

            // Act
            TwoFactorLogInResult<Account> twoFactorLogInResult = await mockAccountSecurityService.
                Object.
                TwoFactorLogInAsync(_testToken, false);

            // Assert
            Assert.True(twoFactorLogInResult.InvalidToken);
            mockAccountSecurityService.VerifyAll();
            mockTotpTokenService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailResultNotLoggedInIfUnableToRetrieveEmailConfirmationAccount()
        {
            // Arrange
            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null, null, null, null, null, null);
            mockAccountSecurityService
                .Setup(a => a.GetLoggedInAccountAsync())
                .ReturnsAsync(null);
            mockAccountSecurityService.CallBase = true;

            // Act
            ConfirmEmailResult result = await mockAccountSecurityService.Object.ConfirmEmailAsync(_testToken);

            // Assert
            Assert.True(result.NotLoggedIn);
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailResultInvalidTokenIfTokenIsInvalid()
        {
            // Arrange
            Mock<ITokenService<Account>> mockTokenService = new Mock<ITokenService<Account>>();
            mockTokenService.
                Setup(t => t.ValidateToken(It.Is<string>(s => s == _testAccountSecurityService.ConfirmEmailTokenPurpose),
                    It.Is<string>(s => s == _testToken),
                    It.Is<Account>(a => a == _testAccount))).
                Returns(ValidateTokenResult.GetInvalidResult());

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null, null, null, null, null, null);
            mockAccountSecurityService.
                Setup(a => a.GetLoggedInAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.
                Object.
                RegisterTokenProvider(TokenServiceOptions.DataProtectionTokenService, mockTokenService.Object);

            // Act
            ConfirmEmailResult result = await mockAccountSecurityService.
                Object.
                ConfirmEmailAsync(_testToken);

            // Assert
            Assert.True(result.InvalidToken);
            mockTokenService.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailResultExpiredTokenIfTokenIsExpired()
        {
            // Arrange
            Mock<ITokenService<Account>> mockTokenService = new Mock<ITokenService<Account>>();
            mockTokenService.
                Setup(t => t.ValidateToken(It.Is<string>(s => s == _testAccountSecurityService.ConfirmEmailTokenPurpose),
                    It.Is<string>(s => s == _testToken),
                    It.Is<Account>(a => a == _testAccount))).
                Returns(ValidateTokenResult.GetExpiredResult());

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null, null, null, null, null, null);
            mockAccountSecurityService.
                Setup(a => a.GetLoggedInAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.
                Object.
                RegisterTokenProvider(TokenServiceOptions.DataProtectionTokenService, mockTokenService.Object);

            // Act
            ConfirmEmailResult result = await mockAccountSecurityService.
                Object.
                ConfirmEmailAsync(_testToken);

            // Assert
            Assert.True(result.ExpiredToken);
            mockTokenService.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<ITokenService<Account>> mockTokenService = new Mock<ITokenService<Account>>();
            mockTokenService.
                Setup(t => t.ValidateToken(It.Is<string>(s => s == _testAccountSecurityService.ConfirmEmailTokenPurpose),
                    It.Is<string>(s => s == _testToken),
                    It.Is<Account>(a => a == _testAccount))).
                Returns(ValidateTokenResult.GetValidResult());

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountEmailVerifiedAsync(It.Is<int>(i => i == _testAccount.AccountId),
                    It.Is<bool>(b => b == true))).
                ReturnsAsync(false);

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null,
                null,
                CreateMockAccountSecurityOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null);
            mockAccountSecurityService.
                Setup(a => a.GetLoggedInAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.
                Object.
                RegisterTokenProvider(TokenServiceOptions.DataProtectionTokenService, mockTokenService.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountSecurityService.Object.ConfirmEmailAsync(_testToken));

            // Assert
            mockTokenService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsConfirmEmailResultSucceededIfTokenIsValidAndDatabaseUpdateSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<ITokenService<Account>> mockTokenService = new Mock<ITokenService<Account>>();
            mockTokenService.
                Setup(t => t.ValidateToken(It.Is<string>(s => s == _testAccountSecurityService.ConfirmEmailTokenPurpose),
                    It.Is<string>(s => s == _testToken),
                    It.Is<Account>(a => a == _testAccount))).
                Returns(ValidateTokenResult.GetValidResult());

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountEmailVerifiedAsync(It.Is<int>(i => i == _testAccount.AccountId), 
                    It.Is<bool>(b => b == true))).
                ReturnsAsync(true);

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null,
                null,
                CreateMockAccountSecurityOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null);
            mockAccountSecurityService.
                Setup(a => a.GetLoggedInAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountSecurityService.CallBase = true;
            mockAccountSecurityService.
                Object.
                RegisterTokenProvider(TokenServiceOptions.DataProtectionTokenService, mockTokenService.Object);

            // Act
            ConfirmEmailResult result = await mockAccountSecurityService.Object.ConfirmEmailAsync(_testToken);

            // Assert
            Assert.True(result.Succeeded);
            mockTokenService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsResetPasswordResultInvalidEmailIfIsNotAssociatedWithAnyAccount()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailOrAlternativeEmailAsync(It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(null);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            ResetPasswordResult result = await accountSecurityService.ResetPasswordAsync(_testToken, 
                _testEmail, 
                _testPassword);

            // Assert
            Assert.True(result.InvalidEmail);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsResetPasswordResultSucceededIfPasswordResetSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailOrAlternativeEmailAsync(It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(_testAccount);
            mockAccountRepository.
                Setup(a => a.UpdateAccountPasswordHashAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testPassword))).
                ReturnsAsync(true);

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null,
                null,
                CreateMockAccountSecurityOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null);
            mockAccountSecurityService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.ResetPasswordTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.GetValidResult());
            mockAccountSecurityService.CallBase = true;

            // Act
            ResetPasswordResult result = await mockAccountSecurityService.Object.ResetPasswordAsync(_testToken,
                _testEmail,
                _testPassword);

            // Assert
            Assert.True(result.Succeeded);
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsResetPasswordResultInvalidTokenIfTokenIsInvalid()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailOrAlternativeEmailAsync(It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(_testAccount);

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null,
                null,
                CreateMockAccountSecurityOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null);
            mockAccountSecurityService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.ResetPasswordTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.GetInvalidResult());
            mockAccountSecurityService.CallBase = true;

            // Act
            ResetPasswordResult result = await mockAccountSecurityService.Object.ResetPasswordAsync(_testToken,
                _testEmail,
                _testPassword);

            // Assert
            Assert.True(result.InvalidToken);
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsResetPasswordResultExpiredTokenIfTokenIsExpired()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailOrAlternativeEmailAsync(It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(_testAccount);

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null,
                null,
                CreateMockAccountSecurityOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null);
            mockAccountSecurityService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.ResetPasswordTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.GetExpiredResult());
            mockAccountSecurityService.CallBase = true;

            // Act
            ResetPasswordResult result = await mockAccountSecurityService.Object.ResetPasswordAsync(_testToken,
                _testEmail,
                _testPassword);

            // Assert
            Assert.True(result.ExpiredToken);
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountByEmailOrAlternativeEmailAsync(It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(_testAccount);
            mockAccountRepository.
                Setup(a => a.UpdateAccountPasswordHashAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testPassword))).
                ReturnsAsync(false);

            Mock<AccountSecurityService<Account>> mockAccountSecurityService = new Mock<AccountSecurityService<Account>>(null,
                null,
                CreateMockAccountSecurityOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null);
            mockAccountSecurityService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.ResetPasswordTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.GetValidResult());
            mockAccountSecurityService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountSecurityService.Object.ResetPasswordAsync(_testToken,
                _testEmail,
                _testPassword));

            // Assert
            mockAccountRepository.VerifyAll();
            mockAccountSecurityService.VerifyAll();
        }

        [Fact]
        public async Task UpdateEmailAsync_ReturnsUpdateEmailResultInvalidEmailIfNewEmailIsInUse()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                Throws(GetSqlException(51000));

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            UpdateEmailResult result = await accountSecurityService.UpdateEmailAsync(_testAccountId, _testEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.InvalidEmail);
        }

        [Fact]
        public async Task UpdateEmailAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountEmailAsync(It.Is<int>(i => i == _testAccountId), 
                    It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(false);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await accountSecurityService.UpdateEmailAsync(_testAccountId, _testEmail));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateEmailAsync_ReturnsUpdateEmailResultSucceededIfUpdateSucceeds()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(true);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                 null,
                 null,
                 mockAccountRepository.Object,
                 null,
                 null);

            // Act
            UpdateEmailResult result = await accountSecurityService.UpdateEmailAsync(_testAccountId, _testEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task UpdatePasswordHashAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountPasswordHashAsync(It.Is<int>(i => i == _testAccountId), 
                It.Is<string>(s => s == _testPassword))).
                ReturnsAsync(false);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
               null,
               null,
               mockAccountRepository.Object,
               null,
               null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await accountSecurityService.UpdatePasswordHashAsync(_testAccountId, _testPassword));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateAlternativeEmailAsync_ReturnsUpdateAlternativeEmailResultInvalidAlternativeEmailIfNewAlternativeEmailIsInUse()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountAlternativeEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                Throws(GetSqlException(51000));

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            UpdateAlternativeEmailResult result = await accountSecurityService.UpdateAlternativeEmailAsync(_testAccountId, _testEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.InvalidAlternativeEmail);
        }

        [Fact]
        public async Task UpdateAlternativeEmailAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountAlternativeEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(false);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await accountSecurityService.UpdateAlternativeEmailAsync(_testAccountId, _testEmail));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateAlternativeEmailAsync_ReturnsUpdateAlternativeEmailResultSucceededIfUpdateSucceeds()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountAlternativeEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(true);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                 null,
                 null,
                 mockAccountRepository.Object,
                 null,
                 null);

            // Act
            UpdateAlternativeEmailResult result = await accountSecurityService.UpdateAlternativeEmailAsync(_testAccountId, _testEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task UpdateDisplayNameAsync_ReturnsUpdateDisplayNameResultInvalidDisplayNameIfNewDisplayNameIsInUse()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountDisplayNameAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                Throws(GetSqlException(51000));

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            UpdateDisplayNameResult result = await accountSecurityService.UpdateDisplayNameAsync(_testAccountId, _testEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.InvalidDisplayName);
        }

        [Fact]
        public async Task UpdateDisplayNameAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountDisplayNameAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(false);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await accountSecurityService.UpdateDisplayNameAsync(_testAccountId, _testEmail));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateDisplayNameAsync_ReturnsUpdateDisplayNameResultSucceededIfUpdateSucceeds()
        {
            // Arrange
            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountDisplayNameAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testEmail))).
                ReturnsAsync(true);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
                 null,
                 null,
                 mockAccountRepository.Object,
                 null,
                 null);

            // Act
            UpdateDisplayNameResult result = await accountSecurityService.UpdateDisplayNameAsync(_testAccountId, _testEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task UpdateTwoFactorEnabledAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            bool testTwoFactorEnabled = true;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAccountTwoFactorEnabledAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<bool>(s => s == testTwoFactorEnabled))).
                ReturnsAsync(false);

            AccountSecurityService<Account> accountSecurityService = new AccountSecurityService<Account>(null,
               null,
               null,
               mockAccountRepository.Object,
               null,
               null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await accountSecurityService.UpdateTwoFactorEnabledAsync(_testAccountId, testTwoFactorEnabled));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SendResetPasswordEmailAsync_CallsGetTokenAsyncEmailServiceCreateMimeMessageAndEmailServiceSendEmailAsync()
        {
            // Arrange
            string testMessageFormat = "{0} {1} {2}";
            _testAccount.Email = _testEmail;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(testMessageFormat, _testLinkDomain, _testToken, _testEmail))));

            Mock<AccountSecurityService<Account>> mockAccountSecurityServices = new Mock<AccountSecurityService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null);

            mockAccountSecurityServices.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.ResetPasswordTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountSecurityServices.CallBase = true;

            // Act
            await mockAccountSecurityServices.Object.SendResetPasswordEmailAsync(_testAccount, _testEmailSubject, testMessageFormat, _testLinkDomain);

            // Assert
            mockAccountSecurityServices.VerifyAll();
            mockEmailService.VerifyAll();
        }

        [Fact]
        public async Task SendEmailVerificationEmailAsync_CallsGetTokenAsyncEmailServiceCreateMimeMessageAndEmailServiceSendEmailAsync()
        {
            // Arrange
            string testMessageFormat = "{0} {1} {2}";
            _testAccount.Email = _testEmail;
            _testAccount.AccountId = _testAccountId;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(testMessageFormat, _testLinkDomain, _testToken, _testAccountId))));

            Mock<AccountSecurityService<Account>> mockAccountSecurityServices = new Mock<AccountSecurityService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null);

            mockAccountSecurityServices.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.ConfirmEmailTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountSecurityServices.CallBase = true;

            // Act
            await mockAccountSecurityServices.Object.SendEmailVerificationEmailAsync(_testAccount, _testEmailSubject, testMessageFormat, _testLinkDomain);

            // Assert
            mockAccountSecurityServices.VerifyAll();
            mockEmailService.VerifyAll();
        }

        [Fact]
        public async Task SendAlternativeEmailVerificationEmailAsync_CallsGetTokenAsyncEmailServiceCreateMimeMessageAndEmailServiceSendEmailAsync()
        {
            // Arrange
            string testMessageFormat = "{0} {1} {2}";
            _testAccount.Email = _testEmail;
            _testAccount.AccountId = _testAccountId;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(testMessageFormat, _testLinkDomain, _testToken, _testAccountId))));

            Mock<AccountSecurityService<Account>> mockAccountSecurityServices = new Mock<AccountSecurityService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null);

            mockAccountSecurityServices.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.ConfirmAlternativeEmailTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountSecurityServices.CallBase = true;

            // Act
            await mockAccountSecurityServices.Object.SendAlternativeEmailVerificationEmailAsync(_testAccount, _testEmailSubject, testMessageFormat, _testLinkDomain);

            // Assert
            mockAccountSecurityServices.VerifyAll();
            mockEmailService.VerifyAll();
        }

        [Fact]
        public async Task SendTwoFactorCodeEmailAsync_CallsGetTokenAsyncEmailServiceCreateMimeMessageAndEmailServiceSendEmailAsync()
        {
            // Arrange
            string testMessageFormat = "{0}";
            _testAccount.Email = _testEmail;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(_testToken))));

            Mock<AccountSecurityService<Account>> mockAccountSecurityServices = new Mock<AccountSecurityService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null);

            mockAccountSecurityServices.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.TotpTokenService),
                    It.Is<string>(s => s == _testAccountSecurityService.TwoFactorTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountSecurityServices.CallBase = true;

            // Act
            await mockAccountSecurityServices.Object.SendTwoFactorCodeEmailAsync(_testAccount, _testEmailSubject, testMessageFormat);

            // Assert
            mockAccountSecurityServices.VerifyAll();
            mockEmailService.VerifyAll();
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

        public void Dispose()
        {
            // nothing to dispose yet
        }

        private Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(Mock<AuthenticationManager> mockAuthenticationManager)
        {
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.
                Setup(a => a.Authentication).
                Returns(mockAuthenticationManager.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.
                Setup(h => h.HttpContext).
                Returns(mockHttpContext.Object);

            return mockHttpContextAccessor;
        }

        private Mock<IOptions<AccountSecurityOptions>> CreateMockAccountSecurityOptionsAccessor()
        {
            Mock<IOptions<AccountSecurityOptions>> mockOptions = new Mock<IOptions<AccountSecurityOptions>>();
            mockOptions.
                Setup(o => o.Value).
                Returns(_testAccountSecurityOptions);

            return mockOptions;
        }

        #endregion
    }
}

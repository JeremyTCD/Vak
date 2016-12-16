using Jering.Accounts;
using Jering.Accounts.DatabaseInterface;
using Jering.Mail;
using Jering.Security;
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

namespace Jering.Accounts.Tests.UnitTests
{
    public class AccountsServiceUnitTests : IDisposable
    {
        private AccountsService<Account> _testAccountsService =
            new AccountsService<Account>(null, null, null, null, null, null, null);

        private const string _testToken = "testToken";
        private const string _testEmail = "testEmail";
        private const string _testNewEmail = "testNewEmail";
        private const string _testAltEmail = "testAltEmail";
        private const string _testNewAltEmail = "testNewAltEmail";
        private const string _testNewDisplayName = "testNewDisplayName";
        private const bool _testNewTwoFactorEnabled = true;
        private const bool _testNewEmailVerified = true;
        private const bool _testNewAltEmailVerified = true;
        private const string _testEmailSubject = "testEmailSubject";
        private const string _testLinkDomain = "testLinkDomain";
        private const string _testPassword = "testPassword";
        private const string _testPasswordHash = "testPasswordHash";
        private const string _testNewPasswordHash = "testNewPasswordHash";
        private const string _testNewPassword = "testNewPassword";
        private const int _testAccountId = 1;
        private AccountsServiceOptions _testAccountsServiceOptions;
        private Account _testAccount;

        public AccountsServiceUnitTests()
        {
            _testAccountsServiceOptions = new AccountsServiceOptions();
            _testAccount = new Account();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsNullIfTwoFactorCookieIsInvalid()
        {
            // Arrange
            Mock<AuthenticationManager> mockAuthManager = new Mock<AuthenticationManager>();
            mockAuthManager.
                Setup(a => a.AuthenticateAsync(It.Is<string>(s => s == _testAccountsServiceOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme))).
                ReturnsAsync(null);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                CreateMockHttpContextAccessor(mockAuthManager).Object,
                CreateMockAccountsServiceOptionsAccessor().Object,
                null,
                null,
                null,
                null);

            // Act
            Account account = await AccountsService.GetTwoFactorAccountAsync();

            // Assert
            Assert.Null(account);
            mockAuthManager.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsNullIfTwoFactorCookieDoesNotHaveAnAccountIdClaim()
        {
            // Arrange
            Mock<ClaimsPrincipal> mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.
                Setup(c => c.FindFirst(It.Is<string>(s => s == _testAccountsServiceOptions.ClaimsOptions.AccountIdClaimType))).
                Returns<Account>(null);

            Mock<AuthenticationManager> mockAuthManager = new Mock<AuthenticationManager>();
            mockAuthManager.
                Setup(a => a.AuthenticateAsync(It.IsAny<string>())).
                ReturnsAsync(mockClaimsPrincipal.Object);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                CreateMockHttpContextAccessor(mockAuthManager).Object,
                CreateMockAccountsServiceOptionsAccessor().Object,
                null,
                null,
                null,
                null);

            // Act
            Account account = await AccountsService.GetTwoFactorAccountAsync();

            // Assert
            Assert.Null(account);
            mockClaimsPrincipal.VerifyAll();
            mockAuthManager.VerifyAll();
        }

        [Fact]
        public async Task GetTwoFactorAccountAsync_ReturnsAccountIfTwoFactorCookieExistsAndHasAccountIdClaim()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<ClaimsPrincipal> mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.
                Setup(c => c.FindFirst(It.Is<string>(s => s == _testAccountsServiceOptions.ClaimsOptions.AccountIdClaimType))).
                Returns(new System.Security.Claims.Claim("", _testAccount.AccountId.ToString()));

            Mock<AuthenticationManager> mockAuthManager = new Mock<AuthenticationManager>();
            mockAuthManager.
                Setup(a => a.AuthenticateAsync(It.IsAny<string>())).
                ReturnsAsync(mockClaimsPrincipal.Object);

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.GetAccountAsync(It.Is<int>(i => i == _testAccount.AccountId))).
                ReturnsAsync(_testAccount);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                CreateMockHttpContextAccessor(mockAuthManager).Object,
                CreateMockAccountsServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            Account retrievedAccount = await AccountsService.GetTwoFactorAccountAsync();

            // Assert
            Assert.NotNull(retrievedAccount);
            mockAuthManager.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetPasswordAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.PasswordHash = _testPasswordHash;

            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testNewPassword))).
                Returns(false);
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testNewPassword))).
                Returns(_testNewPasswordHash);

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdatePasswordHashAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewPasswordHash))).
                ReturnsAsync(false);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await AccountsService.
                SetPasswordHashAsync(_testAccount, _testNewPassword));

            // Assert
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetPasswordAsync_ReturnsSetPasswordResultSucceededIfPasswordChangeSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.PasswordHash = _testPasswordHash;

            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testNewPassword))).
                Returns(false);
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testNewPassword))).
                Returns(_testNewPasswordHash);

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdatePasswordHashAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewPasswordHash))).
                ReturnsAsync(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);

            // Act
            SetPasswordHashResult result = await AccountsService.
                SetPasswordHashAsync(_testAccount, _testNewPassword);

            // Assert
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SetPasswordAsync_ReturnsSetPasswordResultAlreadySetIfAccountPasswordIsAlreadyEqualToNewPassword()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testNewPassword))).
                Returns(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                mockPasswordService.Object);

            // Act
            SetPasswordHashResult result = await AccountsService.SetPasswordHashAsync(_testAccount, _testNewPassword);

            // Assert
            mockPasswordService.VerifyAll();
            Assert.True(result.AlreadySet);
        }

        [Fact]
        public async Task SetEmailAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.SaveEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewEmail))).
                ReturnsAsync(false);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await AccountsService.
                SetEmailAsync(_testAccount, _testNewEmail));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetEmailAsync_ReturnsSetEmailResultSucceededIfEmailChangeSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.SaveEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewEmail))).
                ReturnsAsync(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetEmailResult result = await AccountsService.
                SetEmailAsync(_testAccount, _testNewEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SetEmailAsync_ReturnsSetEmailResultAlreadySetIfAccountEmailIsAlreadyEqualToNewEmail()
        {
            // Arrange
            _testAccount.Email = _testNewEmail;

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            SetEmailResult result = await AccountsService.SetEmailAsync(_testAccount, _testNewEmail);

            // Assert
            Assert.True(result.AlreadySet);
        }

        [Fact]
        public async Task SetEmailAsync_ReturnsSetEmailResultInvalidNewEmailIfNewEmailIsInUse()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.SaveEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewEmail))).
                Throws(GetSqlException(51000));

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetEmailResult result = await AccountsService.SetEmailAsync(_testAccount, _testNewEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.InvalidNewEmail);
        }

        [Fact]
        public async Task SetAltEmailAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewAltEmail))).
                ReturnsAsync(false);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await AccountsService.
                SetAltEmailAsync(_testAccount, _testNewAltEmail));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailAsync_ReturnsSetAltEmailResultSucceededIfAltEmailChangeSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewAltEmail))).
                ReturnsAsync(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetAltEmailResult result = await AccountsService.
                SetAltEmailAsync(_testAccount, _testNewAltEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SetAltEmailAsync_ReturnsSetAltEmailResultAlreadySetIfAccountAltEmailIsAlreadyEqualToNewAltEmail()
        {
            // Arrange
            _testAccount.AltEmail = _testNewAltEmail;

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            SetAltEmailResult result = await AccountsService.SetAltEmailAsync(_testAccount, _testNewAltEmail);

            // Assert
            Assert.True(result.AlreadySet);
        }

        [Fact]
        public async Task SetAltEmailAsync_ReturnsSetAltEmailResultInvalidNewAltEmailIfNewAltEmailIsInUse()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewAltEmail))).
                Throws(GetSqlException(51000));

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetAltEmailResult result = await AccountsService.SetAltEmailAsync(_testAccount, _testNewAltEmail);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.InvalidNewAltEmail);
        }

        [Fact]
        public async Task SetDisplayNameAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateDisplayNameAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewDisplayName))).
                ReturnsAsync(false);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await AccountsService.
                SetDisplayNameAsync(_testAccount, _testNewDisplayName));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetDisplayNameAsync_ReturnsSetDisplayNameResultSucceededIfDisplayNameChangeSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateDisplayNameAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewDisplayName))).
                ReturnsAsync(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetDisplayNameResult result = await AccountsService.
                SetDisplayNameAsync(_testAccount, _testNewDisplayName);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SetDisplayNameAsync_ReturnsSetDisplayNameResultAlreadySetIfAccountDisplayNameIsAlreadyEqualToNewDisplayName()
        {
            // Arrange
            _testAccount.DisplayName = _testNewDisplayName;

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            SetDisplayNameResult result = await AccountsService.SetDisplayNameAsync(_testAccount, _testNewDisplayName);

            // Assert
            Assert.True(result.AlreadySet);
        }

        [Fact]
        public async Task SetDisplayNameAsync_ReturnsSetDisplayNameResultInvalidNewDisplayNameIfNewDisplayNameIsInUse()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateDisplayNameAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<string>(s => s == _testNewDisplayName))).
                Throws(GetSqlException(51000));

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetDisplayNameResult result = await AccountsService.SetDisplayNameAsync(_testAccount, _testNewDisplayName);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.InvalidNewDisplayName);
        }

        [Fact]
        public async Task SetTwoFactorEnabledAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.EmailVerified = true;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateTwoFactorEnabledAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<bool>(b => b))).
                ReturnsAsync(false);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await AccountsService.
                SetTwoFactorEnabledAsync(_testAccount, _testNewTwoFactorEnabled));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetTwoFactorEnabledAsync_ReturnsSetTwoFactorEnabledResultSucceededIfTwoFactorEnabledChangeSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.EmailVerified = true;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateTwoFactorEnabledAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<bool>(b => b))).
                ReturnsAsync(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetTwoFactorEnabledResult result = await AccountsService.
                SetTwoFactorEnabledAsync(_testAccount, _testNewTwoFactorEnabled);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SetTwoFactorEnabledAsync_ReturnsSetTwoFactorEnabledResultAlreadySetIfAccountTwoFactorEnabledIsAlreadyEqualToNewTwoFactorEnabled()
        {
            // Arrange
            _testAccount.TwoFactorEnabled = _testNewTwoFactorEnabled;

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            SetTwoFactorEnabledResult result = await AccountsService.SetTwoFactorEnabledAsync(_testAccount, _testNewTwoFactorEnabled);

            // Assert
            Assert.True(result.AlreadySet);
        }

        [Fact]
        public async Task SetTwoFactorEnabledAsync_ReturnsSetTwoFactorEnabledResultEmailUnverifiedIfEmailVerifiedIsFalse()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.EmailVerified = false;

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            SetTwoFactorEnabledResult result = await AccountsService.
                SetTwoFactorEnabledAsync(_testAccount, _testNewTwoFactorEnabled);

            // Assert
            Assert.True(result.EmailUnverified);
        }

        [Fact]
        public async Task SetEmailVerifiedAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.EmailVerified = false;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailVerifiedAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<bool>(b => b))).
                ReturnsAsync(false);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await AccountsService.
                SetEmailVerifiedAsync(_testAccount, _testNewEmailVerified));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetEmailVerifiedAsync_ReturnsSetEmailVerifiedResultSucceededIfEmailVerifiedChangeSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.EmailVerified = false;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailVerifiedAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<bool>(b => b))).
                ReturnsAsync(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetEmailVerifiedResult result = await AccountsService.
                SetEmailVerifiedAsync(_testAccount, _testNewEmailVerified);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SetEmailVerifiedAsync_ReturnsSetEmailVerifiedResultAlreadySetIfAccountEmailVerifiedIsAlreadyEqualToNewEmailVerified()
        {
            // Arrange
            _testAccount.EmailVerified = _testNewEmailVerified;

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            SetEmailVerifiedResult result = await AccountsService.SetEmailVerifiedAsync(_testAccount, _testNewEmailVerified);

            // Assert
            Assert.True(result.AlreadySet);
        }

        [Fact]
        public async Task SetAltEmailVerifiedAsync_ThrowsExceptionIfDatabaseUpdateFailsUnexpectedly()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.AltEmailVerified = false;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailVerifiedAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<bool>(b => b))).
                ReturnsAsync(false);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await AccountsService.
                SetAltEmailVerifiedAsync(_testAccount, _testNewAltEmailVerified));

            // Assert
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailVerifiedAsync_ReturnsSetAltEmailVerifiedResultSucceededIfAltEmailVerifiedChangeSucceeds()
        {
            // Arrange
            _testAccount.AccountId = _testAccountId;
            _testAccount.AltEmailVerified = false;

            Mock<IAccountRepository<Account>> mockAccountRepository = new Mock<IAccountRepository<Account>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailVerifiedAsync(It.Is<int>(i => i == _testAccountId),
                    It.Is<bool>(b => b))).
                ReturnsAsync(true);

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SetAltEmailVerifiedResult result = await AccountsService.
                SetAltEmailVerifiedAsync(_testAccount, _testNewAltEmailVerified);

            // Assert
            mockAccountRepository.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SetAltEmailVerifiedAsync_ReturnsSetAltEmailVerifiedResultAlreadySetIfAccountAltEmailVerifiedIsAlreadyEqualToNewAltEmailVerified()
        {
            // Arrange
            _testAccount.AltEmailVerified = _testNewAltEmailVerified;

            AccountsService<Account> AccountsService = new AccountsService<Account>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            SetAltEmailVerifiedResult result = await AccountsService.SetAltEmailVerifiedAsync(_testAccount, _testNewAltEmailVerified);

            // Assert
            Assert.True(result.AlreadySet);
        }

        [Fact]
        public async Task SendResetPasswordEmailAsync()
        {
            // Arrange
            string testMessageFormat = "{0} {1} {2}";
            _testAccount.Email = _testEmail;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testAltEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(testMessageFormat, _testLinkDomain, _testToken, _testAltEmail))));

            Mock<AccountsService<Account>> mockAccountsService = new Mock<AccountsService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null,
                null);

            mockAccountsService.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountsService.ResetPasswordTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountsService.CallBase = true;

            // Act
            await mockAccountsService.Object.SendResetPasswordEmailAsync(_testAccount, _testAltEmail, _testEmailSubject, testMessageFormat, _testLinkDomain);

            // Assert
            mockAccountsService.VerifyAll();
            mockEmailService.VerifyAll();
        }

        [Fact]
        public async Task SendEmailVerificationEmailAsync()
        {
            // Arrange
            string testMessageFormat = "{0} {1} {2}";
            _testAccount.Email = _testEmail;
            _testAccount.AccountId = _testAccountId;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(testMessageFormat, _testLinkDomain, _testToken, _testAccountId))));

            Mock<AccountsService<Account>> mockAccountsService = new Mock<AccountsService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null,
                null);

            mockAccountsService.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountsService.ConfirmEmailTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountsService.CallBase = true;

            // Act
            await mockAccountsService.Object.SendEmailVerificationEmailAsync(_testAccount, _testEmailSubject, testMessageFormat, _testLinkDomain);

            // Assert
            mockAccountsService.VerifyAll();
            mockEmailService.VerifyAll();
        }

        [Fact]
        public async Task SendAltEmailVerificationEmailAsync_ReturnsSendAltEmailVerificationResultSucceededIfEmailIsSentSuccessfully()
        {
            // Arrange
            string testMessageFormat = "{0} {1} {2}";
            _testAccount.AltEmail = _testAltEmail;
            _testAccount.AccountId = _testAccountId;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testAltEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(testMessageFormat, _testLinkDomain, _testToken, _testAccountId))));

            Mock<AccountsService<Account>> mockAccountsService = new Mock<AccountsService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null,
                null);

            mockAccountsService.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testAccountsService.ConfirmAltEmailTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountsService.CallBase = true;

            // Act
            SendAltEmailVerificationEmailResult result = await mockAccountsService.Object.SendAltEmailVerificationEmailAsync(_testAccount, _testEmailSubject, testMessageFormat, _testLinkDomain);

            // Assert
            mockAccountsService.VerifyAll();
            mockEmailService.VerifyAll();
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SendAltEmailVerificationEmailAsync_ReturnsSendAltEmailVerificationResultInvalidAltEmailIfAccountAltEmailIsNullOrEmpty()
        {
            // Arrange
            string testMessageFormat = "{0} {1} {2}";
            _testAccount.AccountId = _testAccountId;

            AccountsService<Account> accountsService = new AccountsService<Account>(null, null, null, null, null, null, null);

            // Act
            SendAltEmailVerificationEmailResult result = await accountsService.
                SendAltEmailVerificationEmailAsync(_testAccount, _testEmailSubject, testMessageFormat, _testLinkDomain);

            // Assert
            Assert.True(result.InvalidAltEmail);
        }

        [Fact]
        public async Task SendTwoFactorCodeEmailAsync()
        {
            // Arrange
            string testMessageFormat = "{0}";
            _testAccount.Email = _testEmail;

            Mock<IEmailService> mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.CreateMimeMessage(It.Is<string>(s => s == _testEmail),
                It.Is<string>(s => s == _testEmailSubject),
                It.Is<string>(s => s == string.Format(_testToken))));

            Mock<AccountsService<Account>> mockAccountsService = new Mock<AccountsService<Account>>(null,
                null,
                null,
                null,
                mockEmailService.Object,
                null,
                null);

            mockAccountsService.
                Setup(a => a.GetToken(It.Is<string>(s => s == TokenServiceOptions.TotpTokenService),
                    It.Is<string>(s => s == _testAccountsService.TwoFactorTokenPurpose),
                    It.Is<Account>(acc => acc == _testAccount))).
                Returns(_testToken);
            mockAccountsService.CallBase = true;

            // Act
            await mockAccountsService.Object.SendTwoFactorCodeEmailAsync(_testAccount, _testEmailSubject, testMessageFormat);

            // Assert
            mockAccountsService.VerifyAll();
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

        private Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(Mock<AuthenticationManager> mockAuthManager)
        {
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.
                Setup(a => a.Authentication).
                Returns(mockAuthManager.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.
                Setup(h => h.HttpContext).
                Returns(mockHttpContext.Object);

            return mockHttpContextAccessor;
        }

        private Mock<IOptions<AccountsServiceOptions>> CreateMockAccountsServiceOptionsAccessor()
        {
            Mock<IOptions<AccountsServiceOptions>> mockOptions = new Mock<IOptions<AccountsServiceOptions>>();
            mockOptions.
                Setup(o => o.Value).
                Returns(_testAccountsServiceOptions);

            return mockOptions;
        }

        #endregion
    }
}

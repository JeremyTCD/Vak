using Jering.Accounts;
using Jering.Accounts.DatabaseInterface;
using Jering.Mail;
using Jering.Security;
using Jering.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jering.Accounts.Tests.UnitTests
{
    public class AccountServiceUnitTests 
    {
        private const string _testToken = "testToken";
        private const string _testEmail = "testEmail";
        private const string _testNewEmail = "testNewEmail";
        private const string _testAltEmail = "testAltEmail";
        private const string _testNewAltEmail = "testNewAltEmail";
        private const string _testNewDisplayName = "testNewDisplayName";
        private const bool _testNewTwoFactorEnabled = true;
        private const string _testPassword = "testPassword";
        private const string _testPasswordHash = "testPasswordHash";
        private const string _testNewPassword = "testNewPassword";
        private const string _testTokenPurpose = "testTokenPurpose";
        private AccountServiceOptions _testAccountServiceOptions;
        private StubAccount _testAccount;

        public AccountServiceUnitTests()
        {
            _testAccountServiceOptions = new AccountServiceOptions();
            _testAccount = new StubAccount();
        }

        #region Session
        [Fact]
        public async Task LogInActionAsync_ReturnsLogInActionResultSuccessIfLogInIsSuccessful()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            _testAccount.TwoFactorEnabled = false;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.ApplicationLogInAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<AuthenticationProperties>(ap => ap.IsPersistent == true))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            LogInActionResult result = await mockAccountService.Object.LogInActionAsync(_testEmail, _testPassword, true);

            // Assert
            Assert.Equal(LogInActionResult.Success, result);
            mockAccountRepository.VerifyAll();
            mockAccountService.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task LogInActionAsync_ReturnsLogInActionResultTwoFactorRequiredIfTwoFactorIsRequired()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            _testAccount.TwoFactorEnabled = true;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.TwoFactorLogInAsync(It.Is<StubAccount>(acc => acc == _testAccount))).
                Returns(Task.CompletedTask);
            mockAccountService.
                Setup(a => a.SendTwoFactorCodeEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            LogInActionResult result = await mockAccountService.Object.LogInActionAsync(_testEmail, _testPassword, true);

            // Assert
            Assert.Equal(LogInActionResult.TwoFactorRequired, result);
            mockAccountRepository.VerifyAll();
            mockAccountService.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task LogInActionAsync_ReturnsLogInActionResultInvalidPasswordIfPasswordIsInvalid()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);

            AccountService<StubAccount> accountService = new AccountService<StubAccount>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);

            // Act
            LogInActionResult result = await accountService.LogInActionAsync(_testEmail, _testPassword, true);

            // Assert
            Assert.Equal(LogInActionResult.InvalidPassword, result);
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task LogInActionAsync_ReturnsLogInActionResultInvalidEmailIfEmailIsInvalid()
        {
            // Arrange
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(null);

            AccountService<StubAccount> accountService = new AccountService<StubAccount>(null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            LogInActionResult result = await accountService.LogInActionAsync(_testEmail, _testPassword, true);

            // Assert
            Assert.Equal(LogInActionResult.InvalidEmail, result);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorLogInActionAsync_ReturnsTwoFactorLogInActionResultSuccessIfTwoFactorLogInIsSuccessful()
        {
            // Arrange
            _testAccountServiceOptions.TwoFactorTokenPurpose = _testTokenPurpose;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetTwoFactorAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.TotpTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.
                Setup(a => a.ApplicationLogInAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<AuthenticationProperties>(ap => ap.IsPersistent == true))).
                Returns(Task.CompletedTask);
            mockAccountService.
                Setup(a => a.TwoFactorLogOffAsync()).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            TwoFactorLogInActionResult result = await mockAccountService.Object.TwoFactorLogInActionAsync(_testToken, true);

            // Assert
            Assert.Equal(TwoFactorLogInActionResult.Success, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorLogInActionAsync_ReturnsTwoFactorLogInActionResultInvalidCodeIfCodeIsInvalid()
        {
            // Arrange
            _testAccountServiceOptions.TwoFactorTokenPurpose = _testTokenPurpose;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetTwoFactorAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.TotpTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Invalid);
            mockAccountService.CallBase = true;

            // Act
            TwoFactorLogInActionResult result = await mockAccountService.Object.TwoFactorLogInActionAsync(_testToken, true);

            // Assert
            Assert.Equal(TwoFactorLogInActionResult.InvalidCode, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorLogInActionAsync_ReturnsTwoFactorLogInActionResultInvalidCredentialsIfCredentialsAreInvalid()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetTwoFactorAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            TwoFactorLogInActionResult result = await mockAccountService.Object.TwoFactorLogInActionAsync(_testToken, true);

            // Assert
            Assert.Equal(TwoFactorLogInActionResult.InvalidCredentials, result);
            mockAccountService.VerifyAll();
        }
        #endregion

        #region AccountManagement
        [Fact]
        public async Task SignUpActionAsync_ReturnsSignUpActionResultSuccessIfSignUpIsSuccessful()
        {
            // Arrange
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.CreateAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<string>(s => s == _testPasswordHash),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testPassword))).
                Returns(_testPasswordHash);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.ApplicationLogInAsync(
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<AuthenticationProperties>(ap => ap.IsPersistent))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            SignUpActionResult result = await mockAccountService.Object.SignUpActionAsync(_testEmail, _testPassword);

            // Assert
            Assert.Equal(SignUpActionResult.Success, result);
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SignUpActionAsync_ReturnsSignUpActionResultInvalidEmailIfAccountEmailIsInUse()
        {
            // Arrange
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.CreateAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<string>(s => s == _testPasswordHash),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(null);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testPassword))).
                Returns(_testPasswordHash);

            AccountService<StubAccount> accountService = new AccountService<StubAccount>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);

            // Act
            SignUpActionResult result = await accountService.SignUpActionAsync(_testEmail, _testPassword);

            // Assert
            Assert.Equal(SignUpActionResult.EmailInUse, result);
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SendResetPasswordEmailActionAsync_ReturnsSendResetPasswordEmailActionResultSuccessIfEmailSendsSuccessfully()
        {
            // Arrange
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.SendResetPasswordEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            SendResetPasswordEmailActionResult result = await mockAccountService.Object.SendResetPasswordEmailActionAsync(_testEmail);

            // Assert
            Assert.Equal(SendResetPasswordEmailActionResult.Success, result);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SendResetPasswordEmailActionAsync_ReturnsSendResetPasswordEmailActionResultInvalidEmailIfEmailIsInvalid()
        {
            // Arrange
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(null);

            AccountService<StubAccount> accountService = new AccountService<StubAccount>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            SendResetPasswordEmailActionResult result = await accountService.SendResetPasswordEmailActionAsync(_testEmail);

            // Assert
            Assert.Equal(SendResetPasswordEmailActionResult.InvalidEmail, result);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordActionAsync_ReturnsResetPasswordActionResultSuccessIfPasswordResetIsSuccessful()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            _testAccountServiceOptions.ResetPasswordTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            mockAccountRepository.
                Setup(a => a.UpdatePasswordHashAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testPasswordHash),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testPassword))).
                Returns(_testPasswordHash);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            ResetPasswordActionResult result = await mockAccountService.Object.ResetPasswordActionAsync(_testEmail, _testToken, _testPassword);

            // Assert
            Assert.Equal(ResetPasswordActionResult.Success, result);
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordActionAsync_ReturnsResetPasswordActionResultInvalidEmailIfEmailIsInvalid()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(null);

            AccountService<StubAccount> accountService = new AccountService<StubAccount>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);

            // Act
            ResetPasswordActionResult result = await accountService.ResetPasswordActionAsync(_testEmail, _testToken, _testPassword);

            // Assert
            Assert.Equal(ResetPasswordActionResult.InvalidEmail, result);
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordActionAsync_ReturnsResetPasswordActionResultInvlidNewPasswordIfNewPasswordIsSameAsOldPassword()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            AccountService<StubAccount> accountService = new AccountService<StubAccount>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);

            // Act
            ResetPasswordActionResult result = await accountService.ResetPasswordActionAsync(_testEmail, _testToken, _testPassword);

            // Assert
            Assert.Equal(ResetPasswordActionResult.InvalidNewPassword, result);
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordActionAsync_ReturnsResetPasswordActionResultTokenInvalidIfTokenIsInvalid()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            _testAccountServiceOptions.ResetPasswordTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Invalid);
            mockAccountService.CallBase = true;

            // Act
            ResetPasswordActionResult result = await mockAccountService.Object.ResetPasswordActionAsync(_testEmail, _testToken, _testPassword);

            // Assert
            Assert.Equal(ResetPasswordActionResult.InvalidToken, result);
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task ResetPasswordActionAsync_ThrowsExceptionIfDatabaseErrorOccurs()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            _testAccountServiceOptions.ResetPasswordTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.GetAsync(It.Is<string>(s => s == _testEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(_testAccount);
            mockAccountRepository.
                Setup(a => a.UpdatePasswordHashAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testPasswordHash),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testPassword))).
                Returns(_testPasswordHash);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.
                ResetPasswordActionAsync(_testEmail, _testToken, _testPassword));

            // Assert
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task GetAccountDetailsActionAsync_ReturnsLoggedInAccountIfItExists()
        {
            // Arrange 
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            StubAccount result = await mockAccountService.Object.GetAccountDetailsActionAsync();

            // Assert
            Assert.Equal(result, _testAccount);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task GetAccountDetailsActionAsync_ReturnsNullIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange 
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            StubAccount account = await mockAccountService.Object.GetAccountDetailsActionAsync();

            // Assert
            Assert.Null(account);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetPasswordActionAsync_ReturnsSetPasswordActionResultSuccessIfPasswordChangeSucceeds()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdatePasswordHashAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testPasswordHash),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testNewPassword))).
                Returns(_testPasswordHash);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.RefreshApplicationLogInAsync(It.Is<StubAccount>(acc => acc == _testAccount))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            SetPasswordActionResult result = await mockAccountService.Object.SetPasswordActionAsync(_testPassword, _testNewPassword);

            // Assert
            Assert.Equal(SetPasswordActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetPasswordActionAsync_ReturnsSetPasswordActionResultInvalidNewPasswordIfAccountPasswordIsAlreadyEqualToNewPassword()
        {
            // Arrange
            AccountService<StubAccount> accountService = new AccountService<StubAccount>(
                null, null, null, null, null, null, null);


            // Act
            SetPasswordActionResult result = await accountService.SetPasswordActionAsync(_testPassword, _testPassword);

            // Assert
            Assert.Equal(SetPasswordActionResult.AlreadySet, result);
        }

        [Fact]
        public async Task SetPasswordActionAsync_ReturnsSetPasswordActionResultInvalidCurrentPasswordIfCurrentPasswordIsInvalid()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetPasswordActionResult result = await mockAccountService.Object.SetPasswordActionAsync(_testPassword, _testNewPassword);

            // Assert
            Assert.Equal(SetPasswordActionResult.InvalidCurrentPassword, result);
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetPasswordActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdatePasswordHashAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testPasswordHash),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);
            mockPasswordService.
                Setup(p => p.HashPassword(It.Is<string>(s => s == _testNewPassword))).
                Returns(_testPasswordHash);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.SetPasswordActionAsync(_testPassword, _testNewPassword));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetPasswordActionAsync_ReturnsSetPasswordActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SetPasswordActionResult result = await mockAccountService.Object.SetPasswordActionAsync(_testPassword, _testNewPassword);

            // Assert
            Assert.Equal(SetPasswordActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailActionAsync_ReturnsSetEmailActionResultSuccessIfEmailChangeSucceeds()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.RefreshApplicationLogInAsync(It.Is<StubAccount>(acc => acc == _testAccount))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            SetEmailActionResult result = await mockAccountService.Object.SetEmailActionAsync(_testPassword, _testNewEmail);

            // Assert
            Assert.Equal(SetEmailActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailActionAsync_ReturnsSetEmailActionResultEmailInUseIfNewEmailIsInUse()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.UniqueIndexViolation);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetEmailActionResult result = await mockAccountService.Object.SetEmailActionAsync(_testPassword, _testNewEmail);

            // Assert
            Assert.Equal(SetEmailActionResult.EmailInUse, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailActionAsync_ReturnsSetEmailActionResultInvalidNewEmailIfAccountEmailIsAlreadyEqualToNewEmail()
        {
            // Arrange
            _testAccount.Email = _testNewEmail;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetEmailActionResult result = await mockAccountService.Object.SetEmailActionAsync(_testPassword, _testNewEmail);


            // Assert
            Assert.Equal(SetEmailActionResult.AlreadySet, result);
        }

        [Fact]
        public async Task SetEmailActionAsync_ReturnsSetEmailActionResultInvalidPasswordIfPasswordIsInvalid()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetEmailActionResult result = await mockAccountService.Object.SetEmailActionAsync(_testPassword, _testNewEmail);

            // Assert
            Assert.Equal(SetEmailActionResult.InvalidPassword, result);
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.SetEmailActionAsync(_testPassword, _testNewEmail));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailActionAsync_ReturnsSetEmailActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SetEmailActionResult result = await mockAccountService.Object.SetEmailActionAsync(_testPassword, _testNewEmail);

            // Assert
            Assert.Equal(SetEmailActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailActionAsync_ReturnsSetAltEmailActionResultSuccessIfAltEmailChangeSucceeds()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewAltEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailActionResult result = await mockAccountService.Object.SetAltEmailActionAsync(_testPassword, _testNewAltEmail);

            // Assert
            Assert.Equal(SetAltEmailActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailActionAsync_ReturnsSetAltEmailActionResultInvalidNewAltEmailIfAccountAltEmailIsAlreadyEqualToNewAltEmail()
        {
            // Arrange
            _testAccount.AltEmail = _testNewAltEmail;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailActionResult result = await mockAccountService.Object.SetAltEmailActionAsync(_testPassword, _testNewAltEmail);


            // Assert
            Assert.Equal(SetAltEmailActionResult.AlreadySet, result);
        }

        [Fact]
        public async Task SetAltEmailActionAsync_ReturnsSetAltEmailActionResultInvalidPasswordIfPasswordIsInvalid()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailActionResult result = await mockAccountService.Object.SetAltEmailActionAsync(_testPassword, _testNewAltEmail);

            // Assert
            Assert.Equal(SetAltEmailActionResult.InvalidPassword, result);
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewAltEmail),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.SetAltEmailActionAsync(_testPassword, _testNewAltEmail));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailActionAsync_ReturnsSetAltEmailActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailActionResult result = await mockAccountService.Object.SetAltEmailActionAsync(_testPassword, _testNewAltEmail);

            // Assert
            Assert.Equal(SetAltEmailActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetDisplayNameActionAsync_ReturnsSetDisplayNameActionResultSuccessIfDisplayNameChangeSucceeds()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateDisplayNameAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewDisplayName),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetDisplayNameActionResult result = await mockAccountService.Object.SetDisplayNameActionAsync(_testPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(SetDisplayNameActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetDisplayNameActionAsync_ReturnsSetDisplayNameActionResultDisplayNameInUseIfNewDisplayNameIsInUse()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateDisplayNameAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewDisplayName),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.UniqueIndexViolation);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetDisplayNameActionResult result = await mockAccountService.Object.SetDisplayNameActionAsync(_testPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(SetDisplayNameActionResult.DisplayNameInUse, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetDisplayNameActionAsync_ReturnsSetDisplayNameActionResultInvalidNewDisplayNameIfAccountDisplayNameIsAlreadyEqualToNewDisplayName()
        {
            // Arrange
            _testAccount.DisplayName = _testNewDisplayName;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetDisplayNameActionResult result = await mockAccountService.Object.SetDisplayNameActionAsync(_testPassword, _testNewDisplayName);


            // Assert
            Assert.Equal(SetDisplayNameActionResult.AlreadySet, result);
        }

        [Fact]
        public async Task SetDisplayNameActionAsync_ReturnsSetDisplayNameActionResultInvalidPasswordIfPasswordIsInvalid()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(false);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetDisplayNameActionResult result = await mockAccountService.Object.SetDisplayNameActionAsync(_testPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(SetDisplayNameActionResult.InvalidPassword, result);
            mockPasswordService.VerifyAll();
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetDisplayNameActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.PasswordHash = _testPasswordHash;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateDisplayNameAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testNewDisplayName),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);
            Mock<IPasswordService> mockPasswordService = new Mock<IPasswordService>();
            mockPasswordService.
                Setup(p => p.ValidatePassword(It.Is<string>(s => s == _testPasswordHash),
                    It.Is<string>(s => s == _testPassword))).
                Returns(true);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                mockPasswordService.Object);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.SetDisplayNameActionAsync(_testPassword, _testNewDisplayName));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
            mockPasswordService.VerifyAll();
        }

        [Fact]
        public async Task SetDisplayNameActionAsync_ReturnsSetDisplayNameActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SetDisplayNameActionResult result = await mockAccountService.Object.SetDisplayNameActionAsync(_testPassword, _testNewDisplayName);

            // Assert
            Assert.Equal(SetDisplayNameActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetTwoFactorEnabledActionAsync_ReturnsSetTwoFactorEnabledActionResultSuccessIfTwoFactorEnabledChangeSucceeds()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateTwoFactorEnabledAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == _testNewTwoFactorEnabled),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetTwoFactorEnabledActionResult result = await mockAccountService.Object.
                SetTwoFactorEnabledActionAsync(_testNewTwoFactorEnabled);

            // Assert
            Assert.Equal(SetTwoFactorEnabledActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetTwoFactorEnabledActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateTwoFactorEnabledAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == _testNewTwoFactorEnabled),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.
                SetTwoFactorEnabledActionAsync(_testNewTwoFactorEnabled));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetTwoFactorEnabledActionAsync_ReturnsSetTwoFactorEnabledActionResultAlreadySetIfAccountTwoFactorEnabledIsAlreadyEqualToEnabled()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            _testAccount.TwoFactorEnabled = _testNewTwoFactorEnabled;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetTwoFactorEnabledActionResult result = await mockAccountService.Object.
                SetTwoFactorEnabledActionAsync(_testNewTwoFactorEnabled);

            // Assert
            Assert.Equal(SetTwoFactorEnabledActionResult.AlreadySet, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetTwoFactorEnabledActionAsync_ReturnsSetTwoFactorEnabledActionResultEmailUnverifiedIfAccountEmailIsUnverified()
        {
            // Arrange
            _testAccount.EmailVerified = false;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.SendTwoFactorCodeEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            SetTwoFactorEnabledActionResult result = await mockAccountService.Object.
                SetTwoFactorEnabledActionAsync(_testNewTwoFactorEnabled);

            // Assert
            Assert.Equal(SetTwoFactorEnabledActionResult.EmailUnverified, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetTwoFactorEnabledActionAsync_ReturnsSetTwoFactorEnabledActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SetTwoFactorEnabledActionResult result = await mockAccountService.Object.
                SetTwoFactorEnabledActionAsync(_testNewTwoFactorEnabled);

            // Assert
            Assert.Equal(SetTwoFactorEnabledActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailVerifiedActionAsync_ReturnsSetEmailVerifiedActionResultSuccessIfEmailVerifiedChangeSucceeds()
        {
            // Arrange
            _testAccount.EmailVerified = false;
            _testAccountServiceOptions.ConfirmEmailTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailVerifiedAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == true),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            SetEmailVerifiedActionResult result = await mockAccountService.Object.
                SetEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetEmailVerifiedActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetEmailVerifiedActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.EmailVerified = false;
            _testAccountServiceOptions.ConfirmEmailTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailVerifiedAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == true),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.
                SetEmailVerifiedActionAsync(_testToken));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetEmailVerifiedActionAsync_ReturnsSetEmailVerifiedActionResultInvalidTokenIfTokenIsInvalid()
        {
            // Arrange
            _testAccount.EmailVerified = false;
            _testAccountServiceOptions.ConfirmEmailTokenPurpose = _testTokenPurpose;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Invalid);
            mockAccountService.CallBase = true;

            // Act
            SetEmailVerifiedActionResult result = await mockAccountService.Object.
                SetEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetEmailVerifiedActionResult.InvalidToken, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailVerifiedActionAsync_ReturnsSetEmailVerifiedActionResultAlreadySetIfEmailVerifiedIsAlreadyTrue()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetEmailVerifiedActionResult result = await mockAccountService.Object.
                SetEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetEmailVerifiedActionResult.AlreadySet, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetEmailVerifiedActionAsync_ReturnsSetEmailVerifiedActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            // Act
            SetEmailVerifiedActionResult result = await mockAccountService.Object.SetEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetEmailVerifiedActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailVerifiedActionAsync_ReturnsSetAltEmailVerifiedActionResultSuccessIfAltEmailVerifiedChangeSucceeds()
        {
            // Arrange
            _testAccount.AltEmailVerified = false;
            _testAccountServiceOptions.ConfirmAltEmailTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailVerifiedAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == true),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailVerifiedActionResult result = await mockAccountService.Object.
                SetAltEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetAltEmailVerifiedActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailVerifiedActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.AltEmailVerified = false;
            _testAccountServiceOptions.ConfirmAltEmailTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateAltEmailVerifiedAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == true),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.
                SetAltEmailVerifiedActionAsync(_testToken));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailVerifiedActionAsync_ReturnsSetAltEmailVerifiedActionResultInvalidTokenIfTokenIsInvalid()
        {
            // Arrange
            _testAccount.AltEmailVerified = false;
            _testAccountServiceOptions.ConfirmAltEmailTokenPurpose = _testTokenPurpose;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.DataProtectionTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Invalid);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailVerifiedActionResult result = await mockAccountService.Object.
                SetAltEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetAltEmailVerifiedActionResult.InvalidToken, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailVerifiedActionAsync_ReturnsSetAltEmailVerifiedActionResultAlreadySetIfAltEmailVerifiedIsAlreadyTrue()
        {
            // Arrange
            _testAccount.AltEmailVerified = true;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailVerifiedActionResult result = await mockAccountService.Object.
                SetAltEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetAltEmailVerifiedActionResult.AlreadySet, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SetAltEmailVerifiedActionAsync_ReturnsSetAltEmailVerifiedActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SetAltEmailVerifiedActionResult result = await mockAccountService.Object.SetAltEmailVerifiedActionAsync(_testToken);

            // Assert
            Assert.Equal(SetAltEmailVerifiedActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SendEmailVerificationEmailActionAsync_ReturnsSendEmailVerifiedEmailActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SendEmailVerificationEmailActionResult result = await mockAccountService.Object.SendEmailVerificationEmailActionAsync();

            // Assert
            Assert.Equal(SendEmailVerificationEmailActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SendAltEmailVerifiedEmailActionAsync_ReturnsSendAltEmailVerificationEmailActionResultSuccessIfAltEmailVerificationSucceeds()
        {
            // Arrange
            _testAccount.AltEmail = _testAltEmail;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.SendAltEmailVerificationEmailAsync(It.Is<StubAccount>(acc => acc == _testAccount))).
                Returns(Task.CompletedTask);
            mockAccountService.CallBase = true;

            // Act
            SendAltEmailVerificationEmailActionResult result = await mockAccountService.Object.
                SendAltEmailVerificationEmailActionAsync();

            // Assert
            Assert.Equal(SendAltEmailVerificationEmailActionResult.Success, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SendAltEmailVerifiedEmailActionAsync_ReturnsSendAltEmailVerificationEmailActionResultNoAltEmailIfAccountAltEmailIsNull()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            SendAltEmailVerificationEmailActionResult result = await mockAccountService.Object.
                SendAltEmailVerificationEmailActionAsync();

            // Assert
            Assert.Equal(SendAltEmailVerificationEmailActionResult.NoAltEmail, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task SendAltEmailVerifiedEmailActionAsync_ReturnsSendAltEmailVerificationEmailActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            SendAltEmailVerificationEmailActionResult result = await mockAccountService.Object.
                SendAltEmailVerificationEmailActionAsync();

            // Assert
            Assert.Equal(SendAltEmailVerificationEmailActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorVerifyEmailActionAsync_ReturnsTwoFactorVerifyEmailActionResultSuccessIfEmailVerifiedChangeSucceeds()
        {
            // Arrange
            _testAccount.EmailVerified = false;
            _testAccountServiceOptions.TwoFactorTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailVerifiedAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == true),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.Success);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.TotpTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            TwoFactorVerifyEmailActionResult result = await mockAccountService.Object.
                TwoFactorVerifyEmailActionAsync(_testToken);

            // Assert
            Assert.Equal(TwoFactorVerifyEmailActionResult.Success, result);
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorVerifyEmailActionAsync_ThrowsExceptionIfDatabaseUpdateFails()
        {
            // Arrange
            _testAccount.EmailVerified = false;
            _testAccountServiceOptions.TwoFactorTokenPurpose = _testTokenPurpose;
            Mock<IAccountRepository<StubAccount>> mockAccountRepository = new Mock<IAccountRepository<StubAccount>>();
            mockAccountRepository.
                Setup(a => a.UpdateEmailVerifiedAsync(It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<bool>(b => b == true),
                    It.Is<CancellationToken>(c => c == CancellationToken.None))).
                ReturnsAsync(SaveChangesResult.ConcurrencyError);

            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                mockAccountRepository.Object,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.TotpTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Valid);
            mockAccountService.CallBase = true;

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await mockAccountService.Object.
                TwoFactorVerifyEmailActionAsync(_testToken));

            // Assert
            mockAccountService.VerifyAll();
            mockAccountRepository.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorVerifyEmailActionAsync_ReturnsTwoFactorVerifyEmailActionResultInvalidCodeIfCodeIsInvalid()
        {
            // Arrange
            _testAccount.EmailVerified = false;
            _testAccountServiceOptions.TwoFactorTokenPurpose = _testTokenPurpose;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                CreateMockAccountServiceOptionsAccessor().Object,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.
                Setup(a => a.ValidateToken(It.Is<string>(s => s == TokenServiceOptions.TotpTokenService),
                    It.Is<string>(s => s == _testTokenPurpose),
                    It.Is<StubAccount>(acc => acc == _testAccount),
                    It.Is<string>(s => s == _testToken))).
                Returns(ValidateTokenResult.Invalid);
            mockAccountService.CallBase = true;

            // Act
            TwoFactorVerifyEmailActionResult result = await mockAccountService.Object.
                TwoFactorVerifyEmailActionAsync(_testToken);

            // Assert
            Assert.Equal(TwoFactorVerifyEmailActionResult.InvalidCode, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorVerifyEmailActionAsync_ReturnsTwoFactorVerifyEmailActionResultAlreadySetIfAccountEmailVerifiedIsAlreadyTrue()
        {
            // Arrange
            _testAccount.EmailVerified = true;
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(_testAccount);
            mockAccountService.CallBase = true;

            // Act
            TwoFactorVerifyEmailActionResult result = await mockAccountService.Object.
                TwoFactorVerifyEmailActionAsync(_testToken);

            // Assert
            Assert.Equal(TwoFactorVerifyEmailActionResult.AlreadySet, result);
            mockAccountService.VerifyAll();
        }

        [Fact]
        public async Task TwoFactorVerifyEmailActionAsync_ReturnsTwoFactorVerifyEmailActionResultNoLoggedInAccountIfUnableToRetrieveLoggedInAccount()
        {
            // Arrange
            Mock<AccountService<StubAccount>> mockAccountService = new Mock<AccountService<StubAccount>>(
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            mockAccountService.
                Setup(a => a.GetApplicationAccountAsync()).
                ReturnsAsync(null);
            mockAccountService.CallBase = true;

            // Act
            TwoFactorVerifyEmailActionResult result = await mockAccountService.Object.
                TwoFactorVerifyEmailActionAsync(_testToken);

            // Assert
            Assert.Equal(TwoFactorVerifyEmailActionResult.NoLoggedInAccount, result);
            mockAccountService.VerifyAll();
        }
        #endregion

        #region Helpers
        private Mock<IOptions<AccountServiceOptions>> CreateMockAccountServiceOptionsAccessor()
        {
            Mock<IOptions<AccountServiceOptions>> mockOptions = new Mock<IOptions<AccountServiceOptions>>();
            mockOptions.
                Setup(o => o.Value).
                Returns(_testAccountServiceOptions);

            return mockOptions;
        }
        #endregion
    }

    public class StubAccount : IAccount
    {
        public int AccountId { get; set; }

        public string AltEmail { get; set; }

        public bool AltEmailVerified { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }
        public bool EmailVerified { get; set; }

        public string PasswordHash { get; set; }

        public DateTimeOffset PasswordLastChanged { get; set; }

        public byte[] RowVersion { get; set; }

        public Guid SecurityStamp { get; set; }

        public bool TwoFactorEnabled { get; set; }
    }
}

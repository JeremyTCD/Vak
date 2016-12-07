using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Xunit;
using System.Text;
using System.Data;
using Dapper;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.DatabaseInterface.Dapper.Tests.IntegrationTests
{
    [Collection("DapperDatabaseCollection")]
    public class DapperAccountRepositoryTests
    {
        private SqlConnection _sqlConnection { get; }
        private DapperRoleRepository _dapperRoleRepository { get; }
        private DapperClaimRepository _dapperClaimRepository { get; }
        private DapperAccountRepository<Account> _dapperAccountRepository { get; }
        private Func<Task> _resetClaimsTable { get; }
        private Func<Task> _resetRolesTable { get; }
        private Func<Task> _resetAccountsTable { get; }

        private const string _testEmail1 = "test@email1.com";
        private const string _testEmail2 = "test@email2.com";
        private const string _testAlternativeEmail = "testAlternative@email.com";
        private const string _testWrongEmail = "testWrong@email.com";
        private const string _testPassword = "testPassword";
        private const string _testNewPassword = "testNewPassword";
        private const string _testWrongPassword = "testWrongPassword";
        private const string _testDisplayName = "testDisplayName";
        private const string _testRoleName1 = "testRoleName1";
        private const string _testRoleName2 = "testRoleName2";
        private const string _testClaimType1 = "testClaimType1";
        private const string _testClaimValue1 = "testClaimValue1";
        private const string _testClaimType2 = "testClaimType2";
        private const string _testClaimValue2 = "testClaimValue2";

        // TODO ensure that functions that should trigger security stamp renewal do so.

        public DapperAccountRepositoryTests(DapperDatabaseFixture databaseFixture)
        {
            _sqlConnection = databaseFixture.SqlConnection;
            _dapperRoleRepository = databaseFixture.DapperRoleRepository;
            _dapperClaimRepository = databaseFixture.DapperClaimRepository;
            _dapperAccountRepository = databaseFixture.DapperAccountRepository;
            _resetClaimsTable = databaseFixture.ResetClaimsTable;
            _resetRolesTable = databaseFixture.ResetRolesTable;
            _resetAccountsTable = databaseFixture.ResetAccountsTable;
        }

        [Fact]
        public async Task CreateAccountAsync_ReturnsAccountIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Assert
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.Unicode.GetBytes(_testPassword + _testEmail1));
            Assert.Equal(1, account.AccountId);
            Assert.Equal(_testEmail1, account.Email);
            Assert.Equal(null, account.DisplayName);
            Assert.NotEqual(Guid.Empty, account.SecurityStamp);
            Assert.Equal(false, account.EmailVerified);
            Assert.Equal(false, account.TwoFactorEnabled);
        }

        [Fact]
        public async Task CreateAccountAsync_CreatesSecurityStampTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Guid securityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);

            // Assert
            Assert.NotEqual(Guid.Empty, securityStamp);
        }

        [Fact]
        public async Task CreateAccountAsync_ThrowsExceptionIfDuplicateEmailTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("An account with this email already exists.", sqlException.Message);
        }

        [Fact]
        public async Task GetAccountAsync_GetsAccountTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            IAccount retrievedAccount = await _dapperAccountRepository.GetAccountAsync(account.AccountId);

            // Assert
            Assert.NotEqual(null, retrievedAccount);
            Assert.Equal(1, retrievedAccount.AccountId);
            Assert.Equal(_testEmail1, retrievedAccount.Email);
            Assert.Equal(null, retrievedAccount.DisplayName);
            Assert.NotEqual(Guid.Empty, retrievedAccount.SecurityStamp);
            Assert.Equal(false, retrievedAccount.EmailVerified);
            Assert.Equal(false, retrievedAccount.TwoFactorEnabled);
        }

        [Fact]
        public async Task GetAccountAsync_ReturnsNullIfAccountDoesNotExistTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount account = await _dapperAccountRepository.GetAccountAsync(0);

            // Assert
            Assert.Equal(null, account);
        }

        [Fact]
        public async Task GetAccountByEmailAndPasswordAsync_GetsAccountTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            IAccount retrievedAccount = await _dapperAccountRepository.GetAccountByEmailAndPasswordAsync(_testEmail1, _testPassword);

            // Assert
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.Unicode.GetBytes(_testPassword + _testEmail1));
            Assert.Equal(1, account.AccountId);
            Assert.Equal(_testEmail1, account.Email);
            Assert.Equal(null, account.DisplayName);
            Assert.NotEqual(Guid.Empty, account.SecurityStamp);
            Assert.Equal(false, account.EmailVerified);
            Assert.Equal(false, account.TwoFactorEnabled);
        }

        [Fact]
        public async Task GetAccountByEmailAndPasswordAsync_ReturnsNullIfAccountDoesNotExistTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            IAccount wrongPasswordAccount = await _dapperAccountRepository.GetAccountByEmailAndPasswordAsync(_testEmail1, _testWrongPassword);
            IAccount wrongEmailAndPasswordAccount = await _dapperAccountRepository.GetAccountByEmailAndPasswordAsync(_testWrongEmail, _testWrongPassword);

            // Assert
            Assert.Equal(null, wrongPasswordAccount);
            Assert.Equal(null, wrongEmailAndPasswordAccount);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_GetsAccountTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            IAccount retrievedAccount = await _dapperAccountRepository.GetAccountByEmailAsync(_testEmail1);

            // Assert
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.Unicode.GetBytes(_testPassword + _testEmail1));
            Assert.Equal(1, account.AccountId);
            Assert.Equal(_testEmail1, account.Email);
            Assert.Equal(null, account.DisplayName);
            Assert.NotEqual(Guid.Empty, account.SecurityStamp);
            Assert.Equal(false, account.EmailVerified);
            Assert.Equal(false, account.TwoFactorEnabled);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_ReturnsNullIfAccountDoesNotExistTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount nonExistantAccount = await _dapperAccountRepository.GetAccountByEmailAsync(_testEmail1);

            // Assert
            Assert.Equal(null, nonExistantAccount);
        }

        [Theory]
        [MemberData(nameof(GetAccountByEmailOrAlternativeEmailAsyncData))]
        public async Task GetAccountByEmailOrAlternativeEmailAsync_GetsAccountTest(bool useAlt)
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            if (useAlt)
            {
                await _dapperAccountRepository.UpdateAccountAlternativeEmailAsync(account.AccountId, _testAlternativeEmail);
            }

            // Act
            account = await _dapperAccountRepository.GetAccountByEmailOrAlternativeEmailAsync(useAlt ? _testAlternativeEmail : _testEmail1);

            // Assert
            Assert.NotNull(account);
        }

        public static IEnumerable<object[]> GetAccountByEmailOrAlternativeEmailAsyncData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task GetAccountByEmailOrAlternativeEmailAsync_ReturnsNullIfAccountDoesNotExistTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount nonExistantAccount = await _dapperAccountRepository.GetAccountByEmailOrAlternativeEmailAsync(_testEmail1);

            // Assert
            Assert.Equal(null, nonExistantAccount);
        }

        [Fact]
        public async Task DeleteAccountAsync_DeletesAccountTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            await _dapperAccountRepository.DeleteAccountAsync(account.AccountId);

            // Assert
            Assert.Equal(0, await _sqlConnection.ExecuteScalarAsync<int>("Select Count(*) from [dbo].[Accounts]", commandType: CommandType.Text));
        }

        [Fact]
        public async Task DeleteAccountAsync_ReturnsTrueIfSuccessfulTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.DeleteAccountAsync(account.AccountId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAccountAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.DeleteAccountAsync(1);

            // Assert
            Assert.False(result);
        }

        public async Task AddAccountRoleAsync_AddsAccountRoleTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);

            // Assert
            Role retrievedRole = (await _dapperAccountRepository.GetAccountRolesAsync(account.AccountId)).First();
            Assert.Equal(1, retrievedRole.RoleId);
            Assert.Equal(_testRoleName1, retrievedRole.Name);
        }

        [Fact]
        public async Task AddAccountRoleAsync_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddAccountRoleAsync_ThrowsExceptionIfAccountRoleAlreadyExistsTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);

            // Act
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account already has role.", sqlException.Message);
        }

        [Fact]
        public async Task AddRoleAccountAsync_ThrowsExceptionIfAccountOrRoleDoesNotExistTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();

            // Act
            SqlException sqlException = await Assert.
                ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddAccountRoleAsync(1, 1));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account or role does not exist.", sqlException.Message);
        }

        [Fact]
        public async Task AddAccountRoleAsync_UpdatesSecurityStampTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Guid initialSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);

            // Act
            Guid finalSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);

            // Assert
            Assert.NotEqual(Guid.Empty, initialSecurityStamp);
            Assert.NotEqual(Guid.Empty, finalSecurityStamp);
            Assert.NotEqual(initialSecurityStamp, finalSecurityStamp);
        }

        [Fact]
        public async Task DeleteAccountRoleAsync_DeletesAccountRoleTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);

            // Act
            await _dapperAccountRepository.DeleteAccountRoleAsync(account.AccountId, role.RoleId);

            // Assert
            List<Role> roles = (await _dapperAccountRepository.GetAccountRolesAsync(role.RoleId)).ToList<Role>();
            Assert.Equal(0, roles.Count);
        }

        [Fact]
        public async Task DeleteAccountRoleAsync_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);

            // Act
            bool result = await _dapperAccountRepository.DeleteAccountRoleAsync(account.AccountId, role.RoleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAccountRoleAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.DeleteAccountRoleAsync(0, 0) ||
                await _dapperAccountRepository.DeleteAccountRoleAsync(account.AccountId, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAccountRoleAsync_UpdatesSecurityStampTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);
            Guid initialSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);
            await _dapperAccountRepository.DeleteAccountRoleAsync(account.AccountId, role.RoleId);

            // Act
            Guid finalSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);

            // Assert
            Assert.NotEqual(Guid.Empty, initialSecurityStamp);
            Assert.NotEqual(Guid.Empty, finalSecurityStamp);
            Assert.NotEqual(initialSecurityStamp, finalSecurityStamp);
        }

        [Fact]
        public async Task GetAccountRolesAsync_GetsAccountRolesTest()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetRolesTable();
            Role role1 = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            Role role2 = await _dapperRoleRepository.CreateRoleAsync(_testRoleName2);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role1.RoleId);
            await _dapperAccountRepository.AddAccountRoleAsync(account.AccountId, role2.RoleId);

            // Act
            List<Role> roles = (await _dapperAccountRepository.GetAccountRolesAsync(account.AccountId)).ToList<Role>();

            // Assert
            Assert.Equal(2, roles.Count);
            Assert.Equal(_testRoleName1, roles[0].Name);
            Assert.Equal(_testRoleName2, roles[1].Name);
        }

        [Fact]
        public async Task GetAccountRolesAsync_ReturnsEmptyListIfAccountRolesOrAccountDoesNotExist()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetRolesTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            List<Role> accountDoesNotExistRoles = (await _dapperAccountRepository.GetAccountRolesAsync(0)).ToList<Role>();
            List<Role> rolesDoNotExistRoles = (await _dapperAccountRepository.GetAccountRolesAsync(account.AccountId)).ToList<Role>();

            // Assert
            Assert.NotEqual(null, accountDoesNotExistRoles);
            Assert.Equal(0, accountDoesNotExistRoles.Count);
            Assert.NotEqual(null, rolesDoNotExistRoles);
            Assert.Equal(0, rolesDoNotExistRoles.Count);
        }

        [Fact]
        public async Task AddAccountClaimAsync_AddsAccountClaimTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            Claim retrievedClaim = (await _dapperAccountRepository.GetAccountClaimsAsync(account.AccountId)).First();
            Assert.Equal(1, retrievedClaim.ClaimId);
            Assert.Equal(_testClaimType1, retrievedClaim.Type);
            Assert.Equal(_testClaimValue1, retrievedClaim.Value);
        }

        [Fact]
        public async Task AddAccountClaimAsync_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddAccountClaimAsync_UpdatesSecurityStampTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Guid initialSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Act
            Guid finalSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);

            // Assert
            Assert.NotEqual(Guid.Empty, initialSecurityStamp);
            Assert.NotEqual(Guid.Empty, finalSecurityStamp);
            Assert.NotEqual(initialSecurityStamp, finalSecurityStamp);
        }

        [Fact]
        public async Task AddAccountClaimAsync_ThrowsExceptionIfAccountClaimAlreadyExistsTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);

            // Act
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account already has claim.", sqlException.Message);
        }

        [Fact]
        public async Task AddAccountClaimAsync_ThrowsExceptionIfAccountOrClaimDoesNotExistTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();

            // Act
            SqlException sqlException = await Assert.
                ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddAccountClaimAsync(1, 1));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account or claim does not exist.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteAccountClaimAsync_DeletesAccountClaimTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Act
            await _dapperAccountRepository.DeleteAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            List<Claim> claims = (await _dapperAccountRepository.GetAccountClaimsAsync(claim.ClaimId)).ToList<Claim>();
            Assert.Equal(0, claims.Count);
        }

        [Fact]
        public async Task DeleteAccountClaimAsync_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Act
            bool result = await _dapperAccountRepository.DeleteAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAccountClaimAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.DeleteAccountClaimAsync(0, 0) ||
                await _dapperAccountRepository.DeleteAccountClaimAsync(account.AccountId, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAccountClaimAsync_UpdatesSecurityStampTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);
            Guid initialSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);
            await _dapperAccountRepository.DeleteAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Act
            Guid finalSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);

            // Assert
            Assert.NotEqual(Guid.Empty, initialSecurityStamp);
            Assert.NotEqual(Guid.Empty, finalSecurityStamp);
            Assert.NotEqual(initialSecurityStamp, finalSecurityStamp);
        }

        [Fact]
        public async Task GetAccountClaimsAsync_GetsAccountClaimsTest()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetClaimsTable();
            Claim claim1 = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            Claim claim2 = await _dapperClaimRepository.CreateClaimAsync(_testClaimType2, _testClaimValue2);
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim1.ClaimId);
            await _dapperAccountRepository.AddAccountClaimAsync(account.AccountId, claim2.ClaimId);

            // Act
            List<Claim> claims = (await _dapperAccountRepository.GetAccountClaimsAsync(account.AccountId)).ToList<Claim>();

            // Assert
            Assert.Equal(2, claims.Count);
            Assert.Equal(_testClaimType1, claims[0].Type);
            Assert.Equal(_testClaimValue1, claims[0].Value);
            Assert.Equal(_testClaimType2, claims[1].Type);
            Assert.Equal(_testClaimValue2, claims[1].Value);
        }

        [Fact]
        public async Task GetAccountClaimsAsync_ReturnsEmptyListIfAccountClaimsOrAccountDoesNotExistTest()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetClaimsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            List<Claim> accountDoesNotExistClaims = (await _dapperAccountRepository.GetAccountClaimsAsync(0)).ToList<Claim>();
            List<Claim> noAccountClaimsClaims = (await _dapperAccountRepository.GetAccountClaimsAsync(account.AccountId)).ToList<Claim>();

            // Assert
            Assert.NotEqual(null, accountDoesNotExistClaims);
            Assert.Equal(0, accountDoesNotExistClaims.Count);
            Assert.NotEqual(null, noAccountClaimsClaims);
            Assert.Equal(0, noAccountClaimsClaims.Count);
        }

        [Fact]
        public async Task GetAccountSecurityStamp_GetsSecurityStampIfAccountExistsTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            Guid existingSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(account.AccountId);

            // Assert
            Assert.NotEqual(Guid.Empty, existingSecurityStamp);
        }

        [Fact]
        public async Task GetAccountSecurityStamp_ReturnsEmptyGuidIfAccountDoesNotExistTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            Guid nonExistantSecurityStamp = await _dapperAccountRepository.GetAccountSecurityStampAsync(0);

            // Assert
            Assert.Equal(Guid.Empty, nonExistantSecurityStamp);
        }

        [Fact]
        public async Task UpdateAccountEmailVerifiedAsync_UpdatesEmailVerifiedAndReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountEmailVerifiedAsync(account.AccountId, true);

            // Assert
            IAccount retrievedAccount = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.False(account.EmailVerified);
            Assert.True(retrievedAccount.EmailVerified);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAccountEmailVerifiedAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountEmailVerifiedAsync(0, true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTwoFactorEnabledAsync_UpdatesTwoFactorEnabledAndReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountTwoFactorEnabledAsync(account.AccountId, true);

            // Assert
            IAccount retrievedAccount = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.False(account.TwoFactorEnabled);
            Assert.True(retrievedAccount.TwoFactorEnabled);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateTwoFactorEnabledAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountTwoFactorEnabledAsync(0, true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAccountPasswordHashAsync_UpdatesPasswordHashAndReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountPasswordHashAsync(account.AccountId, _testNewPassword);

            // Assert
            account = await _dapperAccountRepository.GetAccountByEmailAndPasswordAsync(account.Email, _testNewPassword);
            Assert.NotNull(account);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAccountPasswordHashAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountPasswordHashAsync(0, _testNewPassword);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(UpdateAccountEmailData))]
        public async Task UpdateAccountEmail_ThrowsExceptionIfDuplicateEmailTest(bool alt)
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            if (alt)
            {
                await _dapperAccountRepository.CreateAccountAsync("Email3@Jering.com", _testPassword);
                await _dapperAccountRepository.UpdateAccountAlternativeEmailAsync(2, _testAlternativeEmail);
            }
            else
            {
                await _dapperAccountRepository.CreateAccountAsync(_testAlternativeEmail, _testPassword);
            }

            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.UpdateAccountEmailAsync(account.AccountId, _testAlternativeEmail));
            // TODO if duplicate is an alt

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("EmailInUse", sqlException.Message);
        }

        public static IEnumerable<object[]> UpdateAccountEmailData()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        [Fact]
        public async Task UpdateAccountEmail_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountEmailAsync(account.AccountId, _testEmail2);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testEmail2, account.Email);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAccountEmail_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountEmailAsync(0, _testEmail1);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(UpdateAccountAlternativeEmailData))]
        public async Task UpdateAccountAlternativeEmail_ThrowsExceptionIfDuplicateAlternativeEmailTest(bool alt)
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            if (alt)
            {
                await _dapperAccountRepository.CreateAccountAsync(_testEmail2, _testPassword);
                await _dapperAccountRepository.UpdateAccountAlternativeEmailAsync(2, _testAlternativeEmail);
            }
            else{
                await _dapperAccountRepository.CreateAccountAsync(_testAlternativeEmail, _testPassword);
            }

            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.UpdateAccountAlternativeEmailAsync(account.AccountId, _testAlternativeEmail));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("EmailInUse", sqlException.Message);
        }

        public static IEnumerable<object[]> UpdateAccountAlternativeEmailData()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        [Fact]
        public async Task UpdateAccountAlternativeEmail_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountAlternativeEmailAsync(account.AccountId, _testAlternativeEmail);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testAlternativeEmail, account.AlternativeEmail);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAccountAlternativeEmail_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountAlternativeEmailAsync(0, _testAlternativeEmail);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAccountEmail_ThrowsExceptionIfDuplicateDisplayNameTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            await _dapperAccountRepository.CreateAccountAsync(_testEmail2, _testPassword);
            await _dapperAccountRepository.UpdateAccountDisplayNameAsync(2, _testDisplayName);

            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.UpdateAccountDisplayNameAsync(account.AccountId, _testDisplayName));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("DisplayNameInUse", sqlException.Message);
        }

        [Fact]
        public async Task UpdateDisplayNameAsync_UpdatesDisplayNameAndReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountDisplayNameAsync(account.AccountId, _testDisplayName);

            // Assert
            IAccount retrievedAccount = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(null, account.DisplayName);
            Assert.Equal(_testDisplayName, retrievedAccount.DisplayName);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateDisplayNameAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountDisplayNameAsync(0, _testDisplayName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAccountAlternativeEmailVerifiedAsync_UpdatesAlternativeEmailVerifiedAndReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountAlternativeEmailVerifiedAsync(account.AccountId, true);

            // Assert
            IAccount retrievedAccount = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.False(account.AlternativeEmailVerified);
            Assert.True(retrievedAccount.AlternativeEmailVerified);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAccountAlternativeEmailVerifiedAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.UpdateAccountAlternativeEmailVerifiedAsync(0, true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckEmailInUseAsync_ReturnsTrueIfEmailIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);

            // Act
            bool result = await _dapperAccountRepository.CheckEmailInUseAsync(_testEmail1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckEmailInUseAsync_ReturnsFalseIfEmailIsNotInUse()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.CheckEmailInUseAsync(_testEmail1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckDisplayNameInUseAsync_ReturnsTrueIfDisplayyNameIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            IAccount account = await _dapperAccountRepository.CreateAccountAsync(_testEmail1, _testPassword);
            await _dapperAccountRepository.UpdateAccountDisplayNameAsync(account.AccountId, _testDisplayName);

            // Act
            bool result = await _dapperAccountRepository.CheckDisplayNameInUseAsync(_testDisplayName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckDisplayNameInUseAsync_ReturnsFalseIfDisplayNameIsNotInUse()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            bool result = await _dapperAccountRepository.CheckDisplayNameInUseAsync(_testDisplayName);

            // Assert
            Assert.False(result);
        }
    }
}

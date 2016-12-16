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
using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts.DatabaseInterface.Dapper.Tests.IntegrationTests
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

        private const int _invalidAccountId = 0;
        private const string _testEmail1 = "test@email1.com";
        private const string _testEmail2 = "test@email2.com";
        private const string _testEmail3 = "test@email3.com";
        private const string _testAltEmail = "testAlternative@email.com";
        private const string _testWrongEmail = "testWrong@email.com";
        private const string _testPasswordHash = "testPasswordHash";
        private const string _testNewPasswordHash = "testNewPasswordHash";
        private const string _testDisplayName = "testDisplayName";
        private const string _testRoleName1 = "testRoleName1";
        private const string _testRoleName2 = "testRoleName2";
        private const string _testClaimType1 = "testClaimType1";
        private const string _testClaimValue1 = "testClaimValue1";
        private const string _testClaimType2 = "testClaimType2";
        private const string _testClaimValue2 = "testClaimValue2";
        private const bool _testEmailVerified = true;
        private const bool _testTwoFactorEnabled = true;
        private const bool _testAltEmailVerified = true;
        private DateTimeOffset _testPasswordLastChanged;
        private Guid _testSecurityStamp;

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
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            _testPasswordLastChanged = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, utcNow.Offset);
            _testSecurityStamp = Guid.NewGuid();
        }

        #region Create
        [Fact]
        public async Task CreateAccountAsync_ReturnsCreateAccountResultSucceededIfSuccessful()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            CreateResult<Account> result = await _dapperAccountRepository.
                CreateAsync(_testEmail1, _testPasswordHash, _testPasswordLastChanged, _testSecurityStamp);

            // Assert
            Assert.True(result.Succeeded);
            Account account = result.Account;
            VerifyAccount(account,
                1, _testEmail1, null, _testSecurityStamp, false, null, false, false, _testPasswordHash, _testPasswordLastChanged);
        }

        [Fact]
        public async Task CreateAccountAsync_ReturnsCreateAccountDuplicateRowIfEmailIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            CreateResult<Account> result = await _dapperAccountRepository.
                CreateAsync(_testEmail1, _testPasswordHash, _testPasswordLastChanged, _testSecurityStamp);

            // Assert
            Assert.True(result.DuplicateRow);
        }
        #endregion

        #region Read
        [Fact]
        public async Task GetAccountAsync_ReturnsAccountWithSpecifiedAccountIdIfItExists()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            Account retrievedAccount = await _dapperAccountRepository.GetAccountAsync(account.AccountId);

            // Assert          
            VerifyAccount(retrievedAccount,
                1, _testEmail1, null, _testSecurityStamp, false, null, false, false, _testPasswordHash, _testPasswordLastChanged);
        }

        [Fact]
        public async Task GetAccountAsync_ReturnsNullIfAccountWithSpecifiedAccountIdDoesNotExist()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            Account account = await _dapperAccountRepository.GetAccountAsync(0);

            // Assert
            Assert.Null(account);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_ReturnsAccountWithSpecifiedEmailIfItExists()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            Account account = await _dapperAccountRepository.GetByEmailAsync(_testEmail1);

            // Assert
            VerifyAccount(account,
                1, _testEmail1, null, _testSecurityStamp, false, null, false, false, _testPasswordHash, _testPasswordLastChanged);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_ReturnsNullIfAccountWithSpecifiedEmailDoesNotExist()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            Account nonExistantAccount = await _dapperAccountRepository.GetByEmailAsync(_testEmail1);

            // Assert
            Assert.Null(nonExistantAccount);
        }

        [Fact]
        public async Task GetAccountRolesAsync_GetsAccountRoles()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetRolesTable();
            Role role1 = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            Role role2 = await _dapperRoleRepository.CreateRoleAsync(_testRoleName2);
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            await _dapperAccountRepository.AddRoleAsync(account.AccountId, role1.RoleId);
            await _dapperAccountRepository.AddRoleAsync(account.AccountId, role2.RoleId);

            // Act
            List<Role> roles = (await _dapperAccountRepository.GetRolesAsync(account.AccountId)).ToList<Role>();

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
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            List<Role> accountDoesNotExistRoles = (await _dapperAccountRepository.GetRolesAsync(0)).ToList<Role>();
            List<Role> rolesDoNotExistRoles = (await _dapperAccountRepository.GetRolesAsync(account.AccountId)).ToList<Role>();

            // Assert
            Assert.NotEqual(null, accountDoesNotExistRoles);
            Assert.Equal(0, accountDoesNotExistRoles.Count);
            Assert.NotEqual(null, rolesDoNotExistRoles);
            Assert.Equal(0, rolesDoNotExistRoles.Count);
        }
        [Fact]
        public async Task GetAccountClaimsAsync_GetsAccountClaims()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetClaimsTable();
            Claim claim1 = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            Claim claim2 = await _dapperClaimRepository.CreateClaimAsync(_testClaimType2, _testClaimValue2);
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim1.ClaimId);
            await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim2.ClaimId);

            // Act
            List<Claim> claims = (await _dapperAccountRepository.GetClaimsAsync(account.AccountId)).ToList<Claim>();

            // Assert
            Assert.Equal(2, claims.Count);
            Assert.Equal(_testClaimType1, claims[0].Type);
            Assert.Equal(_testClaimValue1, claims[0].Value);
            Assert.Equal(_testClaimType2, claims[1].Type);
            Assert.Equal(_testClaimValue2, claims[1].Value);
        }

        [Fact]
        public async Task GetAccountClaimsAsync_ReturnsEmptyListIfAccountClaimsOrAccountDoesNotExist()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetClaimsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            List<Claim> accountDoesNotExistClaims = (await _dapperAccountRepository.GetClaimsAsync(0)).ToList<Claim>();
            List<Claim> noAccountClaimsClaims = (await _dapperAccountRepository.GetClaimsAsync(account.AccountId)).ToList<Claim>();

            // Assert
            Assert.NotEqual(null, accountDoesNotExistClaims);
            Assert.Equal(0, accountDoesNotExistClaims.Count);
            Assert.NotEqual(null, noAccountClaimsClaims);
            Assert.Equal(0, noAccountClaimsClaims.Count);
        }

        [Fact]
        public async Task CheckEmailInUseAsync_ReturnsTrueIfEmailIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            await CreateAccount(_testEmail1, _testPasswordHash);

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
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            await _dapperAccountRepository.UpdateDisplayNameAsync(account.AccountId, _testDisplayName);

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
        #endregion

        #region Delete
        [Theory]
        [MemberData(nameof(DeleteAccountAsync_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task DeleteAccountAsync_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            DeleteResult result = await _dapperAccountRepository.
                DeleteAsync(account.AccountId, rowVersionNull ? null : account.RowVersion);

            // Assert
            Assert.True(result.Succeeded);
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Null(account);
        }

        public static IEnumerable<object[]> DeleteAccountAsync_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task DeleteAccountAsync_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                SaveEmailAsync(account.AccountId, _testEmail2, account.RowVersion);
            DeleteResult result = await _dapperAccountRepository.
                DeleteAsync(account.AccountId, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task DeleteAccountAsync_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            DeleteResult result = await _dapperAccountRepository.
                DeleteAsync(_invalidAccountId, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task DeleteAccountRoleAsync_DeletesAccountRole()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            await _dapperAccountRepository.AddRoleAsync(account.AccountId, role.RoleId);

            // Act
            await _dapperAccountRepository.DeleteRoleAsync(account.AccountId, role.RoleId);

            // Assert
            List<Role> roles = (await _dapperAccountRepository.GetRolesAsync(role.RoleId)).ToList<Role>();
            Assert.Equal(0, roles.Count);
        }

        [Fact]
        public async Task DeleteAccountRoleAsync_ReturnsTrueIfSuccessful()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            await _dapperAccountRepository.AddRoleAsync(account.AccountId, role.RoleId);

            // Act
            bool result = await _dapperAccountRepository.DeleteRoleAsync(account.AccountId, role.RoleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAccountRoleAsync_ReturnsFalseIfUnsuccessful()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            bool result = await _dapperAccountRepository.DeleteRoleAsync(0, 0) ||
                await _dapperAccountRepository.DeleteRoleAsync(account.AccountId, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAccountClaimAsync_DeletesAccountClaim()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim.ClaimId);

            // Act
            await _dapperAccountRepository.DeleteClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            List<Claim> claims = (await _dapperAccountRepository.GetClaimsAsync(claim.ClaimId)).ToList<Claim>();
            Assert.Equal(0, claims.Count);
        }

        [Fact]
        public async Task DeleteAccountClaimAsync_ReturnsTrueIfSuccessful()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim.ClaimId);

            // Act
            bool result = await _dapperAccountRepository.DeleteClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAccountClaimAsync_ReturnsFalseIfUnsuccessful()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            bool result = await _dapperAccountRepository.DeleteClaimAsync(0, 0) ||
                await _dapperAccountRepository.DeleteClaimAsync(account.AccountId, 0);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region Update
        public async Task AddAccountRoleAsync_AddsAccountRole()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.AddRoleAsync(account.AccountId, role.RoleId);

            // Assert
            Role retrievedRole = (await _dapperAccountRepository.GetRolesAsync(account.AccountId)).First();
            Assert.Equal(1, retrievedRole.RoleId);
            Assert.Equal(_testRoleName1, retrievedRole.Name);
        }

        [Fact]
        public async Task AddAccountRoleAsync_ReturnsTrueIfSuccessful()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            bool result = await _dapperAccountRepository.AddRoleAsync(account.AccountId, role.RoleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddAccountRoleAsync_ThrowsExceptionIfAccountRoleAlreadyExists()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            Role role = await _dapperRoleRepository.CreateRoleAsync(_testRoleName1);

            // Act
            await _dapperAccountRepository.AddRoleAsync(account.AccountId, role.RoleId);
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddRoleAsync(account.AccountId, role.RoleId));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account already has role.", sqlException.Message);
        }

        [Fact]
        public async Task AddRoleAccountAsync_ThrowsExceptionIfAccountOrRoleDoesNotExist()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();

            // Act
            SqlException sqlException = await Assert.
                ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddRoleAsync(1, 1));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account or role does not exist.", sqlException.Message);
        }

        [Fact]
        public async Task AddAccountClaimAsync_AddsAccountClaim()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            Claim retrievedClaim = (await _dapperAccountRepository.GetClaimsAsync(account.AccountId)).First();
            Assert.Equal(1, retrievedClaim.ClaimId);
            Assert.Equal(_testClaimType1, retrievedClaim.Type);
            Assert.Equal(_testClaimValue1, retrievedClaim.Value);
        }

        [Fact]
        public async Task AddAccountClaimAsync_ReturnsTrueIfSuccessful()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            bool result = await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddAccountClaimAsync_ThrowsExceptionIfAccountClaimAlreadyExists()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);
            Claim claim = await _dapperClaimRepository.CreateClaimAsync(_testClaimType1, _testClaimValue1);

            // Act
            await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim.ClaimId);
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddClaimAsync(account.AccountId, claim.ClaimId));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account already has claim.", sqlException.Message);
        }

        [Fact]
        public async Task AddAccountClaimAsync_ThrowsExceptionIfAccountOrClaimDoesNotExist()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();

            // Act
            SqlException sqlException = await Assert.
                ThrowsAsync<SqlException>(async () => await _dapperAccountRepository.AddClaimAsync(1, 1));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Account or claim does not exist.", sqlException.Message);
        }

        [Theory]
        [MemberData(nameof(UpdateDisplayName_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task UpdateDisplayName_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateDisplayNameAsync(account.AccountId, _testDisplayName, rowVersionNull? null : account.RowVersion);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testDisplayName, account.DisplayName);
            Assert.True(result.Succeeded);
        }

        public static IEnumerable<object[]> UpdateDisplayName_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task UpdateDisplayName_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                UpdateDisplayNameAsync(account.AccountId, _testDisplayName, account.RowVersion);
            UpdateResult result = await _dapperAccountRepository.
                UpdateDisplayNameAsync(account.AccountId, _testDisplayName, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateDisplayName_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateDisplayNameAsync(_invalidAccountId, _testDisplayName, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateDisplayName_ReturnsUpdateAsyncResultsDuplicateRowIfDisplayNameIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            Account account1 = await CreateAccount(_testEmail1, _testPasswordHash);
            Account account2 = await CreateAccount(_testEmail2, _testPasswordHash);
            await _dapperAccountRepository.
                UpdateDisplayNameAsync(account2.AccountId, _testDisplayName, account2.RowVersion);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateDisplayNameAsync(account1.AccountId, _testDisplayName, account1.RowVersion);

            // Assert
            Assert.True(result.DuplicateRow);
        }

        [Theory]
        [MemberData(nameof(UpdateAltEmail_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task UpdateAltEmail_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateAltEmailAsync(account.AccountId, _testAltEmail, rowVersionNull ? null : account.RowVersion);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testAltEmail, account.AltEmail);
            Assert.True(result.Succeeded);
        }

        public static IEnumerable<object[]> UpdateAltEmail_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task UpdateAltEmail_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                UpdateAltEmailAsync(account.AccountId, _testAltEmail, account.RowVersion);
            UpdateResult result = await _dapperAccountRepository.
                UpdateAltEmailAsync(account.AccountId, _testAltEmail, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateAltEmail_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateAltEmailAsync(_invalidAccountId, _testAltEmail, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Theory]
        [MemberData(nameof(UpdateEmailVerified_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task UpdateEmailVerified_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateEmailVerifiedAsync(account.AccountId, _testEmailVerified, rowVersionNull ? null : account.RowVersion);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testEmailVerified, account.EmailVerified);
            Assert.True(result.Succeeded);
        }

        public static IEnumerable<object[]> UpdateEmailVerified_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task UpdateEmailVerified_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                UpdateEmailVerifiedAsync(account.AccountId, _testEmailVerified, account.RowVersion);
            UpdateResult result = await _dapperAccountRepository.
                UpdateEmailVerifiedAsync(account.AccountId, _testEmailVerified, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateEmailVerified_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateEmailVerifiedAsync(_invalidAccountId, _testEmailVerified, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Theory]
        [MemberData(nameof(UpdateTwoFactorEnabled_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task UpdateTwoFactorEnabled_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateTwoFactorEnabledAsync(account.AccountId, _testTwoFactorEnabled, rowVersionNull ? null : account.RowVersion);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testTwoFactorEnabled, account.TwoFactorEnabled);
            Assert.True(result.Succeeded);
        }

        public static IEnumerable<object[]> UpdateTwoFactorEnabled_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task UpdateTwoFactorEnabled_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                UpdateTwoFactorEnabledAsync(account.AccountId, _testTwoFactorEnabled, account.RowVersion);
            UpdateResult result = await _dapperAccountRepository.
                UpdateTwoFactorEnabledAsync(account.AccountId, _testTwoFactorEnabled, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateTwoFactorEnabled_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateTwoFactorEnabledAsync(_invalidAccountId, _testTwoFactorEnabled, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Theory]
        [MemberData(nameof(UpdatePasswordHash_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task UpdatePasswordHash_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdatePasswordHashAsync(account.AccountId, _testPasswordHash, _testPasswordLastChanged, _testSecurityStamp, rowVersionNull ? null : account.RowVersion);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testPasswordHash, account.PasswordHash);
            Assert.Equal(_testPasswordLastChanged, account.PasswordLastChanged);
            Assert.Equal(_testSecurityStamp, account.SecurityStamp);
            Assert.True(result.Succeeded);
        }

        public static IEnumerable<object[]> UpdatePasswordHash_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task UpdatePasswordHash_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                UpdatePasswordHashAsync(account.AccountId, _testPasswordHash, _testPasswordLastChanged, _testSecurityStamp, account.RowVersion);
            UpdateResult result = await _dapperAccountRepository.
                UpdatePasswordHashAsync(account.AccountId, _testPasswordHash, _testPasswordLastChanged, _testSecurityStamp, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdatePasswordHash_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdatePasswordHashAsync(_invalidAccountId, _testPasswordHash, _testPasswordLastChanged, _testSecurityStamp, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Theory]
        [MemberData(nameof(UpdateEmail_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task UpdateEmail_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateEmailResult result = await _dapperAccountRepository.
                SaveEmailAsync(account.AccountId, _testEmail1, rowVersionNull ? null : account.RowVersion);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testEmail1, account.Email);
            Assert.True(result.Succeeded);
        }

        public static IEnumerable<object[]> UpdateEmail_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task UpdateEmail_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                SaveEmailAsync(account.AccountId, _testEmail1, account.RowVersion);
            UpdateEmailResult result = await _dapperAccountRepository.
                SaveEmailAsync(account.AccountId, _testEmail1, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateEmail_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateEmailResult result = await _dapperAccountRepository.
                SaveEmailAsync(_invalidAccountId, _testEmail1, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateEmail_ReturnsUpdateAsyncResultsDuplicateRowIfEmailIsInUse()
        {
            // Arrange
            await _resetAccountsTable();
            Account account1 = await CreateAccount(_testEmail1, _testPasswordHash);
            Account account2 = await CreateAccount(_testEmail2, _testPasswordHash);
            await _dapperAccountRepository.
                SaveEmailAsync(account2.AccountId, _testEmail3, account2.RowVersion);

            // Act
            UpdateEmailResult result = await _dapperAccountRepository.
                SaveEmailAsync(account1.AccountId, _testEmail3, account1.RowVersion);

            // Assert
            Assert.True(result.DuplicateRow);
        }

        [Theory]
        [MemberData(nameof(UpdateAltEmailVerified_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData))]
        public async Task UpdateAltEmailVerified_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData(bool rowVersionNull)
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateAltEmailVerifiedAsync(account.AccountId, _testAltEmailVerified, rowVersionNull ? null : account.RowVersion);

            // Assert
            account = await _dapperAccountRepository.GetAccountAsync(account.AccountId);
            Assert.Equal(_testAltEmailVerified, account.AltEmailVerified);
            Assert.True(result.Succeeded);
        }

        public static IEnumerable<object[]> UpdateAltEmailVerified_ReturnsUpdateAsyncResultsSuccessIfSuccessfulData()
        {
            yield return new object[] { false };
            yield return new object[] { true };
        }

        [Fact]
        public async Task UpdateAltEmailVerified_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfRowVersionIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            await _dapperAccountRepository.
                UpdateAltEmailVerifiedAsync(account.AccountId, _testAltEmailVerified, account.RowVersion);
            UpdateResult result = await _dapperAccountRepository.
                UpdateAltEmailVerifiedAsync(account.AccountId, _testAltEmailVerified, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }

        [Fact]
        public async Task UpdateAltEmailVerified_ReturnsUpdateAsyncResultsInvalidRowVersionOrAccountIdIfAccountIdIsInvalid()
        {
            // Arrange
            await _resetAccountsTable();
            Account account = await CreateAccount(_testEmail1, _testPasswordHash);

            // Act
            UpdateResult result = await _dapperAccountRepository.
                UpdateAltEmailVerifiedAsync(_invalidAccountId, _testAltEmailVerified, account.RowVersion);

            // Assert
            Assert.True(result.InvalidRowVersionOrAccountId);
        }
        #endregion

        #region Helpers
        private async Task<Account> CreateAccount(string email, string passwordHash, 
            DateTimeOffset passwordLastChanged = default(DateTimeOffset),
            Guid securityStamp = default(Guid))
        {
            return (await _dapperAccountRepository.CreateAsync(email, passwordHash, 
                passwordLastChanged == default(DateTimeOffset) ? _testPasswordLastChanged : passwordLastChanged, 
                securityStamp == default(Guid) ? _testSecurityStamp : securityStamp)).Account;
        }

        private void VerifyAccount(Account account, int accountId, string email, string displayName, Guid securityStamp,
            bool emailVerified, string altEmail, bool altEmailVerified, bool twoFactorEnabled,
            string passwordHash, DateTimeOffset passwordLastChanged)
        {
            Assert.Equal(accountId, account.AccountId);
            Assert.Equal(email, account.Email);
            Assert.Equal(displayName, account.DisplayName);
            Assert.Equal(securityStamp, account.SecurityStamp);
            Assert.Equal(emailVerified, account.EmailVerified);
            Assert.Equal(altEmail, account.AltEmail);
            Assert.Equal(altEmailVerified, account.AltEmailVerified);
            Assert.Equal(twoFactorEnabled, account.TwoFactorEnabled);
            Assert.Equal(_testPasswordHash, account.PasswordHash);
            Assert.Equal(passwordLastChanged, account.PasswordLastChanged);
            // Server generated 
            Assert.NotNull(account.RowVersion);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Data;
using Dapper;

namespace Jering.Vak.DatabaseInterface.Test
{
    [Collection("DatabaseCollection")]
    public class AccountRepositoryTests
    {
        private DatabaseFixture _databaseFixture { get; }
        private SqlConnection _sqlConnection { get; }
        private RoleRepository _roleRepository { get; }
        private ClaimRepository _claimRepository { get; }
        private AccountRepository _accountRepository { get; }
        private Func<Task> _resetClaimsTable { get; }
        private Func<Task> _resetRolesTable { get; }
        private Func<Task> _resetAccountsTable { get; }

        public AccountRepositoryTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _sqlConnection = _databaseFixture.SqlConnection;
            _roleRepository = databaseFixture.RoleRepository;
            _claimRepository = databaseFixture.ClaimRepository;
            _accountRepository = databaseFixture.AccountRepository;
            _resetClaimsTable = _databaseFixture.ResetClaimsTable;
            _resetRolesTable = _databaseFixture.ResetRolesTable;
            _resetAccountsTable = _databaseFixture.ResetAccountsTable;
        }

        [Fact]
        public async Task CreateAccountAsyncTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");

            // Assert
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.Unicode.GetBytes("$PassworD" + "Email@Jering.com"));
            Assert.Equal(1, account.AccountId);
            Assert.Equal("Email@Jering.com", account.Email);
            Assert.Equal(hash, account.PasswordHash);
            Assert.Equal(null, account.Username);
            Assert.Equal(null, account.SecurityStamp);
            Assert.Equal(false, account.EmailConfirmed);
            Assert.Equal(false, account.TwoFactorEnabled);
        }

        [Fact]
        public async Task CreateAccountAsync_UniqueEmailTest()
        {
            // Arrange
            await _resetAccountsTable();

            // Act
            await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD"));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("An account with this email already exists.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteAccountAsyncTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetAccountsTable();
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");

            // Act
            await _accountRepository.DeleteAccountAsync(account.AccountId);

            // Assert
            Assert.Equal(0, await _sqlConnection.ExecuteScalarAsync<int>("Select Count(*) from [dbo].[Accounts]", commandType: CommandType.Text));
        }

        [Fact]
        public async Task AddAccountRoleAsyncTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Role role = await _roleRepository.CreateRoleAsync("Name1");
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");

            // Act
            await _accountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);

            // Assert
            Role retrievedRole = (await _accountRepository.GetAccountRolesAsync(account.AccountId)).First();
            Assert.Equal(1, retrievedRole.RoleId);
            Assert.Equal("Name1", retrievedRole.Name);
        }

        [Fact]
        public async Task DeleteAccountRoleAsyncTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetAccountsTable();
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");
            Role role = await _roleRepository.CreateRoleAsync("Name1");
            await _accountRepository.AddAccountRoleAsync(account.AccountId, role.RoleId);

            // Act
            await _accountRepository.DeleteAccountRoleAsync(account.AccountId, role.RoleId);

            // Assert
            List<Role> roles = (await _accountRepository.GetAccountRolesAsync(role.RoleId)).ToList<Role>();
            Assert.Equal(0, roles.Count);
        }

        [Fact]
        public async Task GetAccountRolesAsyncTest()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetRolesTable();
            Role role1 = await _roleRepository.CreateRoleAsync("Name1");
            Role role2 = await _roleRepository.CreateRoleAsync("Name2");
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");
            await _accountRepository.AddAccountRoleAsync(account.AccountId, role1.RoleId);
            await _accountRepository.AddAccountRoleAsync(account.AccountId, role2.RoleId);

            // Act
            List<Role> roles = (await _accountRepository.GetAccountRolesAsync(account.AccountId)).ToList<Role>();

            // Assert
            Assert.Equal("Name1", roles[0].Name);
            Assert.Equal("Name2", roles[1].Name);
        }

        [Fact]
        public async Task AddAccountClaimAsyncTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");

            // Act
            await _accountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            Claim retrievedClaim = (await _accountRepository.GetAccountClaimsAsync(account.AccountId)).First();
            Assert.Equal(1, retrievedClaim.ClaimId);
            Assert.Equal("Type1", retrievedClaim.Type);
            Assert.Equal("Value1", retrievedClaim.Value);
        }

        [Fact]
        public async Task DeleteAccountClaimAsyncTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetAccountsTable();
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            await _accountRepository.AddAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Act
            await _accountRepository.DeleteAccountClaimAsync(account.AccountId, claim.ClaimId);

            // Assert
            List<Claim> claims = (await _accountRepository.GetAccountClaimsAsync(claim.ClaimId)).ToList<Claim>();
            Assert.Equal(0, claims.Count);
        }

        [Fact]
        public async Task GetAccountClaimsAsyncTest()
        {
            // Arrange
            await _resetAccountsTable();
            await _resetClaimsTable();
            Claim claim1 = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Claim claim2 = await _claimRepository.CreateClaimAsync("Type2", "Value2");
            Account account = await _accountRepository.CreateAccountAsync("Email@Jering.com", "$PassworD");
            await _accountRepository.AddAccountClaimAsync(account.AccountId, claim1.ClaimId);
            await _accountRepository.AddAccountClaimAsync(account.AccountId, claim2.ClaimId);

            // Act
            List<Claim> claims = (await _accountRepository.GetAccountClaimsAsync(account.AccountId)).ToList<Claim>();

            // Assert
            Assert.Equal("Type1", claims[0].Type);
            Assert.Equal("Value1", claims[0].Value);
            Assert.Equal("Type2", claims[1].Type);
            Assert.Equal("Value2", claims[1].Value);
        }
    }
}

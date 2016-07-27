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
    public class RoleRepositoryTests 
    {
        private DatabaseFixture _databaseFixture { get; }
        private SqlConnection _sqlConnection { get; }
        private RoleRepository _roleRepository { get; }
        private ClaimRepository _claimRepository { get; }
        private Func<Task> _resetClaimsTable { get; }
        private Func<Task> _resetRolesTable { get; }

        public RoleRepositoryTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _sqlConnection = _databaseFixture.SqlConnection;       
            _roleRepository = databaseFixture.RoleRepository;
            _claimRepository = databaseFixture.ClaimRepository;
            _resetClaimsTable = _databaseFixture.ResetClaimsTable;
            _resetRolesTable = _databaseFixture.ResetRolesTable;
        }

        [Fact]
        public async Task CreateRoleAsyncTest()
        {
            // Arrange
            await _resetRolesTable();

            // Act
            Role role = await _roleRepository.CreateRoleAsync("Name1");

            // Assert
            Assert.Equal(1, role.RoleId);
            Assert.Equal("Name1", role.Name);
        }

        [Fact]
        public async Task CreateRoleAsync_UniqueNameTest()
        {
            // Arrange
            await _resetRolesTable();

            // Act
            await _roleRepository.CreateRoleAsync("Name1");
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _roleRepository.CreateRoleAsync("Name1"));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Role already exists.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteRoleAsyncTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetRolesTable();
            Role role = await _roleRepository.CreateRoleAsync("Name1");

            // Act
            await _roleRepository.DeleteRoleAsync(role.RoleId);

            // Assert
            Assert.Equal(0, await _sqlConnection.ExecuteScalarAsync<int>("Select Count(*) from [dbo].[Roles]", commandType: CommandType.Text));
        }

        [Fact]
        public async Task AddRoleClaimAsyncTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _roleRepository.CreateRoleAsync("Name1");

            // Act
            await _roleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Assert
            Claim retrievedClaim = (await _roleRepository.GetRoleClaimsAsync(role.RoleId)).First();
            Assert.Equal(1, retrievedClaim.ClaimId);
            Assert.Equal("Type1", retrievedClaim.Type);
            Assert.Equal("Value1", retrievedClaim.Value);
        }

        [Fact]
        public async Task AddRoleClaimAsync_UniqueClaimTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _roleRepository.CreateRoleAsync("Name1");

            // Act
            await _roleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _roleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Role already has claim.", sqlException.Message);
        }

        [Fact]
        public async Task AddRoleClaimAsync_RoleAndClaimExistTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();

            // Act
            SqlException sqlException = await Assert.
                ThrowsAsync<SqlException>(async () => await _roleRepository.AddRoleClaimAsync(1, 1));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Role or claim does not exist.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteRoleClaimAsyncTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _roleRepository.CreateRoleAsync("Name1");
            await _roleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Act
            await _roleRepository.DeleteRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Assert
            List<Claim> claims = (await _roleRepository.GetRoleClaimsAsync(role.RoleId)).ToList<Claim>();
            Assert.Equal(0, claims.Count);
        }

        [Fact]
        public async Task GetRoleClaimsAsyncTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim1 = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Claim claim2 = await _claimRepository.CreateClaimAsync("Type2", "Value2");
            Role role = await _roleRepository.CreateRoleAsync("Name1");
            await _roleRepository.AddRoleClaimAsync(role.RoleId, claim1.ClaimId);
            await _roleRepository.AddRoleClaimAsync(role.RoleId, claim2.ClaimId);

            // Act
            List<Claim> claims = (await _roleRepository.GetRoleClaimsAsync(role.RoleId)).ToList<Claim>();

            // Assert
            Assert.Equal("Type1", claims[0].Type);
            Assert.Equal("Value1", claims[0].Value);
            Assert.Equal("Type2", claims[1].Type);
            Assert.Equal("Value2", claims[1].Value);
        }
    }
}

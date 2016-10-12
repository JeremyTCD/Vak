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
using Jering.VectorArtKit.WebApi.BusinessModel;
using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.DatabaseInterface.Dapper.Tests.IntegrationTests
{
    [Collection("DapperDatabaseCollection")]
    public class DapperRoleRepositoryTests
    {
        private SqlConnection _sqlConnection { get; }
        private DapperRoleRepository _dapperRoleRepository { get; }
        private DapperClaimRepository _dapperClaimRepository { get; }
        private Func<Task> _resetClaimsTable { get; }
        private Func<Task> _resetRolesTable { get; }

        public DapperRoleRepositoryTests(DapperDatabaseFixture databaseFixture)
        {
            _sqlConnection = databaseFixture.SqlConnection;
            _dapperRoleRepository = databaseFixture.DapperRoleRepository;
            _dapperClaimRepository = databaseFixture.DapperClaimRepository;
            _resetClaimsTable = databaseFixture.ResetClaimsTable;
            _resetRolesTable = databaseFixture.ResetRolesTable;
        }

        [Fact]
        public async Task CreateRoleAsync_ReturnsRoleIfSuccessfulTest()
        {
            // Arrange
            await _resetRolesTable();

            // Act
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");

            // Assert
            Assert.Equal(1, role.RoleId);
            Assert.Equal("Name1", role.Name);
        }

        [Fact]
        public async Task CreateRoleAsync_ThrowsExceptionIfRoleAlreadyExistsTest()
        {
            // Arrange
            await _resetRolesTable();

            // Act
            await _dapperRoleRepository.CreateRoleAsync("Name1");
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperRoleRepository.CreateRoleAsync("Name1"));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Role already exists.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteRoleAsync_DeletesRoleTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetRolesTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");

            // Act
            await _dapperRoleRepository.DeleteRoleAsync(role.RoleId);

            // Assert
            Assert.Equal(0, await _sqlConnection.ExecuteScalarAsync<int>("Select Count(*) from [dbo].[Roles]", commandType: CommandType.Text));
        }

        [Fact]
        public async Task DeleteRoleAsync_ReturnsTrueIfSuccessfulTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetRolesTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");

            // Act
            bool result = await _dapperRoleRepository.DeleteRoleAsync(role.RoleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteRoleAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetRolesTable();

            // Act
            bool result = await _dapperRoleRepository.DeleteRoleAsync(0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddRoleClaimAsync_AddsRoleClaimTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");

            // Act
            await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Assert
            Claim retrievedClaim = (await _dapperRoleRepository.GetRoleClaimsAsync(role.RoleId)).First();
            Assert.Equal(1, retrievedClaim.ClaimId);
            Assert.Equal("Type1", retrievedClaim.Type);
            Assert.Equal("Value1", retrievedClaim.Value);
        }

        [Fact]
        public async Task AddRoleClaimAsync_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");

            // Act
            bool result = await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddRoleClaimAsync_ThrowsExceptionIfRoleClaimAlreadyExistsTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");

            // Act
            await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Role already has claim.", sqlException.Message);
        }

        [Fact]
        public async Task AddRoleClaimAsync_ThrowsExceptionIfRoleOrClaimDoesNotExistTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();

            // Act
            SqlException sqlException = await Assert.
                ThrowsAsync<SqlException>(async () => await _dapperRoleRepository.AddRoleClaimAsync(1, 1));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Role or claim does not exist.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteRoleClaimAsync_DeletesRoleClaimTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");
            await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Act
            await _dapperRoleRepository.DeleteRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Assert
            List<Claim> claims = (await _dapperRoleRepository.GetRoleClaimsAsync(role.RoleId)).ToList<Claim>();
            Assert.Equal(0, claims.Count);
        }

        [Fact]
        public async Task DeleteRoleClaimAsync_ReturnsTrueIfSuccessfulTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");
            await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Act
            bool result = await _dapperRoleRepository.DeleteRoleClaimAsync(role.RoleId, claim.ClaimId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteRoleClaimAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();

            // Act
            bool result = await _dapperRoleRepository.DeleteRoleClaimAsync(0, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetRoleClaimsAsync_GetsRoleClaimsTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Claim claim1 = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");
            Claim claim2 = await _dapperClaimRepository.CreateClaimAsync("Type2", "Value2");
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");
            await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim1.ClaimId);
            await _dapperRoleRepository.AddRoleClaimAsync(role.RoleId, claim2.ClaimId);

            // Act
            List<Claim> claims = (await _dapperRoleRepository.GetRoleClaimsAsync(role.RoleId)).ToList<Claim>();

            // Assert
            Assert.Equal("Type1", claims[0].Type);
            Assert.Equal("Value1", claims[0].Value);
            Assert.Equal("Type2", claims[1].Type);
            Assert.Equal("Value2", claims[1].Value);
        }

        [Fact]
        public async Task GetRoleClaimsAsync_ReturnsEmptyListIfClaimsOrRoleDoesNotExist()
        {
            // Arrange
            await _resetRolesTable();
            await _resetClaimsTable();
            Role role = await _dapperRoleRepository.CreateRoleAsync("Name1");

            // Act
            List<Claim> roleDoesNotExistClaims = (await _dapperRoleRepository.GetRoleClaimsAsync(0)).ToList<Claim>();
            List<Claim> claimsDoNotExistClaims = (await _dapperRoleRepository.GetRoleClaimsAsync(role.RoleId)).ToList<Claim>();

            // Assert
            Assert.NotEqual(null, roleDoesNotExistClaims);
            Assert.Equal(0, roleDoesNotExistClaims.Count);
            Assert.NotEqual(null, claimsDoNotExistClaims);
            Assert.Equal(0, claimsDoNotExistClaims.Count);
        }
    }
}

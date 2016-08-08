using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data;
using Dapper;
using System;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.DatabaseInterface.Dapper.IntegrationTests
{
    [Collection("DapperDatabaseCollection")]
    public class DapperClaimRepositoryTests
    {
        private SqlConnection _sqlConnection { get; }
        private DapperClaimRepository _dapperClaimRepository { get; }
        private Func<Task> _resetClaimsTable { get; }

        public DapperClaimRepositoryTests(DapperDatabaseFixture databaseFixture)
        {
            _sqlConnection = databaseFixture.SqlConnection;
            _dapperClaimRepository = databaseFixture.DapperClaimRepository;
            _resetClaimsTable = databaseFixture.ResetClaimsTable;
        }

        [Fact]
        public async Task CreateClaimAsync_ReturnsClaimIfSuccessfulTest()
        {
            // Arrange
            await _resetClaimsTable();

            // Act
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");

            // Assert
            Assert.Equal(1, claim.ClaimId);
            Assert.Equal("Type1", claim.Type);
            Assert.Equal("Value1", claim.Value);
        }

        [Fact]
        public async Task CreateClaimAsync_ThrowsExceptionIfClaimAlreadyExistsTest()
        {
            // Arrange
            await _resetClaimsTable();

            // Act
            await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1"));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("Claim already exists.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteClaimAsync_DeletesClaimTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetClaimsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");

            // Act
            await _dapperClaimRepository.DeleteClaimAsync(claim.ClaimId);

            // Assert
            Assert.Equal(0, await _sqlConnection.ExecuteScalarAsync<int>("Select Count(*) from [dbo].[Claims]", commandType: CommandType.Text));
        }

        [Fact]
        public async Task DeleteClaimAsync_ReturnsTrueIfSuccessfulTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetClaimsTable();
            Claim claim = await _dapperClaimRepository.CreateClaimAsync("Type1", "Value1");

            // Act
            bool result = await _dapperClaimRepository.DeleteClaimAsync(claim.ClaimId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteClaimAsync_ReturnsFalseIfUnsuccessfulTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetClaimsTable();

            // Act
            bool result = await _dapperClaimRepository.DeleteClaimAsync(0);

            // Assert
            Assert.False(result);
        }
    }
}

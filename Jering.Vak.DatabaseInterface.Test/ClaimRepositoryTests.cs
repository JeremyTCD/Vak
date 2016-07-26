using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data;
using Dapper;
using System;

namespace Jering.Vak.DatabaseInterface.Test
{
    [Collection("DatabaseCollection")]
    public class ClaimRepositoryTests
    {
        private DatabaseFixture _databaseFixture { get; }
        private SqlConnection _sqlConnection { get; }
        private ClaimRepository _claimRepository { get; }
        private Func<Task> _resetClaimsTable { get; }

        public ClaimRepositoryTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _sqlConnection = _databaseFixture.SqlConnection;
            _claimRepository = databaseFixture.ClaimRepository;
            _resetClaimsTable = _databaseFixture.ResetClaimsTable;
        }

        [Fact]
        public async Task CreateClaimAsyncTest()
        {
            // Arrange
            await _resetClaimsTable();

            // Act
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");

            // Assert
            Assert.Equal(1, claim.ClaimId);
            Assert.Equal("Type1", claim.Type);
            Assert.Equal("Value1", claim.Value);
        }

        [Fact]
        public async Task CreateClaimAsync_UniqueTypeAndValueTest()
        {
            // Arrange
            await _resetClaimsTable();

            // Act
            await _claimRepository.CreateClaimAsync("Type1", "Value1");
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _claimRepository.CreateClaimAsync("Type1", "Value1"));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("A claim with type \"Type1\" and value \"Value1\" already exists.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteClaimAsyncTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetClaimsTable();
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");

            // Act
            await _claimRepository.DeleteClaimAsync(claim.ClaimId);

            // Assert
            Assert.Equal(0, await _sqlConnection.ExecuteScalarAsync<int>("Select Count(*) from [dbo].[Claims]", commandType: CommandType.Text));
        }
    }
}

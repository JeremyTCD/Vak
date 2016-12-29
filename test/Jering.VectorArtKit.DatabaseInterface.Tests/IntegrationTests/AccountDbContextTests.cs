using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Jering.VectorArtKit.DatabaseInterface;

namespace Jering.VectorArtKit.DatabaseInterface.Tests.IntegrationTests
{
    [Collection("DatabaseCollection")]
    public class VakDbContextTests : IDisposable
    {
        private DatabaseFixture _dbFixture { get; }
        private VakDbContext _dbContext { get; }
        private Func<Task> _resetAccountsTable { get; }

        private const string _testEmail1 = "test@email1.com";
        private const string _testEmail2 = "test@email2.com";
        private const string _testPasswordHash = "testPasswordHash";
        private const string _testDisplayName = "testDisplayName";
        private DateTimeOffset _testPasswordLastChanged;
        private Guid _testSecurityStamp;

        public VakDbContextTests(DatabaseFixture dbFixture)
        {
            _dbFixture = dbFixture;
            _dbContext =  new VakDbContext(dbFixture.DbContextOptions);
            dbFixture.ResetAccountsTable(_dbContext);

            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            _testPasswordLastChanged = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 
                utcNow.Second, utcNow.Offset);
            _testSecurityStamp = Guid.NewGuid();
        }

        #region Model Configuration
        [Fact]
        public async Task VakDbContext_AccountAccountIdValueGeneratedOnAdd()
        {
            // Arrange
            VakAccount account1 = new VakAccount
            {
                Email = _testEmail1,
                PasswordHash = _testPasswordHash,
                PasswordLastChanged = _testPasswordLastChanged,
                SecurityStamp = _testSecurityStamp
            };
            VakAccount account2 = new VakAccount
            {
                Email = _testEmail2,
                PasswordHash = _testPasswordHash,
                PasswordLastChanged = _testPasswordLastChanged,
                SecurityStamp = _testSecurityStamp
            };

            // Act 
            await _dbContext.AddAsync(account1);
            await _dbContext.SaveChangesAsync();
            await _dbContext.AddAsync(account2);
            await _dbContext.SaveChangesAsync();

            // Assert
            Assert.Equal(1, account1.AccountId);
            Assert.Equal(2, account2.AccountId);
        }

        [Fact]
        public async Task VakDbContext_AccountRowVersionValueGeneratedOnAddOrUpdate()
        {
            // Arrange
            VakAccount account = new VakAccount
            {
                Email = _testEmail1,
                PasswordHash = _testPasswordHash,
                PasswordLastChanged = _testPasswordLastChanged,
                SecurityStamp = _testSecurityStamp
            };

            // Act 
            await _dbContext.AddAsync(account);
            await _dbContext.SaveChangesAsync();
            byte[] initialRowVersion = account.RowVersion;

            account.DisplayName = _testDisplayName;
            await _dbContext.SaveChangesAsync();
            byte[] finalRowVersion = account.RowVersion;

            // Assert
            Assert.NotEqual(initialRowVersion, finalRowVersion);
        }

        [Fact]
        public async Task VakDbContext_AccountRowVersionIsConcurrencyToken()
        {
            // Arrange
            VakAccount account = new VakAccount
            {
                Email = _testEmail1,
                PasswordHash = _testPasswordHash,
                PasswordLastChanged = _testPasswordLastChanged,
                SecurityStamp = _testSecurityStamp
            };
            VakDbContext dbContext2 = new VakDbContext(_dbFixture.DbContextOptions);

            // Act 
            await _dbContext.AddAsync(account);
            await _dbContext.SaveChangesAsync();

            VakAccount account2 = await dbContext2.Accounts.FirstOrDefaultAsync(a => a.AccountId == account.AccountId);
            account2.DisplayName = _testDisplayName;
            await dbContext2.SaveChangesAsync();

            account.DisplayName = _testDisplayName;
            
            // Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await _dbContext.SaveChangesAsync());
        }
        #endregion

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}

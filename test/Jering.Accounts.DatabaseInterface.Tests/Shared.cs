using Microsoft.Extensions.Configuration;
using System.IO;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Jering.VectorArtKit.DatabaseInterface;

namespace Jering.VectorArtKit.DatabaseInterface.Tests.IntegrationTests
{
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {

    }

    public class DatabaseFixture
    {
        public IConfigurationRoot ConfigurationRoot { get; }
        public DbContextOptions<VakDbContext> DbContextOptions { get; }

        public DatabaseFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("project.json");
            ConfigurationRoot = configurationBuilder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<VakDbContext>();
            optionsBuilder.UseSqlServer(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);

            DbContextOptions = optionsBuilder.Options;
        }

        //public async Task ResetClaimsTable()
        //{
        //    await DbContext.ExecuteAsync("Delete from [dbo].[Claims];" +
        //        "DBCC CHECKIDENT('[dbo].[Claims]', RESEED, 0);", commandType: CommandType.Text);
        //}

        //public async Task ResetRolesTable()
        //{
        //    await DbContext.ExecuteAsync("Delete from [dbo].[Roles];" +
        //        "DBCC CHECKIDENT('[dbo].[Roles]', RESEED, 0);", commandType: CommandType.Text);
        //}

        public void ResetAccountsTable(VakDbContext dbContext)
        {
            dbContext.Database.ExecuteSqlCommand("Delete from [dbo].[Accounts]; DBCC CHECKIDENT('[dbo].[Accounts]', RESEED, 0);");
        }
    }
}

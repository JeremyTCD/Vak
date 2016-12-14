using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Xunit;

namespace Jering.Accounts.DatabaseInterface.Dapper.Tests.IntegrationTests
{
    [CollectionDefinition("DapperDatabaseCollection")]
    public class DapperDatabaseCollection : ICollectionFixture<DapperDatabaseFixture>
    {

    }

    public class DapperDatabaseFixture : IDisposable 
    {
        public IConfigurationRoot ConfigurationRoot { get; }
        public SqlConnection SqlConnection { get; }
        public DapperRoleRepository DapperRoleRepository { get; }
        public DapperClaimRepository DapperClaimRepository { get; }
        public DapperAccountRepository<Account> DapperAccountRepository { get; }

        public DapperDatabaseFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("project.json");
            ConfigurationRoot = configurationBuilder.Build();

            SqlConnection = new SqlConnection(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);

            DapperRoleRepository = new DapperRoleRepository(SqlConnection);
            DapperClaimRepository = new DapperClaimRepository(SqlConnection);
            DapperAccountRepository = new DapperAccountRepository<Account>(SqlConnection);
        }

        public void Dispose()
        {
            SqlConnection.Dispose();
        }

        public async Task ResetClaimsTable()
        {
            await SqlConnection.ExecuteAsync("Delete from [dbo].[Claims];" +
                "DBCC CHECKIDENT('[dbo].[Claims]', RESEED, 0);", commandType: CommandType.Text);
        }

        public async Task ResetRolesTable()
        {
            await SqlConnection.ExecuteAsync("Delete from [dbo].[Roles];" +
                "DBCC CHECKIDENT('[dbo].[Roles]', RESEED, 0);", commandType: CommandType.Text);
        }

        public async Task ResetAccountsTable()
        {
            await SqlConnection.ExecuteAsync("Delete from [dbo].[Accounts];" +
                "DBCC CHECKIDENT('[dbo].[Accounts]', RESEED, 0);", commandType: CommandType.Text);
        }
    }
}

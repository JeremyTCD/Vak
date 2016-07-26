using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Xunit;

namespace Jering.Vak.DatabaseInterface.Test
{
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {

    }

    public class DatabaseFixture : IDisposable
    {
        public IConfigurationRoot ConfigurationRoot { get; }
        public SqlConnection SqlConnection { get; }
        public RoleRepository RoleRepository { get; }
        public ClaimRepository ClaimRepository { get; }
        public MemberRepository MemberRepository { get; }

        public DatabaseFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("project.json");
            ConfigurationRoot = configurationBuilder.Build();

            SqlConnection = new SqlConnection(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);

            RoleRepository = new RoleRepository(SqlConnection);
            ClaimRepository = new ClaimRepository(SqlConnection);
            MemberRepository = new MemberRepository(SqlConnection);
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

        public async Task ResetMembersTable()
        {
            await SqlConnection.ExecuteAsync("Delete from [dbo].[Members];" +
                "DBCC CHECKIDENT('[dbo].[Members]', RESEED, 0);", commandType: CommandType.Text);
        }
    }
}

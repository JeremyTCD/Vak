using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Dapper;
using System.Collections.Generic;
using System.Security.Claims;

namespace Jering.Vak.DatabaseInterface
{
    /// <summary>
    /// Provides a lightweight interface for creating, reading and updating accounts. Functions avoid reading and writing 
    /// contextually superflous information as much as possible. 
    /// </summary>
    public class AccountRepository
    {
        private SqlConnection _sqlConnection { get; }

        public AccountRepository(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        /// <summary>
        /// Creates a new Account in the Accounts table
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public async Task<Account> CreateAccountAsync(string email, string password)
        {
            return await _sqlConnection.QuerySingleAsync<Account>(@"[Website].[CreateAccount]",
                new
                {
                    Password = password,
                    Email = email
                },
                commandType: CommandType.StoredProcedure);
        }

        public Task DeleteAccountAsync(int accountId)
        {
            return _sqlConnection.ExecuteAsync(@"[Website].[DeleteAccount]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddAccountRoleAsync(int accountId, int roleId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[AddAccountRole]",
                new
                {
                    AccountId = accountId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteAccountRoleAsync(int accountId, int roleId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[DeleteAccountRole]",
                new
                {
                    AccountId = accountId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Gets a account's roles
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Role>> GetAccountRolesAsync(int accountId)
        {
            return await _sqlConnection.QueryAsync<Role>(@"[Website].[GetAccountRoles]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddAccountClaimAsync(int accountId, int claimId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[AddAccountClaim]",
                new
                {
                    AccountId = accountId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteAccountClaimAsync(int accountId, int claimId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[DeleteAccountClaim]",
                new
                {
                    AccountId = accountId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Claim>> GetAccountClaimsAsync(int accountId)
        {
            return await _sqlConnection.QueryAsync<Claim>(@"[Website].[GetAccountClaims]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}

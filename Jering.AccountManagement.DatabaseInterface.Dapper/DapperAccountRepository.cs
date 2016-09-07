using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Jering.AccountManagement.DatabaseInterface.Dapper
{
    /// <summary>
    /// Provides an interface for performing CRUD operations on account representations in a database. 
    /// Functions with varying granularity are provided to avoid reading and writing contextually superfluous 
    /// information while minimizing round trips.
    /// </summary>
    public class DapperAccountRepository<TAccount> : IAccountRepository<TAccount> 
        where TAccount : IAccount
    {
        /// <summary>
        /// 
        /// </summary>
        protected SqlConnection _sqlConnection { get; }

        /// <summary>
        /// Constructs an instance of <see cref="DapperAccountRepository{TAccount}"/>. 
        /// </summary>
        /// <param name="sqlConnection"></param>
        public DapperAccountRepository(SqlConnection sqlConnection)
        {
            if(sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            _sqlConnection = sqlConnection;
        }

        /// <summary>
        /// Creates an account with specified <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>Newly created account.</returns>
        public virtual async Task<TAccount> CreateAccountAsync(string email, string password)
        {
            return await _sqlConnection.QuerySingleAsync<TAccount>(@"[Website].[CreateAccount]",
                new
                {
                    Password = password,
                    Email = email
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Deletes account with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> DeleteAccountAsync(int accountId)
        {
            return (await _sqlConnection.QueryAsync<int>(@"[Website].[DeleteAccount]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure)).FirstOrDefault() > 0;
        }

        /// <summary>
        /// Gets account with specified <paramref name="accountId"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>account with specified <paramref name="accountId"/> if it exists, 
        /// null otherwise.</returns>
        public virtual async Task<TAccount> GetAccountAsync(int accountId)
        {
            return (await _sqlConnection.QueryAsync<TAccount>(@"[Website].[GetAccount]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /// <summary>
        /// Gets account with specified <paramref name="email"/> and <paramref name="password"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>account with specified <paramref name="email"/> and <paramref name="password"/> if it exists, 
        /// null otherwise.</returns>
        public virtual async Task<TAccount> GetAccountByEmailAndPasswordAsync(string email, string password)
        {
            return (await _sqlConnection.QueryAsync<TAccount>(@"[Website].[GetAccountByEmailAndPassword]",
                new
                {
                    Email = email,
                    Password = password
                },
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /// <summary>
        /// Gets account with specified <paramref name="email"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>account with specified <paramref name="email"/> if it exists, 
        /// null otherwise.</returns>
        public virtual async Task<TAccount> GetAccountByEmailAsync(string email)
        {
            return (await _sqlConnection.QueryAsync<TAccount>(@"[Website].[GetAccountByEmail]",
                new
                {
                    Email = email
                },
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /// <summary>
        /// Adds <see cref="Role"/> with specified <paramref name="roleId"/> to account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> AddAccountRoleAsync(int accountId, int roleId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[AddAccountRole]",
                new
                {
                    AccountId = accountId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Deletes <see cref="Role"/> with specified <paramref name="roleId"/> from account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> DeleteAccountRoleAsync(int accountId, int roleId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[DeleteAccountRole]",
                new
                {
                    AccountId = accountId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Gets <see cref="Role"/>s belonging to account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns><see cref="IEnumerable{Role}"/> containing <see cref="Role"/>s belonging to account with 
        /// specified <paramref name="accountId"/>.</returns>
        /// <remarks>Returns empty <see cref="IEnumerable{Role}"/> if account has no <see cref="Role"/>s 
        /// or account does not exist.</remarks>
        public virtual async Task<IEnumerable<Role>> GetAccountRolesAsync(int accountId)
        {
            return await _sqlConnection.QueryAsync<Role>(@"[Website].[GetAccountRoles]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Adds <see cref="Claim"/> with specified <paramref name="claimId"/> to account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="claimId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> AddAccountClaimAsync(int accountId, int claimId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[AddAccountClaim]",
                new
                {
                    AccountId = accountId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Deletes <see cref="Claim"/> with specified <paramref name="claimId"/> from account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="claimId"></param>
        /// <returns>True if successful, false othewise.</returns>
        public virtual async Task<bool> DeleteAccountClaimAsync(int accountId, int claimId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[DeleteAccountClaim]",
                new
                {
                    AccountId = accountId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Gets <see cref="Claim"/>s belonging to account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns><see cref="IEnumerable{Claim}"/> containing <see cref="Claim"/>s belonging to account with 
        /// specified <paramref name="accountId"/>.</returns>
        /// <remarks>Returns empty <see cref="IEnumerable{Claim}"/> if account has no <see cref="Role"/>s 
        /// or account does not exist.</remarks>
        public virtual async Task<IEnumerable<Claim>> GetAccountClaimsAsync(int accountId)
        {
            return await _sqlConnection.QueryAsync<Claim>(@"[Website].[GetAccountClaims]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Gets SecurityStamp of account with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>A <see cref="Guid"/>.</returns>
        /// <remarks>Returns a <see cref="Guid"/> with value <see cref="Guid.Empty"/> if account does not exist.</remarks>
        public virtual async Task<Guid> GetAccountSecurityStampAsync(int accountId)
        {
            return await _sqlConnection.ExecuteScalarAsync<Guid>(@"[Website].[GetAccountSecurityStamp]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Sets EmailConfirmed of account with specified <paramref name="accountId"/> to true. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> UpdateAccountEmailConfirmedAsync(int accountId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[UpdateAccountEmailConfirmed]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Sets TwoFactorEnabled of account with specified <paramref name="accountId"/> to <paramref name="twoFactorEnabled"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="twoFactorEnabled"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> UpdateAccountTwoFactorEnabledAsync(int accountId, bool twoFactorEnabled)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[UpdateAccountTwoFactorEnabled]",
                new
                {
                    AccountId = accountId,
                    TwoFactorEnabled = twoFactorEnabled
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Sets PasswordHash of account with specified <paramref name="accountId"/> to 
        /// hash generated from <paramref name="password"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="password"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> UpdateAccountPasswordHashAsync(int accountId, string password)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[UpdateAccountPasswordHash]",
                new
                {
                    AccountId = accountId,
                    Password = password
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Sets Email of account with specified <paramref name="accountId"/> to 
        /// <paramref name="email"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="email"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> UpdateAccountEmail(int accountId, string email)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[UpdateAccountEmail]",
                new
                {
                    AccountId = accountId,
                    Email = email
                },
                commandType: CommandType.StoredProcedure) > 0;
        }
    }
}

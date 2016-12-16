using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System;
using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts.DatabaseInterface.Dapper
{
    /// <summary>
    /// Provides an interface for performing CRUD operations on account representations in a database. 
    /// Functions with varying granularity are provided to avoid reading and writing contextually superfluous 
    /// information while minimizing round trips.
    /// </summary>
    public class DapperAccountRepository<TAccount> : IAccountRepository<TAccount> 
        where TAccount : IAccount
    {
        private int _invalidArgumentErrorNumber = 51000;
        private int _constraintViolationErrorNumber = 2627;
        private int _uniqueIndexViolationErrorNumber = 2601;

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

        #region Create
        /// <summary>
        /// Creates an account with specified <paramref name="email"/> and <paramref name="passwordHash"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordLastChanged"></param>
        /// <param name="securityStamp"></param>
        /// <returns>
        /// <see cref="CreateResult{TAccount}"/> with <see cref="CreateResult{TAccount}.DuplicateRow"/> set to true if <paramref name="email"/> is in use.
        /// <see cref="CreateResult{TAccount}"/> with <see cref="CreateResult{TAccount}.Succeeded"/> set to true if account is created successfully. 
        /// </returns>
        public virtual async Task<CreateResult<TAccount>> CreateAsync(string email, string passwordHash, 
            DateTimeOffset passwordLastChanged, Guid securityStamp)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException(nameof(email));
            }
            if(string.IsNullOrEmpty(passwordHash))
            {
                throw new ArgumentException(nameof(passwordHash));
            }
            if(securityStamp == default(Guid))
            {
                throw new ArgumentException(nameof(securityStamp));
            }
            if(passwordLastChanged == default(DateTimeOffset))
            {
                throw new ArgumentException(nameof(passwordLastChanged));
            }

            try
            {
                TAccount account = (await _sqlConnection.QueryAsync<TAccount>(@"[Accounts].[CreateAccount]",
                    new
                    {
                        PasswordHash = passwordHash,
                        Email = email,
                        PasswordLastChanged = passwordLastChanged,
                        SecurityStamp = securityStamp
                    },
                commandType: CommandType.StoredProcedure)).First();

                return CreateResult<TAccount>.GetSucceededResult(account);
            }
            catch (SqlException exception)
            {
                if (exception.Number == _constraintViolationErrorNumber)
                {
                    return CreateResult<TAccount>.GetDuplicateRowResult();
                }

                throw;
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Checks whether <paramref name="email"/> is in use.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// True if <paramref name="email"/> is in use.
        /// False otherwise.
        /// </returns>
        public virtual async Task<bool> CheckEmailInUseAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException(nameof(email));
            }

            return await _sqlConnection.ExecuteScalarAsync<bool>(@"[Accounts].[CheckEmailInUse]",
                new
                {
                    Email = email
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Checks whether <paramref name="displayName"/> is in use.
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns>
        /// True if <paramref name="displayName"/> is in use.
        /// False otherwise.
        /// </returns>
        public virtual async Task<bool> CheckDisplayNameInUseAsync(string displayName)
        {
            return await _sqlConnection.ExecuteScalarAsync<bool>(@"[Accounts].[CheckDisplayNameInUse]",
                new
                {
                    DisplayName = displayName
                },
                commandType: CommandType.StoredProcedure);
        } 

        /// <summary>
        /// Gets account with specified <paramref name="accountId"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>
        /// Account with specified <paramref name="accountId"/> if it exists. 
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetAccountAsync(int accountId)
        {
            try
            {
                TAccount account = (await _sqlConnection.QueryAsync<TAccount>(@"[Accounts].[GetAccount]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure)).First();

                return account;
            }
            catch (SqlException exception)
            {
                if (exception.Number == _invalidArgumentErrorNumber)
                {
                    return default(TAccount);
                }

                throw;
            }
        }

        /// <summary>
        /// Gets account with specified <paramref name="email"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// Account with specified <paramref name="email"/> if it exists. 
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetByEmailAsync(string email)
        {
            try
            {
                TAccount account = (await _sqlConnection.QueryAsync<TAccount>(@"[Accounts].[GetAccountByEmail]",
                new
                {
                    Email = email
                },
                commandType: CommandType.StoredProcedure)).First();

                return account;
            }
            catch (SqlException exception)
            {
                if (exception.Number == _invalidArgumentErrorNumber)
                {
                    return default(TAccount);
                }

                throw;
            }
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
        public virtual async Task<IEnumerable<Role>> GetRolesAsync(int accountId)
        {
            return await _sqlConnection.QueryAsync<Role>(@"[Accounts].[GetAccountRoles]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
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
        public virtual async Task<IEnumerable<Claim>> GetClaimsAsync(int accountId)
        {
            return await _sqlConnection.QueryAsync<Claim>(@"[Accounts].[GetAccountClaims]",
                new
                {
                    AccountId = accountId
                },
                commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region Update
        /// <summary>
        /// Adds <see cref="Role"/> with specified <paramref name="roleId"/> to account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> AddRoleAsync(int accountId, int roleId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Accounts].[AddAccountRole]",
                new
                {
                    AccountId = accountId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// Adds <see cref="Claim"/> with specified <paramref name="claimId"/> to account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="claimId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> AddClaimAsync(int accountId, int claimId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Accounts].[AddAccountClaim]",
                new
                {
                    AccountId = accountId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }


        /// <summary>
        /// Sets Email of account with specified <paramref name="accountId"/> to 
        /// <paramref name="email"/>. 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="newEmail"></param>
        /// <returns>
        /// <see cref="UpdateResult.InvalidRowVersionOrAccountId"/> if <paramref name="accountId"/> is invalid.
        /// <see cref="UpdateResult.InvalidRowVersionOrAccountId"/> if <paramref name="rowVersion"/> is invalid.
        /// <see cref="UpdateResult.DuplicateRow"/> if <paramref name="email"/> is in use.
        /// <see cref="UpdateResult.Succeeded"/> if update is successful.
        /// </returns>
        public virtual async Task<UpdateResult<TAccount>> UpdateEmailAsync(TAccount account, string newEmail)
        {
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if(account.Email == newEmail)
            {
                return UpdateResult<TAccount>.GetAlreadyUpdatedResult();
            }

            try
            {
                TAccount updatedAccount = (await _sqlConnection.QueryAsync<TAccount>(@"[Accounts].[UpdateEmail]",
                    new
                    {
                        AccountId = account.AccountId,
                        Email = newEmail,
                        RowVersion = account.RowVersion
                    },
                    commandType: CommandType.StoredProcedure)).First();


                updatedAccount.EmailVerified = false;
                updatedAccount.TwoFactorEnabled = false;

                return UpdateResult<TAccount>.GetSucceededResult(updatedAccount);
            }
            catch (SqlException exception)
            {
                if (exception.Number == _invalidArgumentErrorNumber)
                {
                    return UpdateResult<TAccount>.GetInvalidRowVersionOrAccountIdResult();
                }
                if (exception.Number == _constraintViolationErrorNumber)
                {
                    return UpdateResult<TAccount>.GetDuplicateRowResult();
                }

                throw;
            }
        }

        /// <summary>
        /// Sets AltEmail of account with specified <paramref name="accountId"/> to 
        /// <paramref name="altEmail"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="altEmail"></param>
        /// <param name="rowVersion"></param>
        /// <returns>
        /// <see cref="SaveChangeResult<TAccount>.InvalidRowVersionOrAccountId"/> if <paramref name="accountId"/> is invalid.
        /// <see cref="SaveChangeResult<TAccount>.InvalidRowVersionOrAccountId"/> if <paramref name="rowVersion"/> is invalid.
        /// <see cref="SaveChangeResult<TAccount>.Succeeded"/> if update is successful.
        /// </returns>
        public virtual async Task<UpdateResult<TAccount>> UpdateAltEmailAsync(TAccount account, string altEmail)
        {
            if (string.IsNullOrEmpty(altEmail))
            {
                throw new ArgumentException(nameof(altEmail));
            }

            try
            {
                await _sqlConnection.ExecuteAsync(@"[Accounts].[UpdateAltEmail]",
                    new
                    {
                        AccountId = accountId,
                        AltEmail = altEmail,
                        RowVersion = rowVersion
                    },
                    commandType: CommandType.StoredProcedure);

                return UpdateResult<TAccount>.GetSucceededResult();
            }
            catch (SqlException exception)
            {
                if (exception.Number == _invalidArgumentErrorNumber)
                {
                    return UpdateResult<TAccount>.GetInvalidRowVersionOrAccountIdResult();
                }

                throw;
            }
        }

        /// <summary>
        /// Sets DisplayName of account with specified <paramref name="accountId"/> to 
        /// <paramref name="displayName"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="displayName"></param>
        /// <param name="rowVersion"></param>
        /// <returns>
        /// <see cref="SaveChangeResult<TAccount>.InvalidRowVersionOrAccountId"/> if <paramref name="accountId"/> is invalid.
        /// <see cref="SaveChangeResult<TAccount>.InvalidRowVersionOrAccountId"/> if <paramref name="rowVersion"/> is invalid.
        /// <see cref="SaveChangeResult<TAccount>.DuplicateRow"/> if <paramref name="displayName"/> is in use.
        /// <see cref="SaveChangeResult<TAccount>.Succeeded"/> if update is successful.
        /// </returns>
        public virtual async Task<UpdateResult<TAccount>> UpdateDisplayNameAsync(int accountId, string displayName, 
            byte[] rowVersion = null)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentException(nameof(displayName));
            }

            try
            {
                await _sqlConnection.ExecuteScalarAsync<int>(@"[Accounts].[UpdateDisplayName]",
                    new
                    {
                        AccountId = accountId,
                        DisplayName = displayName,
                        RowVersion = rowVersion
                    },
                    commandType: CommandType.StoredProcedure);

                return UpdateResult<TAccount>.GetSucceededResult();
            }
            catch (SqlException exception)
            {
                if (exception.Number == _invalidArgumentErrorNumber)
                {
                    return UpdateResult<TAccount>.GetInvalidRowVersionOrAccountIdResult();
                }
                if (exception.Number == _uniqueIndexViolationErrorNumber)
                {
                    return UpdateResult<TAccount>.GetDuplicateRowResult();
                }

                throw;
            }
        }

        /// <summary>
        /// Sets AltEmailVerified of account with specified <paramref name="accountId"/> to 
        /// <paramref name="altEmailVerified"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="altEmailVerified"></param>
        /// <param name="rowVersion"></param>
        /// <returns>
        /// <see cref="SaveChangeResult<TAccount>.InvalidRowVersionOrAccountId"/> if <paramref name="accountId"/> is invalid.
        /// <see cref="SaveChangeResult<TAccount>.InvalidRowVersionOrAccountId"/> if <paramref name="rowVersion"/> is invalid.
        /// <see cref="SaveChangeResult<TAccount>.Succeeded"/> if update is successful.
        /// </returns>
        public virtual async Task<UpdateResult<TAccount>> UpdateAltEmailVerifiedAsync(int accountId, bool altEmailVerified, 
            byte[] rowVersion = null)
        {
            try
            {
                await _sqlConnection.ExecuteAsync(@"[Accounts].[UpdateAltEmailVerified]",
                    new
                    {
                        AccountId = accountId,
                        AltEmailVerified = altEmailVerified,
                        RowVersion = rowVersion
                    },
                    commandType: CommandType.StoredProcedure);

                return UpdateResult<TAccount>.GetSucceededResult();
            }
            catch (SqlException exception)
            {
                if (exception.Number == _invalidArgumentErrorNumber)
                {
                    return UpdateResult<TAccount>.GetInvalidRowVersionOrAccountIdResult();
                }

                throw;
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes account with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="rowVersion"></param>
        /// <returns>
        /// <see cref="DeleteResult.InvalidRowVersionOrAccountId"/> if <paramref name="accountId"/> is invalid.
        /// <see cref="DeleteResult.InvalidRowVersionOrAccountId"/> if <paramref name="rowVersion"/> is invalid.
        /// <see cref="DeleteResult.Succeeded"/> if deletion is successful.
        /// </returns>
        public virtual async Task<DeleteResult> DeleteAsync(int accountId, byte[] rowVersion = null)
        {
            try
            {
                await _sqlConnection.ExecuteAsync(@"[Accounts].[DeleteAccount]",
                    new
                    {
                        AccountId = accountId,
                        RowVersion = rowVersion
                    },
                    commandType: CommandType.StoredProcedure);

                return DeleteResult.GetSucceededResult();
            }
            catch (SqlException exception)
            {
                if (exception.Number == _invalidArgumentErrorNumber)
                {
                    return DeleteResult.GetInvalidRowVersionOrAccountIdResult();
                }

                throw;
            }
        }

        /// <summary>
        /// Deletes <see cref="Role"/> with specified <paramref name="roleId"/> from account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public virtual async Task<bool> DeleteRoleAsync(int accountId, int roleId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Accounts].[DeleteAccountRole]",
                new
                {
                    AccountId = accountId,
                    RoleId = roleId
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
        public virtual async Task<bool> DeleteClaimAsync(int accountId, int claimId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Accounts].[DeleteAccountClaim]",
                new
                {
                    AccountId = accountId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }
        #endregion
    }
}

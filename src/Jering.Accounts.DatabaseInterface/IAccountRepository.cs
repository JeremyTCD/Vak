using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading;

namespace Jering.Accounts.DatabaseInterface
{
    /// <summary>
    /// Provides an interface for performing CRUD operations on account representations in a database. 
    /// Functions with varying granularity are provided to avoid reading and writing contextually superfluous 
    /// information while minimizing round trips.
    /// </summary>
    public interface IAccountRepository<TAccount> where TAccount : IAccount
    {
        #region Create
        Task<TAccount> CreateAsync(string email, string password, CancellationToken cancellationToken);
        #endregion

        #region Update
        Task<SaveChangesResult> UpdatePasswordHashAsync(TAccount account, string passwordHash, CancellationToken cancellationToken);

        Task<SaveChangesResult> UpdateEmailAsync(TAccount account, string email, CancellationToken cancellationToken);

        Task<SaveChangesResult> UpdateAltEmailAsync(TAccount account, string altEmail, CancellationToken cancellationToken);

        Task<SaveChangesResult> UpdateDisplayNameAsync(TAccount account, string displayName, CancellationToken cancellationToken);

        Task<SaveChangesResult> UpdateTwoFactorEnabledAsync(TAccount account, bool enabled, CancellationToken cancellationToken);

        Task<SaveChangesResult> UpdateEmailVerifiedAsync(TAccount account, bool verified, CancellationToken cancellationToken);

        Task<SaveChangesResult> UpdateAltEmailVerifiedAsync(TAccount account, bool verified, CancellationToken cancellationToken);
        #endregion

        #region Remove
        /// <summary>
        /// Removes account <paramref name="account"/> from repository.
        /// </summary>
        /// <param name="account"></param>
        void RemoveAsync(TAccount account);
        #endregion

        #region Get
        /// <summary>
        /// Gets account with specified <paramref name="accountId"/>. Attempts to retrieve account from context.
        /// If account does not exist in context, retrieves account from database.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Account with specified <paramref name="accountId"/> if it exists. 
        /// Null otherwise.
        /// </returns>
        Task<TAccount> GetAsync(int accountId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets account with specified <paramref name="email"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// Account with specified <paramref name="email"/> if it exists. 
        /// Null otherwise.
        /// </returns>
        Task<TAccount> GetAsync(string email, CancellationToken cancellationToken);
        #endregion

        #region Check
        /// <summary>
        /// Checks whether <paramref name="email"/> is in use.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// True if <paramref name="email"/> is in use.
        /// False otherwise.
        /// </returns>
        Task<bool> CheckEmailInUseAsync(string email, CancellationToken cancellationToken);

        /// <summary>
        /// Checks whether <paramref name="displayName"/> is in use.
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns>
        /// True if <paramref name="displayName"/> is in use.
        /// False otherwise.
        /// </returns>
        Task<bool> CheckDisplayNameInUseAsync(string displayName, CancellationToken cancellationToken);
        #endregion
    }
}

using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Jering.AccountManagement.DatabaseInterface
{
    /// <summary>
    /// Provides an interface for performing CRUD operations on account representations in a database. 
    /// Functions with varying granularity are provided to avoid reading and writing contextually superfluous 
    /// information while minimizing round trips.
    /// </summary>
    public interface IAccountRepository<TAccount> 
        where TAccount : IAccount
    {
        /// <summary>
        /// Creates an <see cref="IAccount"/> with specified <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>Newly created <see cref="IAccount"/> if <see cref="IAccount"/> is created successfully, null otherwise.</returns>
        Task<TAccount> CreateAccountAsync(string email, string password);

        /// <summary>
        /// Deletes <see cref="IAccount"/> with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> DeleteAccountAsync(int accountId);

        /// <summary>
        /// Gets <see cref="IAccount"/> with specified <paramref name="accountId"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns><see cref="IAccount"/> with specified <paramref name="accountId"/> if it exists, 
        /// null otherwise.</returns>
        Task<TAccount> GetAccountAsync(int accountId);

        /// <summary>
        /// Gets <see cref="IAccount"/> with specified <paramref name="email"/> and <paramref name="password"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns><see cref="IAccount"/> with specified <paramref name="email"/> and <paramref name="password"/> if it exists, 
        /// null otherwise.</returns>
        Task<TAccount> GetAccountByEmailAndPasswordAsync(string email, string password);

        /// <summary>
        /// Gets <see cref="IAccount"/> with specified <paramref name="email"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns><see cref="IAccount"/> with specified <paramref name="email"/> if it exists, 
        /// null otherwise.</returns>
        Task<TAccount> GetAccountByEmailAsync(string email);

        /// <summary>
        /// Adds <see cref="Role"/> with specified <paramref name="roleId"/> to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> AddAccountRoleAsync(int accountId, int roleId);

        /// <summary>
        /// Deletes <see cref="Role"/> with specified <paramref name="roleId"/> from <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> DeleteAccountRoleAsync(int accountId, int roleId);

        /// <summary>
        /// Gets <see cref="Role"/>s belonging to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns><see cref="IEnumerable{Role}"/> containing <see cref="Role"/>s belonging to account with 
        /// specified <paramref name="accountId"/>.</returns>
        /// <remarks>Returns empty <see cref="IEnumerable{Role}"/> if account has no <see cref="Role"/>s 
        /// or <see cref="IAccount"/> does not exist.</remarks>
        Task<IEnumerable<Role>> GetAccountRolesAsync(int accountId);

        /// <summary>
        /// Adds <see cref="Claim"/> with specified <paramref name="claimId"/> to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="claimId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> AddAccountClaimAsync(int accountId, int claimId);

        /// <summary>
        /// Deletes <see cref="Claim"/> with specified <paramref name="claimId"/> from <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="claimId"></param>
        /// <returns>True if successful, false othewise.</returns>
        Task<bool> DeleteAccountClaimAsync(int accountId, int claimId);

        /// <summary>
        /// Gets <see cref="Claim"/>s belonging to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns><see cref="IEnumerable{Claim}"/> containing <see cref="Claim"/>s belonging to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.</returns>
        /// <remarks>Returns empty <see cref="IEnumerable{Claim}"/> if <see cref="IAccount"/> has no <see cref="Role"/>s 
        /// or <see cref="IAccount"/> does not exist.</remarks>
        Task<IEnumerable<Claim>> GetAccountClaimsAsync(int accountId);

        /// <summary>
        /// Gets SecurityStamp of <see cref="IAccount"/> with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>A <see cref="Guid"/>.</returns>
        /// <remarks>Returns a <see cref="Guid"/> with value <see cref="Guid.Empty"/> if <see cref="IAccount"/> does not exist.</remarks>
        Task<Guid> GetAccountSecurityStampAsync(int accountId);

        /// <summary>
        /// Sets EmailConfirmed of <see cref="IAccount"/> with specified <paramref name="accountId"/> to true. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> UpdateAccountEmailConfirmedAsync(int accountId);

        /// <summary>
        /// Sets PasswordHash of <see cref="IAccount"/> with specified <paramref name="accountId"/> to 
        /// hash generated from <paramref name="password"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="password"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> UpdateAccountPasswordHashAsync(int accountId, string password);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> UpdateAccountEmailAsync(int accountId, string email);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="alternativeEmail"></param>
        /// <returns></returns>
        Task<bool> UpdateAccountAlternativeEmailAsync(int accountId, string alternativeEmail);
    }
}

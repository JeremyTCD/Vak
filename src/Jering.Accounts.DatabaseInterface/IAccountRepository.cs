using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Jering.Accounts.DatabaseInterface
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
        /// Creates an account with specified <paramref name="email"/> and <paramref name="passwordHash"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        /// <returns>Newly created account.</returns>
        Task<TAccount> CreateAccountAsync(string email, string passwordHash);

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
        /// Gets <see cref="IAccount"/> with specified <paramref name="email"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns><see cref="IAccount"/> with specified <paramref name="email"/> if it exists, 
        /// null otherwise.</returns>
        Task<TAccount> GetAccountByEmailAsync(string email);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<TAccount> GetAccountByEmailOrAltEmailAsync(string email);

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
        /// Sets EmailVerified of <see cref="IAccount"/> with specified <paramref name="accountId"/> to true. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="emailVerified"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> UpdateAccountEmailVerifiedAsync(int accountId, bool emailVerified);

        /// <summary>
        /// Sets PasswordHash of account with specified <paramref name="accountId"/> to 
        /// <paramref name="passwordHash"/>. 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="passwordHash"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> UpdateAccountPasswordHashAsync(int accountId, string passwordHash);

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
        /// <param name="altEmail"></param>
        /// <returns></returns>
        Task<bool> UpdateAccountAltEmailAsync(int accountId, string altEmail);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        Task<bool> UpdateAccountDisplayNameAsync(int accountId, string displayName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="twoFactorEnabled"></param>
        /// <returns></returns>
        Task<bool> UpdateAccountTwoFactorEnabledAsync(int accountId, bool twoFactorEnabled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="altEmailVerified"></param>
        /// <returns></returns>
        Task<bool> UpdateAccountAltEmailVerifiedAsync(int accountId, bool altEmailVerified);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> CheckEmailInUseAsync(string email);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        Task<bool> CheckDisplayNameInUseAsync(string displayName);
    }
}

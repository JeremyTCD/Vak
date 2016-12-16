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
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordLastChanged"></param>
        /// <param name="securityStamp"></param>
        /// <returns></returns>
        Task<CreateResult<TAccount>> CreateAsync(string email, string passwordHash,
            DateTimeOffset passwordLastChanged, Guid securityStamp);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Task<DeleteResult> DeleteAsync(int accountId, byte[] rowVersion = null);

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
        Task<TAccount> GetByEmailAsync(string email);

        /// <summary>
        /// Adds <see cref="Role"/> with specified <paramref name="roleId"/> to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> AddRoleAsync(int accountId, int roleId);

        /// <summary>
        /// Deletes <see cref="Role"/> with specified <paramref name="roleId"/> from <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roleId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> DeleteRoleAsync(int accountId, int roleId);

        /// <summary>
        /// Gets <see cref="Role"/>s belonging to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns><see cref="IEnumerable{Role}"/> containing <see cref="Role"/>s belonging to account with 
        /// specified <paramref name="accountId"/>.</returns>
        /// <remarks>Returns empty <see cref="IEnumerable{Role}"/> if account has no <see cref="Role"/>s 
        /// or <see cref="IAccount"/> does not exist.</remarks>
        Task<IEnumerable<Role>> GetRolesAsync(int accountId);

        /// <summary>
        /// Adds <see cref="Claim"/> with specified <paramref name="claimId"/> to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="claimId"></param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> AddClaimAsync(int accountId, int claimId);

        /// <summary>
        /// Deletes <see cref="Claim"/> with specified <paramref name="claimId"/> from <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="claimId"></param>
        /// <returns>True if successful, false othewise.</returns>
        Task<bool> DeleteClaimAsync(int accountId, int claimId);

        /// <summary>
        /// Gets <see cref="Claim"/>s belonging to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns><see cref="IEnumerable{Claim}"/> containing <see cref="Claim"/>s belonging to <see cref="IAccount"/> with 
        /// specified <paramref name="accountId"/>.</returns>
        /// <remarks>Returns empty <see cref="IEnumerable{Claim}"/> if <see cref="IAccount"/> has no <see cref="Role"/>s 
        /// or <see cref="IAccount"/> does not exist.</remarks>
        Task<IEnumerable<Claim>> GetClaimsAsync(int accountId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="emailVerified"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Task<SaveChangeResult<TAccount>> UpdateEmailVerifiedAsync(int accountId, bool emailVerified, byte[] rowVersion = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="passwordHash"></param>
        /// <param name="rowVersion"></param>
        /// <param name="passwordLastChanged"></param>
        /// <param name="securityStamp"></param>
        /// <returns></returns>
        Task<SaveChangeResult<TAccount>> UpdatePasswordHashAsync(int accountId, string passwordHash, DateTimeOffset passwordLastChanged, Guid securityStamp, byte[] rowVersion = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<SaveChangeResult<TAccount>> UpdateEmailAsync(TAccount account);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="altEmail"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Task<SaveChangeResult<TAccount>> UpdateAltEmailAsync(int accountId, string altEmail, byte[] rowVersion = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="displayName"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Task<SaveChangeResult<TAccount>> UpdateDisplayNameAsync(int accountId, string displayName, byte[] rowVersion = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="twoFactorEnabled"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Task<SaveChangeResult<TAccount>> UpdateTwoFactorEnabledAsync(int accountId, bool twoFactorEnabled, byte[] rowVersion = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="altEmailVerified"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Task<SaveChangeResult<TAccount>> UpdateAltEmailVerifiedAsync(int accountId, bool altEmailVerified, byte[] rowVersion = null);

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

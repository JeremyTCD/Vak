using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jering.Accounts.DatabaseInterface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Creates a new Role in the Roles table
        /// </summary>
        /// <param name="name"></param>
        Task<Role> CreateRoleAsync(string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<bool> DeleteRoleAsync(int roleId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="claimId"></param>
        /// <returns></returns>
        Task<bool> AddRoleClaimAsync(int roleId, int claimId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="claimId"></param>
        /// <returns></returns>
        Task<bool> DeleteRoleClaimAsync(int roleId, int claimId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<IEnumerable<Claim>> GetRoleClaimsAsync(int roleId);
    }
}

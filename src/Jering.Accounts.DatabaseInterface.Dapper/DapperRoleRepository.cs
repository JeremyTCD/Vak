using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts.DatabaseInterface.Dapper
{
    /// <summary>
    /// 
    /// </summary>
    public class DapperRoleRepository : IRoleRepository
    {
        private SqlConnection _sqlConnection { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlConnection"></param>
        public DapperRoleRepository(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        /// <summary>
        /// Creates a new <see cref="Role"/>  in the Roles table
        /// </summary>
        /// <param name="name"></param>
        public virtual async Task<Role> CreateRoleAsync(string name)
        {
            return (await _sqlConnection.QueryAsync<Role>(@"[Website].[CreateRole]",
                new
                {
                    Name = name
                },
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteRoleAsync(int roleId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[DeleteRole]",
                new
                {
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="claimId"></param>
        /// <returns></returns>
        public virtual async Task<bool> AddRoleClaimAsync(int roleId, int claimId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[AddRoleClaim]",
                new
                {
                    RoleId = roleId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="claimId"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteRoleClaimAsync(int roleId, int claimId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[DeleteRoleClaim]",
                new
                {
                    RoleId = roleId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<Claim>> GetRoleClaimsAsync(int roleId)
        {
            return await _sqlConnection.QueryAsync<Claim>(@"[Website].[GetRoleClaims]",
                new
                {
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure); 
        }
    }
}

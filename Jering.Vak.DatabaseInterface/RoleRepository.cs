using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.Vak.DatabaseInterface
{
    public class RoleRepository
    {
        private SqlConnection _sqlConnection { get; }

        public RoleRepository(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        /// <summary>
        /// Creates a new Role in the Roles table
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public virtual async Task<Role> CreateRoleAsync(string name)
        {
            return await _sqlConnection.QuerySingleAsync<Role>(@"[Website].[CreateRole]",
                new
                {
                    Name = name
                },
                commandType: CommandType.StoredProcedure);
        }

        public virtual Task DeleteRoleAsync(int roleId)
        {
            return _sqlConnection.ExecuteAsync(@"[Website].[DeleteRole]",
                new
                {
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure);
        }

        public virtual Task AddRoleClaimAsync(int roleId, int claimId)
        {
            return _sqlConnection.ExecuteAsync(@"[Website].[AddRoleClaim]",
                new
                {
                    RoleId = roleId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure);
        }

        public virtual Task DeleteRoleClaimAsync(int roleId, int claimId)
        {
            return _sqlConnection.ExecuteAsync(@"[Website].[DeleteRoleClaim]",
                new
                {
                    RoleId = roleId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure);
        }

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

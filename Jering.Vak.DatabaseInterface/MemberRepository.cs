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
    /// Provides a lightweight interface for creating, reading and updating members. Functions avoid reading and writing 
    /// contextually superflous information as much as possible. 
    /// </summary>
    public class MemberRepository
    {
        private SqlConnection _sqlConnection { get; }

        public MemberRepository(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        /// <summary>
        /// Creates a new Member in the Members table
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public async Task<Member> CreateMemberAsync(string email, string password)
        {
            return await _sqlConnection.QuerySingleAsync<Member>(@"[Website].[CreateMember]",
                new
                {
                    Password = password,
                    Email = email
                },
                commandType: CommandType.StoredProcedure);
        }

        public Task DeleteMemberAsync(int memberId)
        {
            return _sqlConnection.ExecuteAsync(@"[Website].[DeleteMember]",
                new
                {
                    MemberId = memberId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddMemberRoleAsync(int memberId, int roleId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[AddMemberRole]",
                new
                {
                    MemberId = memberId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteMemberRoleAsync(int memberId, int roleId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[DeleteMemberRole]",
                new
                {
                    MemberId = memberId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Gets a member's roles
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Role>> GetMemberRolesAsync(int memberId)
        {
            return await _sqlConnection.QueryAsync<Role>(@"[Website].[GetMemberRoles]",
                new
                {
                    MemberId = memberId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task AddMemberClaimAsync(int memberId, int claimId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[AddMemberClaim]",
                new
                {
                    MemberId = memberId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteMemberClaimAsync(int memberId, int claimId)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[DeleteMemberClaim]",
                new
                {
                    MemberId = memberId,
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Claim>> GetMemberClaimsAsync(int memberId)
        {
            return await _sqlConnection.QueryAsync<Claim>(@"[Website].[GetMemberClaims]",
                new
                {
                    MemberId = memberId
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}

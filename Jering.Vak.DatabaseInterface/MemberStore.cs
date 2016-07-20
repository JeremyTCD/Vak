using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;

namespace Jering.ASP.NET.Identity.Dapper
{
    /// <summary>
    /// Class that implements the key ASP.NET Identity member store iterfaces
    /// </summary>
    public class MemberStore :
        //IUserClaimStore<TMember, int>,
        //IUserRoleStore<TMember, int>,
        //IUserPasswordStore<TMember, int>,
        //IUserSecurityStampStore<TMember, int>,
        //IQueryableUserStore<TMember, int>,
        //IUserEmailStore<TMember, int>,
        //IUserTwoFactorStore<TMember, int>,
        //IUserLockoutStore<TMember, int>,
        IUserStore<Member>
    {
        private SqlConnection _sqlConnection { get; }

        public MemberStore(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        /// <summary>
        /// Inserts a Member into the Members table
        /// </summary>
        /// <param name="member"></param>
        public async Task<IdentityResult> CreateAsync(Member member, CancellationToken cancellationToken)
        {
            member.MemberId = await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[InsertMember]",
                new
                {
                    PasswordHash = member.PasswordHash,
                    SecurityStamp = member.SecurityStamp,
                    Email = member.Email,
                    NormalizedEmail = member.NormalizedEmail
                },
                commandType: CommandType.StoredProcedure);

            return IdentityResult.Success;
        }

        /// <summary>
        /// Deletes a Member from the Members table
        /// </summary>
        /// <param name="member"></param>
        public async Task<IdentityResult> DeleteAsync(Member member, CancellationToken cancellationToken)
        {
            await _sqlConnection.ExecuteAsync("[Website].[DeleteMember]", new { MemberId = member.MemberId }, commandType: CommandType.StoredProcedure);

            return IdentityResult.Success;
        }

        /// <summary>
        /// Returns the Member for a given member Id
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public Task<Member> FindByIdAsync(string memberId, CancellationToken cancellationToken)
        {
            return _sqlConnection.QueryFirstAsync<Member>("[Website].[GetMember]", new { MemberId = Convert.ToInt32(memberId) }, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Returns the Member for a given normalized username
        /// </summary>
        /// <param name="normalizedUserName"></param>
        /// <returns></returns>
        public Task<Member> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return _sqlConnection.QueryFirstAsync<Member>("[Website].[GetMemberByNormalizedUsername]", new { NormalizedUsername = normalizedUserName }, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Returns the username for a given Member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public Task<string> GetNormalizedUserNameAsync(Member member, CancellationToken cancellationToken)
        {
            return Task.FromResult(member.NormalizedUsername);
        }

        /// <summary>
        /// Returns the member Id for a given Member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public Task<string> GetUserIdAsync(Member member, CancellationToken cancellationToken)
        {
            return Task.FromResult(member.MemberId.ToString());
        }

        /// <summary>
        /// Returns the username for a given Member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public Task<string> GetUserNameAsync(Member member, CancellationToken cancellationToken)
        {
            return Task.FromResult(member.Username);
        }

        /// <summary>
        /// Sets the normalized username for a given Member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public Task SetNormalizedUserNameAsync(Member member, string normalizedUsername, CancellationToken cancellationToken)
        {
            member.NormalizedUsername = normalizedUsername;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the username for a given Member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public Task SetUserNameAsync(Member member, string userName, CancellationToken cancellationToken)
        {
            member.Username = userName;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Updates a Member in the Members table
        /// </summary>
        /// <param name="Member"></param>
        public async Task<IdentityResult> UpdateAsync(Member member, CancellationToken cancellationToken)
        {
            await _sqlConnection.ExecuteAsync(@"[Website].[UpdateMember]", member, commandType: CommandType.StoredProcedure);

            return IdentityResult.Success;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

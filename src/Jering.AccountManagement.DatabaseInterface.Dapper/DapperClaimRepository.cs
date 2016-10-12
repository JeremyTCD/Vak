using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.AccountManagement.DatabaseInterface.Dapper
{
    /// <summary>
    /// 
    /// </summary>
    public class DapperClaimRepository: IClaimRepository
    {
        private SqlConnection _sqlConnection { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlConnection"></param>
        public DapperClaimRepository(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<Claim> CreateClaimAsync(string type, string value)
        {
            return (await _sqlConnection.QueryAsync<Claim>(@"[Website].[CreateClaim]",
                new
                {
                    Type = type,
                    Value = value
                },
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claimId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteClaimAsync(int claimId)
        {
            return await _sqlConnection.ExecuteScalarAsync<int>(@"[Website].[DeleteClaim]",
                new
                {
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure) > 0;
        }
    }
}

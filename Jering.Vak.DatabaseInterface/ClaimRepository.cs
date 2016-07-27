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
    public class ClaimRepository
    {
        private SqlConnection _sqlConnection { get; }

        public ClaimRepository(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }


        public virtual async Task<Claim> CreateClaimAsync(string type, string value)
        {
            return await _sqlConnection.QuerySingleAsync<Claim>(@"[Website].[CreateClaim]",
                new
                {
                    Type = type,
                    Value = value
                },
                commandType: CommandType.StoredProcedure);
        }

        public virtual Task DeleteClaimAsync(int claimId)
        {
            return _sqlConnection.ExecuteAsync(@"[Website].[DeleteClaim]",
                new
                {
                    ClaimId = claimId
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}

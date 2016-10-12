using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System;
using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.DatabaseInterface.Dapper;

namespace Jering.VectorArtKit.WebApi.BusinessModel
{
    /// <summary>
    /// Provides an interface for performing CRUD operations on account representations in a database. 
    /// Functions with varying granularity are provided to avoid reading and writing contextually superfluous 
    /// information while minimizing round trips.
    /// </summary>
    public class VakAccountRepository : DapperAccountRepository<VakAccount>
    {
        /// <summary>
        /// Constructs an instance of <see cref="VakAccountRepository"/>. 
        /// </summary>
        /// <param name="sqlConnection"></param>
        public VakAccountRepository(SqlConnection sqlConnection) : base(sqlConnection)
        {
        }
    }
}

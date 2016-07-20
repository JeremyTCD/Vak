//using Dapper;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Jering.ASP.NET.Identity.Dapper
//{
//    /// <summary>
//    /// Class that represents the Members table in the Database
//    /// </summary>
//    public class MemberTable<TMember>
//        where TMember : Member
//    {
//        private SqlConnection _sqlConnection;

//        /// <summary>
//        /// Constructor that takes a SqlConnection instance 
//        /// </summary>
//        /// <param name="sqlConnection"></param>
//        public MemberTable(SqlConnection sqlConnection)
//        {
//            _sqlConnection = sqlConnection;
//        }

//        /// <summary>
//        /// Returns the ID for a given username
//        /// </summary>
//        /// <param name="username"></param>
//        /// <returns></returns>
//        public Task<int> GetMemberIdByUsernameAsync(string username)
//        {
//            return _sqlConnection.ExecuteScalarAsync<int>("[Website].[GetMemberId]", new { Username = username }, commandType: CommandType.StoredProcedure);
//        }

//        /// <summary>
//        /// Returns the TMember for a given email
//        /// </summary>
//        /// <param name="email"></param>
//        /// <returns></returns>
//        public async Task<TMember> GetMemberByEmailAsync(string email)
//        {
//            return (await _sqlConnection.QueryAsync<TMember>("[Website].[GetMemberByEmail]", new { Email = email }, commandType: CommandType.StoredProcedure)).FirstOrDefault();
//        }

//        /// <summary>
//        /// Returns the password hash for a given member ID
//        /// </summary>
//        /// <param name="id">The Member's id</param>
//        /// <returns></returns>
//        public Task<string> GetMemberPasswordHashAsync(int id)
//        {
//            return _sqlConnection.ExecuteScalarAsync<string>("[Website].[GetMemberPasswordHash]", new { Id = id }, commandType: CommandType.StoredProcedure);
//        }

//        /// <summary>
//        /// Sets the Member's password hash
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="passwordHash"></param>
//        /// <returns></returns>
//        public Task SetMemberPasswordHashAsync(int id, string passwordHash)
//        {
//            return _sqlConnection.ExecuteAsync("[Website].[SetMemberPasswordHash]", new { PasswordHash = passwordHash, Id = id }, commandType: CommandType.StoredProcedure);
//        }

//        /// <summary>
//        /// Returns the security stamp for a given member ID
//        /// </summary>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        public Task<string> GetMemberSecurityStampAsync(int id)
//        {
//            return _sqlConnection.ExecuteScalarAsync<string>("[Website].[GetMemberSecurityStamp]", new { Id = id }, commandType: CommandType.StoredProcedure);
//        }
//    }
//}

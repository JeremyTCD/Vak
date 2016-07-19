//using Microsoft.AspNet.Identity;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Dapper;
//using System.Data;
//using System.Data.SqlClient;
//using System.Configuration;

//namespace Jering.ASP.NET.Identity.Dapper
//{
//    /// <summary>
//    /// Class that implements the key ASP.NET Identity member store iterfaces
//    /// </summary>
//    public class MemberStore<TMember> : 
//        IUserClaimStore<TMember, int>,
//        IUserRoleStore<TMember, int>,
//        IUserPasswordStore<TMember, int>,
//        IUserSecurityStampStore<TMember, int>,
//        IQueryableUserStore<TMember, int>,
//        IUserEmailStore<TMember, int>,
//        IUserTwoFactorStore<TMember, int>,
//        IUserLockoutStore<TMember, int>,
//        IUserStore<TMember, int>
//        where TMember : Member
//    {
//        private MemberTable<TMember> memberTable;
//        private RoleTable roleTable;
//        private MemberRolesTable memberRolesTable;
//        private MemberClaimsTable memberClaimsTable;
//        public SqlConnection SqlConnection { get; private set; }

//        public IQueryable<TMember> Members
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//        }


//        /// <summary>
//        /// Default constructor that initializes a new database
//        /// instance using the Default Connection string
//        /// </summary>
//        public MemberStore()
//        {
//            new MemberStore<TMember>(new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString));
//        }

//        /// <summary>
//        /// Constructor that takes a SqlConnection as argument 
//        /// </summary>
//        /// <param name="SqlConnection"></param>
//        public MemberStore(SqlConnection SqlConnection)
//        {
//            SqlConnection = SqlConnection;
//            memberTable = new MemberTable<TMember>(SqlConnection);
//            roleTable = new RoleTable(SqlConnection);
//            memberRolesTable = new MemberRolesTable(SqlConnection);
//            memberClaimsTable = new MemberClaimsTable(SqlConnection);
//        }

//        /// <summary>
//        /// Insert a new TMember in the MemberTable
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task CreateAsync(TMember member)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            memberTable.InsertMember(member);

//            return Task.FromResult<object>(null);
//        }

//        /// <summary>
//        /// Returns an TMember instance based on a memberId query 
//        /// </summary>
//        /// <param name="memberId">The member's Id</param>
//        /// <returns></returns>
//        public Task<TMember> FindByIdAsync(int memberId)
//        {

//            TMember result = memberTable.GetMember(memberId) as TMember;
//            if (result != null)
//            {
//                return Task.FromResult<TMember>(result);
//            }

//            return Task.FromResult<TMember>(null);
//        }

//        /// <summary>
//        /// Returns an TMember instance based on a memberName query 
//        /// </summary>
//        /// <param name="memberName">The member's name</param>
//        /// <returns></returns>
//        public Task<TMember> FindByNameAsync(string memberName)
//        {
//            if (string.IsNullOrEmpty(memberName))
//            {
//                throw new ArgumentException("Null or empty argument: memberName");
//            }

//            TMember result = memberTable.GetMemberByUsername(memberName);

//            return Task.FromResult<TMember>(null);
//        }

//        /// <summary>
//        /// Updates the MembersTable with the TMember instance values
//        /// </summary>
//        /// <param name="member">TMember to be updated</param>
//        /// <returns></returns>
//        public Task UpdateAsync(TMember member)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            memberTable.UpdateMember(member);

//            return Task.FromResult<object>(null);
//        }

//        public void Dispose()
//        {
//            if (SqlConnection != null)
//            {
//                SqlConnection.Dispose();
//                SqlConnection = null;
//            }
//        }

//        /// <summary>
//        /// Inserts a claim to the MemberClaimsTable for the given member
//        /// </summary>
//        /// <param name="member">Member to have claim added</param>
//        /// <param name="claim">Claim to be added</param>
//        /// <returns></returns>
//        public Task AddClaimAsync(TMember member, Claim claim)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            if (claim == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            memberClaimsTable.Insert(claim, member.Id);

//            return Task.FromResult<object>(null);
//        }

//        /// <summary>
//        /// Returns all claims for a given member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<IList<Claim>> GetClaimsAsync(TMember member)
//        {
//            ClaimsIdentity identity = memberClaimsTable.FindByMemberId(member.Id);

//            return Task.FromResult<IList<Claim>>(identity.Claims.ToList());
//        }

//        /// <summary>
//        /// Removes a claim froma member
//        /// </summary>
//        /// <param name="member">Member to have claim removed</param>
//        /// <param name="claim">Claim to be removed</param>
//        /// <returns></returns>
//        public Task RemoveClaimAsync(TMember member, Claim claim)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            if (claim == null)
//            {
//                throw new ArgumentNullException("claim");
//            }

//            memberClaimsTable.Delete(member, claim);

//            return Task.FromResult<object>(null);
//        }

//        /// <summary>
//        /// Inserts a Login in the MemberLoginsTable for a given Member
//        /// </summary>
//        /// <param name="member">Member to have login added</param>
//        /// <param name="login">Login to be added</param>
//        /// <returns></returns>
//        public Task AddLoginAsync(TMember member, MemberLoginInfo login)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            if (login == null)
//            {
//                throw new ArgumentNullException("login");
//            }

//            memberLoginsTable.Insert(member, login);

//            return Task.FromResult<object>(null);
//        }

//        /// <summary>
//        /// Returns an TMember based on the Login info
//        /// </summary>
//        /// <param name="login"></param>
//        /// <returns></returns>
//        public Task<TMember> FindAsync(MemberLoginInfo login)
//        {
//            if (login == null)
//            {
//                throw new ArgumentNullException("login");
//            }

//            var memberId = memberLoginsTable.FindMemberIdByLogin(login);
//            if (memberId > 0)
//            {
//                TMember member = memberTable.GetMember(memberId) as TMember;
//                if (member != null)
//                {
//                    return Task.FromResult<TMember>(member);
//                }
//            }

//            return Task.FromResult<TMember>(null);
//        }

//        /// <summary>
//        /// Returns list of MemberLoginInfo for a given TMember
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<IList<MemberLoginInfo>> GetLoginsAsync(TMember member)
//        {
//            List<MemberLoginInfo> memberLogins = new List<MemberLoginInfo>();
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            List<MemberLoginInfo> logins = memberLoginsTable.FindByMemberId(member.Id);
//            if (logins != null)
//            {
//                return Task.FromResult<IList<MemberLoginInfo>>(logins);
//            }

//            return Task.FromResult<IList<MemberLoginInfo>>(null);
//        }

//        /// <summary>
//        /// Deletes a login from MemberLoginsTable for a given TMember
//        /// </summary>
//        /// <param name="member">Member to have login removed</param>
//        /// <param name="login">Login to be removed</param>
//        /// <returns></returns>
//        public Task RemoveLoginAsync(TMember member, MemberLoginInfo login)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            if (login == null)
//            {
//                throw new ArgumentNullException("login");
//            }

//            memberLoginsTable.Delete(member, login);

//            return Task.FromResult<Object>(null);
//        }

//        /// <summary>
//        /// Inserts a entry in the MemberRoles table
//        /// </summary>
//        /// <param name="member">Member to have role added</param>
//        /// <param name="roleName">Name of the role to be added to member</param>
//        /// <returns></returns>
//        public Task AddToRoleAsync(TMember member, string roleName)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            if (string.IsNullOrEmpty(roleName))
//            {
//                throw new ArgumentException("Argument cannot be null or empty: roleName.");
//            }

//            int roleId = roleTable.GetRoleId(roleName);
//            if (roleId > 0)
//            {
//                memberRolesTable.Insert(member, roleId);
//            }
//            //if (!string.IsNullOrEmpty(roleId))
//            //{
//            //    memberRolesTable.Insert(member, roleId);
//            //}

//            return Task.FromResult<object>(null);
//        }

//        /// <summary>
//        /// Returns the roles for a given TMember
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<IList<string>> GetRolesAsync(TMember member)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            List<string> roles = memberRolesTable.FindByMemberId(member.Id);
//            {
//                if (roles != null)
//                {
//                    return Task.FromResult<IList<string>>(roles);
//                }
//            }

//            return Task.FromResult<IList<string>>(null);
//        }

//        /// <summary>
//        /// Verifies if a member is in a role
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="role"></param>
//        /// <returns></returns>
//        public Task<bool> IsInRoleAsync(TMember member, string role)
//        {
//            if (member == null)
//            {
//                throw new ArgumentNullException("member");
//            }

//            if (string.IsNullOrEmpty(role))
//            {
//                throw new ArgumentNullException("role");
//            }

//            List<string> roles = memberRolesTable.FindByMemberId(member.Id);
//            {
//                if (roles != null && roles.Contains(role))
//                {
//                    return Task.FromResult<bool>(true);
//                }
//            }

//            return Task.FromResult<bool>(false);
//        }

//        /// <summary>
//        /// Removes a member from a role
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="role"></param>
//        /// <returns></returns>
//        public Task RemoveFromRoleAsync(TMember member, string role)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// Deletes a member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task DeleteAsync(TMember member)
//        {
//            if (member != null)
//            {
//                memberTable.Delete(member);
//            }

//            return Task.FromResult<Object>(null);
//        }

//        /// <summary>
//        /// Returns the PasswordHash for a given TMember
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<string> GetPasswordHashAsync(TMember member)
//        {
//            string passwordHash = memberTable.GetPasswordHash(member.Id);

//            return Task.FromResult<string>(passwordHash);
//        }

//        /// <summary>
//        /// Verifies if member has password
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<bool> HasPasswordAsync(TMember member)
//        {
//            var hasPassword = !string.IsNullOrEmpty(memberTable.GetPasswordHash(member.Id));

//            return Task.FromResult<bool>(Boolean.Parse(hasPassword.ToString()));
//        }

//        /// <summary>
//        /// Sets the password hash for a given TMember
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="passwordHash"></param>
//        /// <returns></returns>
//        public Task SetPasswordHashAsync(TMember member, string passwordHash)
//        {
//            member.PasswordHash = passwordHash;

//            return Task.FromResult<Object>(null);
//        }

//        /// <summary>
//        ///  Set security stamp
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="stamp"></param>
//        /// <returns></returns>
//        public Task SetSecurityStampAsync(TMember member, string stamp)
//        {
//            member.SecurityStamp = stamp;

//            return Task.FromResult(0);

//        }

//        /// <summary>
//        /// Get security stamp
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<string> GetSecurityStampAsync(TMember member)
//        {
//            return Task.FromResult(member.SecurityStamp);
//        }

//        /// <summary>
//        /// Set email on member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="email"></param>
//        /// <returns></returns>
//        public Task SetEmailAsync(TMember member, string email)
//        {
//            member.Email = email;
//            memberTable.Update(member);

//            return Task.FromResult(0);

//        }

//        /// <summary>
//        /// Get email from member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<string> GetEmailAsync(TMember member)
//        {
//            return Task.FromResult(member.Email);
//        }

//        /// <summary>
//        /// Get if member email is confirmed
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<bool> GetEmailConfirmedAsync(TMember member)
//        {
//            return Task.FromResult(member.EmailConfirmed);
//        }

//        /// <summary>
//        /// Set when member email is confirmed
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="confirmed"></param>
//        /// <returns></returns>
//        public Task SetEmailConfirmedAsync(TMember member, bool confirmed)
//        {
//            member.EmailConfirmed = confirmed;
//            memberTable.Update(member);

//            return Task.FromResult(0);
//        }

//        /// <summary>
//        /// Get member by email
//        /// </summary>
//        /// <param name="email"></param>
//        /// <returns></returns>
//        public Task<TMember> FindByEmailAsync(string email)
//        {
//            if (String.IsNullOrEmpty(email))
//            {
//                throw new ArgumentNullException("email");
//            }

//            TMember result = memberTable.GetMemberByEmail(email) as TMember;
//            if (result != null)
//            {
//                return Task.FromResult<TMember>(result);
//            }

//            return Task.FromResult<TMember>(null);
//        }

//        /// <summary>
//        /// Set two factor authentication is enabled on the member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="enabled"></param>
//        /// <returns></returns>
//        public Task SetTwoFactorEnabledAsync(TMember member, bool enabled)
//        {
//            member.TwoFactorEnabled = enabled;
//            memberTable.Update(member);

//            return Task.FromResult(0);
//        }

//        /// <summary>
//        /// Get if two factor authentication is enabled on the member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<bool> GetTwoFactorEnabledAsync(TMember member)
//        {
//            return Task.FromResult(member.TwoFactorEnabled);
//        }

//        /// <summary>
//        /// Get member lock out end date
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<DateTimeOffset> GetLockoutEndDateAsync(TMember member)
//        {
//            return
//                Task.FromResult(member.LockoutEndDateUtc.HasValue
//                    ? new DateTimeOffset(DateTime.SpecifyKind(member.LockoutEndDateUtc.Value, DateTimeKind.Utc))
//                    : new DateTimeOffset());
//        }


//        /// <summary>
//        /// Set member lockout end date
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="lockoutEnd"></param>
//        /// <returns></returns>
//        public Task SetLockoutEndDateAsync(TMember member, DateTimeOffset lockoutEnd)
//        {
//            member.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
//            memberTable.Update(member);

//            return Task.FromResult(0);
//        }

//        /// <summary>
//        /// Increment failed access count
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<int> IncrementAccessFailedCountAsync(TMember member)
//        {
//            member.AccessFailedCount++;
//            memberTable.Update(member);

//            return Task.FromResult(member.AccessFailedCount);
//        }

//        /// <summary>
//        /// Reset failed access count
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task ResetAccessFailedCountAsync(TMember member)
//        {
//            member.AccessFailedCount = 0;
//            memberTable.Update(member);

//            return Task.FromResult(0);
//        }

//        /// <summary>
//        /// Get failed access count
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<int> GetAccessFailedCountAsync(TMember member)
//        {
//            return Task.FromResult(member.AccessFailedCount);
//        }

//        /// <summary>
//        /// Get if lockout is enabled for the member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <returns></returns>
//        public Task<bool> GetLockoutEnabledAsync(TMember member)
//        {
//            return Task.FromResult(member.LockoutEnabled);
//        }

//        /// <summary>
//        /// Set lockout enabled for member
//        /// </summary>
//        /// <param name="member"></param>
//        /// <param name="enabled"></param>
//        /// <returns></returns>
//        public Task SetLockoutEnabledAsync(TMember member, bool enabled)
//        {
//            member.LockoutEnabled = enabled;
//            memberTable.Update(member);

//            return Task.FromResult(0);
//        }
//    }
//}

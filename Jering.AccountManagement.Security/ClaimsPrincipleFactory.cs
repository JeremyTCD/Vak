using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
using System.Collections.Generic;
using System.Linq;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides methods to create a claims principal for a given account.
    /// </summary>
    public class ClaimsPrincipalFactory<TAccount> where TAccount : IAccount
    {
        private IAccountRepository<TAccount> _accountRepository { get; }
        private IRoleRepository _roleRepository { get; }
        private AccountSecurityOptions _securityOptions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountRepository"></param>
        /// <param name="roleRepository"></param>
        /// <param name="securityOptionsAccessor"></param>
        public ClaimsPrincipalFactory(IAccountRepository<TAccount> accountRepository, IRoleRepository roleRepository, IOptions<AccountSecurityOptions> securityOptionsAccessor)
        {
            _accountRepository = accountRepository;
            _roleRepository = roleRepository;
            _securityOptions = securityOptionsAccessor.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public virtual async Task<ClaimsPrincipal> CreateAsync(TAccount account)
        {
            var claimsIdentity = new ClaimsIdentity(
                _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                _securityOptions.ClaimsOptions.UsernameClaimType,
                _securityOptions.ClaimsOptions.RoleClaimType);

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.AccountIdClaimType, account.AccountId.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.UsernameClaimType, account.Email));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.SecurityStampClaimType, account.SecurityStamp.ToString()));

            IEnumerable<Role> roles = await _accountRepository.GetAccountRolesAsync(account.AccountId);
            foreach (Role role in roles)
            {
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Role, role.Name));
                claimsIdentity.AddClaims(ConvertDatabaseInterfaceClaims(await _roleRepository.GetRoleClaimsAsync(role.RoleId)));
            }

            claimsIdentity.AddClaims(ConvertDatabaseInterfaceClaims(await _accountRepository.GetAccountClaimsAsync(account.AccountId)));

            return new ClaimsPrincipal(claimsIdentity);
        }

        private IEnumerable<System.Security.Claims.Claim> ConvertDatabaseInterfaceClaims(IEnumerable<DatabaseInterface.Claim> databaseInterfaceClaims)
        {
            List<System.Security.Claims.Claim> result = new List<System.Security.Claims.Claim>(databaseInterfaceClaims.Count());

            foreach (DatabaseInterface.Claim databaseInterfaceClaim in databaseInterfaceClaims)
            {
                result.Add(new System.Security.Claims.Claim(databaseInterfaceClaim.Type, databaseInterfaceClaim.Value));
            }

            return result;
        }
    }
}
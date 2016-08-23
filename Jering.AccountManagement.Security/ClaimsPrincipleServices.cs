using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides methods to create a claims principal for a given account.
    /// </summary>
    public class ClaimsPrincipalServices<TAccount> where TAccount : IAccount
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
        public ClaimsPrincipalServices(IAccountRepository<TAccount> accountRepository, IRoleRepository roleRepository, IOptions<AccountSecurityOptions> securityOptionsAccessor)
        {
            _accountRepository = accountRepository;
            _roleRepository = roleRepository;
            _securityOptions = securityOptionsAccessor?.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationScheme"></param>
        /// <returns></returns>
        public virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(TAccount account, string authenticationScheme)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                authenticationScheme,
                _securityOptions.ClaimsOptions.UsernameClaimType,
                _securityOptions.ClaimsOptions.RoleClaimType);

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.AccountIdClaimType, account.AccountId.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.UsernameClaimType, account.Email));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.SecurityStampClaimType, account.SecurityStamp.ToString()));

            // TODO: flag to make addition of roles and extra claims optional. some cookies do not need all this info
            IEnumerable<Role> roles = await _accountRepository.GetAccountRolesAsync(account.AccountId);
            foreach (Role role in roles)
            {
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Role, role.Name));
                claimsIdentity.AddClaims(ConvertDatabaseInterfaceClaims(await _roleRepository.GetRoleClaimsAsync(role.RoleId)));
            }

            claimsIdentity.AddClaims(ConvertDatabaseInterfaceClaims(await _accountRepository.GetAccountClaimsAsync(account.AccountId)));

            return new ClaimsPrincipal(claimsIdentity);
        }

        /// <summary>
        /// Creates an account from a <paramref name="claimsPrincipal"/> instance.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <param name="authenticationScheme"></param>
        /// <returns>
        /// Account if <paramref name="claimsPrincipal"/> is valid.
        /// Null if <paramref name="claimsPrincipal"/> is invalid.
        /// </returns>
        public virtual TAccount CreateAccount(ClaimsPrincipal claimsPrincipal, string authenticationScheme)
        {
            System.Security.Claims.Claim emailClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.UsernameClaimType);
            System.Security.Claims.Claim accountIdClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);
            System.Security.Claims.Claim securityStampClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.SecurityStampClaimType);

            if (claimsPrincipal.Identity.AuthenticationType == authenticationScheme &&
                emailClaim != null &&
                accountIdClaim != null &&
                securityStampClaim != null)
            {
                TAccount account = (TAccount)Activator.CreateInstance(typeof(TAccount));

                account.Email = emailClaim.Value;
                account.AccountId = Convert.ToInt32(accountIdClaim.Value);
                account.SecurityStamp = Guid.Parse(securityStampClaim.Value);

                return account;
            }
            return default(TAccount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="authenticationScheme"></param>
        /// <returns></returns>
        public virtual ClaimsPrincipal CreateClaimsPrincipal(int accountId, string authenticationScheme)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(authenticationScheme);

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.AccountIdClaimType, accountId.ToString()));

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
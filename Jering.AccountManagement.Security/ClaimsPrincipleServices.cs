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
        /// <param name="authenticationMethod"></param>
        /// <param name="authenticationScheme"></param>
        /// <returns></returns>
        public virtual async Task<ClaimsPrincipal> CreateClaimsPrincipleAsync(TAccount account, string authenticationScheme, string authenticationMethod = null)
        {
            var claimsIdentity = new ClaimsIdentity(
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

            if (authenticationMethod != null)
            {
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.AuthenticationMethod, authenticationMethod));
            }

            return new ClaimsPrincipal(claimsIdentity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public virtual Task<TAccount> CreateAccountAsync(ClaimsPrincipal claimsPrincipal)
        {
            return Task.Factory.StartNew(() =>
            {
                System.Security.Claims.Claim accountIdClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);
                System.Security.Claims.Claim emailClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.UsernameClaimType);
                System.Security.Claims.Claim securityStampClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.SecurityStampClaimType);

                if (accountIdClaim == null || emailClaim == null || securityStampClaim == null)
                {
                    return default(TAccount);
                }

                return (TAccount)Convert.ChangeType(new
                {
                    AccountId = Convert.ToInt32(accountIdClaim.Value),
                    Email = emailClaim.Value,
                    SecurityStamp = Guid.Parse(securityStampClaim.Value)
                }, typeof(TAccount));
            });
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
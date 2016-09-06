using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http.Authentication;

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
        /// Creates a <see cref="ClaimsPrincipal"/> from <paramref name="account"/> and <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns></returns>
        public virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(TAccount account, string authenticationScheme, AuthenticationProperties authenticationProperties)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                authenticationScheme,
                _securityOptions.ClaimsOptions.UsernameClaimType,
                _securityOptions.ClaimsOptions.RoleClaimType);

            if (account?.Email == null || account.SecurityStamp == default(Guid) || account.AccountId == default(int))
            {
                throw new ArgumentException(nameof(account));
            }

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.AccountIdClaimType, account.AccountId.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.UsernameClaimType, account.Email));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.SecurityStampClaimType, account.SecurityStamp.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.IsPersistenClaimType, authenticationProperties.IsPersistent.ToString()));

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
        /// Updates a <see cref="ClaimsPrincipal"/> with values from <paramref name="account" />. Does not perform update if
        /// account id does not correspond to id of account that <paramref name="claimsPrincipal"/> represents.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="claimsPrincipal"></param>
        public virtual void UpdateClaimsPrincipal(TAccount account, ClaimsPrincipal claimsPrincipal)
        {
            ClaimsIdentity claimsIdentity = claimsPrincipal?.Identity as ClaimsIdentity;

            if(claimsIdentity == null)
            {
                throw new ArgumentException(nameof(claimsPrincipal));
            }

            System.Security.Claims.Claim accountIdClaim = claimsIdentity.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);
            System.Security.Claims.Claim usernameClaim = claimsIdentity.FindFirst(_securityOptions.ClaimsOptions.UsernameClaimType);
            System.Security.Claims.Claim securityStampClaim = claimsIdentity.FindFirst(_securityOptions.ClaimsOptions.SecurityStampClaimType);

            if (accountIdClaim == null || usernameClaim == null || securityStampClaim == null ||
                account.AccountId.ToString() != accountIdClaim.Value)
            {
                throw new ArgumentException();
            }

            if(usernameClaim.Value != account.Email)
            {
                claimsIdentity.RemoveClaim(usernameClaim);
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.UsernameClaimType, account.Email));
            }

            if (securityStampClaim.Value != account.SecurityStamp.ToString())
            {
                claimsIdentity.RemoveClaim(securityStampClaim);
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(_securityOptions.ClaimsOptions.SecurityStampClaimType, account.SecurityStamp.ToString()));
            }
        }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> with an account id claim.
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
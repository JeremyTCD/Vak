using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.Accounts.DatabaseInterface;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http.Authentication;

namespace Jering.Security
{
    /// <summary>
    /// Abstraction for creating and updating <see cref="ClaimsPrincipal"/> instances.
    /// </summary>
    public class ClaimsPrincipalService<TAccount> : IClaimsPrincipalService<TAccount> where TAccount : IAccount
    {
        private IAccountRepository<TAccount> _accountRepository { get; }
        private IRoleRepository _roleRepository { get; }
        private ClaimsOptions _options { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountRepository"></param>
        /// <param name="roleRepository"></param>
        /// <param name="securityOptionsAccessor"></param>
        public ClaimsPrincipalService(IAccountRepository<TAccount> accountRepository, IRoleRepository roleRepository, IOptions<ClaimsOptions> securityOptionsAccessor)
        {
            _accountRepository = accountRepository;
            _roleRepository = roleRepository;
            _options = securityOptionsAccessor?.Value;
        }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> from <paramref name="account"/> and <paramref name="authProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authScheme"></param>
        /// <param name="authProperties"></param>
        public virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(TAccount account, string authScheme, AuthenticationProperties authProperties)
        {
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if(account.Email == null)
            {
                throw new ArgumentException(nameof(account.Email));
            }
            if(authScheme == null)
            {
                throw new ArgumentNullException(nameof(authScheme));
            }
          

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                authScheme,
                _options.UsernameClaimType,
                _options.RoleClaimType);

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_options.AccountIdClaimType, account.AccountId.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_options.UsernameClaimType, account.Email));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_options.SecurityStampClaimType, account.SecurityStamp.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_options.IsPersistenClaimType, authProperties.IsPersistent.ToString()));

            IEnumerable<Role> roles = await _accountRepository.GetRolesAsync(account.AccountId);
            foreach (Role role in roles)
            {
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Role, role.Name));
                claimsIdentity.AddClaims(ConvertDatabaseInterfaceClaims(await _roleRepository.GetRoleClaimsAsync(role.RoleId)));
            }

            claimsIdentity.AddClaims(ConvertDatabaseInterfaceClaims(await _accountRepository.GetClaimsAsync(account.AccountId)));

            return new ClaimsPrincipal(claimsIdentity);
        }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> with an account id claim.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="authScheme"></param>
        /// <returns></returns>
        public virtual ClaimsPrincipal CreateClaimsPrincipal(int accountId, string authScheme)
        {
            if (authScheme == null)
            {
                throw new ArgumentNullException(nameof(authScheme));
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(authScheme);

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_options.AccountIdClaimType, accountId.ToString()));

            return new ClaimsPrincipal(claimsIdentity);
        }

        /// <summary>
        /// Updates a <see cref="ClaimsPrincipal"/> with values from <paramref name="account" />. Does not perform update if
        /// account id does not correspond to id of account that <paramref name="claimsPrincipal"/> represents.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="claimsPrincipal"></param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="claimsPrincipal"/> is not valid</exception>
        public virtual void UpdateClaimsPrincipal(TAccount account, ClaimsPrincipal claimsPrincipal)
        {
            if(account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            if(claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }
            if(claimsPrincipal.Identity == null)
            {
                throw new ArgumentException(nameof(claimsPrincipal.Identity));
            }

            ClaimsIdentity claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;

            System.Security.Claims.Claim accountIdClaim = claimsIdentity.FindFirst(_options.AccountIdClaimType);
            System.Security.Claims.Claim usernameClaim = claimsIdentity.FindFirst(_options.UsernameClaimType);
            System.Security.Claims.Claim securityStampClaim = claimsIdentity.FindFirst(_options.SecurityStampClaimType);

            if (accountIdClaim == null || usernameClaim == null || securityStampClaim == null ||
                account.AccountId.ToString() != accountIdClaim.Value)
            {
                throw new ArgumentException();
            }

            if(usernameClaim.Value != account.Email)
            {
                claimsIdentity.RemoveClaim(usernameClaim);
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(_options.UsernameClaimType, account.Email));
            }

            if (securityStampClaim.Value != account.SecurityStamp.ToString())
            {
                claimsIdentity.RemoveClaim(securityStampClaim);
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(_options.SecurityStampClaimType, account.SecurityStamp.ToString()));
            }
        }

        private IEnumerable<System.Security.Claims.Claim> ConvertDatabaseInterfaceClaims(IEnumerable<Jering.Accounts.DatabaseInterface.Claim> databaseInterfaceClaims)
        {
            List<System.Security.Claims.Claim> result = new List<System.Security.Claims.Claim>(databaseInterfaceClaims.Count());

            foreach (Jering.Accounts.DatabaseInterface.Claim databaseInterfaceClaim in databaseInterfaceClaims)
            {
                result.Add(new System.Security.Claims.Claim(databaseInterfaceClaim.Type, databaseInterfaceClaim.Value));
            }

            return result;
        }
    }
}
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Jering.Vak.DatabaseInterface;
using System.Collections.Generic;

namespace Jering.Vak.Authentication
{
    /// <summary>
    /// Provides methods to create a claims principal for a given account.
    /// </summary>
    public class ClaimsPrincipalFactory
    {
        private AccountRepository _accountRepository { get; }
        private RoleRepository _roleRepository { get; }
        private IdentityOptions _identityOptions { get; }

        public ClaimsPrincipalFactory(AccountRepository accountRepository, RoleRepository roleRepository, IOptions<IdentityOptions> identityOptionsAccessor)
        {
            _accountRepository = accountRepository;
            _roleRepository = roleRepository;
            _identityOptions = identityOptionsAccessor.Value;
        }

        public async Task<ClaimsPrincipal> CreateAsync(Account account)
        {
            var claimsIdentity = new ClaimsIdentity(
                _identityOptions.Cookies.ApplicationCookieAuthenticationScheme,
                _identityOptions.ClaimsIdentity.UserNameClaimType,
                _identityOptions.ClaimsIdentity.RoleClaimType);

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_identityOptions.ClaimsIdentity.UserIdClaimType, account.AccountId.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_identityOptions.ClaimsIdentity.UserNameClaimType, account.Email));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(_identityOptions.ClaimsIdentity.SecurityStampClaimType, account.SecurityStamp));

            IEnumerable<Role> roles = await _accountRepository.GetAccountRolesAsync(account.AccountId);
            foreach (Role role in roles)
            {
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(_identityOptions.ClaimsIdentity.RoleClaimType, role.Name));
                claimsIdentity.AddClaims(await _roleRepository.GetRoleClaimsAsync(role.RoleId));
            }

            claimsIdentity.AddClaims(await _accountRepository.GetAccountClaimsAsync(account.AccountId));

            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
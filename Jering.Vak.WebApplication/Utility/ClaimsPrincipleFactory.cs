using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Jering.Vak.DatabaseInterface;
using System.Collections.Generic;

namespace Jering.Vak.WebApplication.Utility
{
    /// <summary>
    /// Provides methods to create a claims principal for a given member.
    /// </summary>
    public class ClaimsPrincipalFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsPrincipalFactory"/> class.
        /// </summary>
        /// <param name="memberRepository">The <see cref="MemberRepository"/> to retrieve user information from.</param>
        /// <param name="roleManager">The <see cref="RoleRepository"/> to retrieve a user's roles from.</param>
        /// <param name="identityOptionsAccessor">The configured <see cref="Builder.IdentityOptions"/>.</param>
        public ClaimsPrincipalFactory(MemberRepository memberRepository, RoleRepository roleRepository, IOptions<IdentityOptions> identityOptionsAccessor)
        {
            _memberRepository = memberRepository;
            _roleRepository = roleRepository;
            _identityOptions = identityOptionsAccessor.Value;
        }

        private MemberRepository _memberRepository { get; }
        private RoleRepository _roleRepository { get; }
        private IdentityOptions _identityOptions { get; }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> from a member asynchronously.
        /// </summary>
        /// <param name="user">The member to create a <see cref="ClaimsPrincipal"/> from.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous creation operation, containing the created <see cref="ClaimsPrincipal"/>.</returns>
        public async Task<ClaimsPrincipal> CreateAsync(Member member)
        {
            var claimsIdentity = new ClaimsIdentity(
                _identityOptions.Cookies.ApplicationCookieAuthenticationScheme,
                _identityOptions.ClaimsIdentity.UserNameClaimType,
                _identityOptions.ClaimsIdentity.RoleClaimType);

            //claimsIdentity.AddClaim(new Jering.Vak.DatabaseInterface.Claim(_identityOptions.ClaimsIdentity.UserIdClaimType, member.MemberId.ToString()));
            //claimsIdentity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.UserNameClaimType, member.Email));
            //claimsIdentity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.SecurityStampClaimType, member.SecurityStamp));

            IEnumerable<Role> roles = await _memberRepository.GetMemberRolesAsync(member.MemberId);
            foreach (Role role in roles)
            {
                //claimsIdentity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.RoleClaimType, role.Name));
                claimsIdentity.AddClaims(await _roleRepository.GetRoleClaimsAsync(role.RoleId));
            }

            claimsIdentity.AddClaims(await _memberRepository.GetMemberClaimsAsync(member.MemberId));

            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
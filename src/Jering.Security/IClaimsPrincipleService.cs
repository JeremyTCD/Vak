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
    public interface IClaimsPrincipalService<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> from <paramref name="account"/> and <paramref name="authProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authScheme"></param>
        /// <param name="authProperties"></param>
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(TAccount account, string authScheme, AuthenticationProperties authProperties);

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> with an account id claim.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="authScheme"></param>
        /// <returns></returns>
        ClaimsPrincipal CreateClaimsPrincipal(int accountId, string authScheme);

        /// <summary>
        /// Updates a <see cref="ClaimsPrincipal"/> with values from <paramref name="account" />. Does not perform update if
        /// account id does not correspond to id of account that <paramref name="claimsPrincipal"/> represents.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="claimsPrincipal"></param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="claimsPrincipal"/> is not valid</exception>
        void UpdateClaimsPrincipal(TAccount account, ClaimsPrincipal claimsPrincipal);
    }
}
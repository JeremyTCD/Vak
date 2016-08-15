using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an API for managing Account security.
    /// </summary>
    public interface IAccountSecurityServices<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns></returns>
        Task ApplicationSignInAsync(TAccount account, AuthenticationProperties authenticationProperties);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns></returns>
        Task<ApplicationSignInResult> ApplicationPasswordSignInAsync(string email, string password, AuthenticationProperties authenticationProperties);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task SignOutAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> ConfirmEmailAsync(int accountId, string token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        Task<bool> SendConfirmationEmailAsync(int accountId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<bool> SendConfirmationEmailAsync(TAccount account);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> UpdatePasswordAsync(TAccount account, string password, string token);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task SendTwoFactorTokenByEmailAsync(TAccount account);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<TAccount> GetTwoFactorAccountAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPersistent"></param>
        /// <returns></returns>
        Task<TwoFactorSignInResult> TwoFactorSignInAsync(string token, bool isPersistent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task CreateTwoFactorCookieAsync(TAccount account);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenServiceName"></param>
        /// <param name="tokenService"></param>
        void RegisterTokenProvider(string tokenServiceName, ITokenService<TAccount> tokenService);
    }
}

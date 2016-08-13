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
    /// Provides an API for managing Accounts.
    /// </summary>
    public interface IAccountSecurityServices<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// Signs in specified <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task ApplicationSignInAsync(TAccount account, AuthenticationProperties authenticationProperties);

        /// <summary>
        /// Signs in account with specified <paramref name="email"/> and <paramref name="password"/> using 
        /// specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/> that returns true if sign in is successful and false otherwise.</returns>
        Task<ApplicationSignInResult> ApplicationPasswordSignInAsync(string email, string password, AuthenticationProperties authenticationProperties);

        /// <summary>
        /// Signs out account that sent request. 
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        Task SignOutAsync();

        /// <summary>
        /// Validates <paramref name="token"/>. If valid, sets EmailConfirmed to true for account with 
        /// specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="token"></param>
        /// <returns>A <see cref="Task"/> that returns true if <paramref name="token"/> is valid and EmailConfirmed 
        /// updates successfully, false otherwise.</returns>
        Task<bool> ConfirmEmailAsync(int accountId, string token);

        /// <summary>
        /// Sends confirmation email to account with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>A <see cref="Task"/> that returns true if confirmation email is sent successfully.</returns>
        Task<bool> SendConfirmationEmailAsync(int accountId);

        /// <summary>
        /// Sends confirmation email to specified <paramref name="account"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>A <see cref="Task"/> that returns true if confirmation email is sent successfully.</returns>
        Task<bool> SendConfirmationEmailAsync(TAccount account);

        /// <summary>
        /// Validates <paramref name="token"/>. If valid, updates PasswordHash for account.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns>A <see cref="Task"/> that returns true if <paramref name="token"/> is valid and PasswordHash 
        /// updates successfully, false otherwise.</returns>
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
        Task<TwoFactorSignInResult> TwoFactorSignInAsync(string token, bool isPersistent)
    }
}

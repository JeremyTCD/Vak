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
    public interface IAccountSecurityService<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, ITokenService<TAccount>> TokenServices { get; }

        /// <summary>
        /// 
        /// </summary>
        string ConfirmEmailTokenPurpose { get; }

        /// <summary>
        /// 
        /// </summary>
        string TwoFactorTokenPurpose { get; }

        /// <summary>
        /// 
        /// </summary>
        string ResetPasswordTokenPurpose { get; }

        /// <summary>
        /// 
        /// </summary>
        string ConfirmAlternativeEmailTokenPurpose { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns></returns>
        Task LogInAsync(TAccount account, AuthenticationProperties authenticationProperties);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task RefreshLogInAsync(TAccount account);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns></returns>
        Task<PasswordLogInResult<TAccount>> PasswordLogInAsync(string email, string password, AuthenticationProperties authenticationProperties);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task LogOffAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ConfirmEmailResult> ConfirmEmailAsync(string token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        string GetToken(string tokenService, string purpose, TAccount account);

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
        Task<TwoFactorLogInResult<TAccount>> TwoFactorLogInAsync(string token, bool isPersistent);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<CreateAccountResult<TAccount>> CreateAccountAsync(string email, string password);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<TAccount> GetLoggedInAccountAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        Task<TAccount> GetLoggedInAccountAsync(ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        ValidateTokenResult ValidateToken(string tokenService, string purpose, TAccount account, string token);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetLoggedInAccountEmail();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="twoFactorEnabled"></param>
        Task UpdateTwoFactorEnabledAsync(int accountId, bool twoFactorEnabled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task<ResetPasswordResult> ResetPasswordAsync(string token, string email, string newPassword);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task<ChangePasswordResult> ChangePasswordAsync(string currentPassword, string newPassword);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newEmail"></param>
        /// <returns></returns>
        Task<ChangeEmailResult> ChangeEmailAsync(string password, string newEmail);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newAlternativeEmail"></param>
        /// <returns></returns>
        Task<ChangeAlternativeEmailResult> ChangeAlternativeEmailAsync(string password, string newAlternativeEmail);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newDisplayName"></param>
        /// <returns></returns>
        Task<ChangeDisplayNameResult> ChangeDisplayNameAsync(string password, string newDisplayName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="linkDomain"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <returns></returns>
        Task SendResetPasswordEmailAsync(TAccount account, string linkDomain, string subject, string messageFormat);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        /// <returns></returns>
        Task SendEmailVerificationEmailAsync(TAccount account, string subject, string messageFormat, string linkDomain);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        /// <returns></returns>
        Task SendAlternativeEmailVerificationEmailAsync(TAccount account, string subject, string messageFormat, string linkDomain);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <returns></returns>
        Task SendTwoFactorCodeEmailAsync(TAccount account, string subject, string messageFormat);
    }
}

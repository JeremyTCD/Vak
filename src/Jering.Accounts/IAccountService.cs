using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.Accounts.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Jering.Security;

namespace Jering.Accounts
{
    /// <summary>
    /// Provides an API for managing Account security.
    /// </summary>
    public interface IAccountService<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, ITokenService<TAccount>> TokenServices { get; }

        #region Crypto
        /// <summary>
        /// Adds <paramref name="tokenService"/> to <see cref="TokenServices"/> 
        /// </summary>
        /// <param name="tokenServiceName"></param>
        /// <param name="tokenService"></param>
        void RegisterTokenProvider(string tokenServiceName, ITokenService<TAccount> tokenService);

        /// <summary>
        /// Generates a token using specified <paramref name="tokenService"/>
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns>
        /// Token
        /// </returns>
        string GetToken(string tokenService, string purpose, TAccount account);

        /// <summary>
        /// Validates <paramref name="token"/> 
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <param name="token"></param>
        /// <returns>
        /// <see cref="ValidateTokenResult"/> returned by <see cref="ITokenService{TAccount}.ValidateToken(string, string, TAccount)"/>.
        /// </returns>
        ValidateTokenResult ValidateToken(string tokenService, string purpose, TAccount account,
            string token);
        #endregion

        #region Application Session
        /// <summary>
        /// Logs in <paramref name="account"/> using specified <paramref name="authProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authProperties"></param>
        Task ApplicationLogInAsync(TAccount account, AuthenticationProperties authProperties);

        /// <summary>
        /// Refreshes log in for <paramref name="account"/>.
        /// </summary>
        /// <param name="account"></param>
        Task RefreshApplicationLogInAsync(TAccount account);

        /// <summary>
        /// Logs off account that sent request. 
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        Task ApplicationLogOffAsync();

        /// <summary>
        /// Gets email account of logged in account using <see cref="HttpContext.User"/> .
        /// </summary>
        /// <returns>
        /// Email if it exists. 
        /// Null otherwise.
        /// </returns>
        string GetApplicationAccountEmail();

        /// <summary>
        /// Gets logged in account for <see cref="HttpContext.User"/>.
        /// </summary>
        /// <returns>
        /// An account if there is a logged in account.
        /// Null otherwise.
        /// </returns>
        Task<TAccount> GetApplicationAccountAsync();
        #endregion

        #region Two Factor Session
        /// <summary>
        /// Instructs cookie auth middleware to add two factor cookie to <see cref="HttpResponse"/>.
        /// </summary>
        /// <param name="account"></param>
        Task TwoFactorLogInAsync(TAccount account);

        /// <summary>
        /// Gets account using two factor cookie's account id value. 
        /// </summary>
        /// <returns>
        /// Null if two factor cookie is invalid.
        /// Null if two factor cookie does not have an account id value.
        /// An account if two factor cookie exists and has account id claim.
        /// </returns>
        Task<TAccount> GetTwoFactorAccountAsync();

        /// <summary>
        /// Logs off two factor account
        /// </summary>
        Task TwoFactorLogOffAsync();
        #endregion

        #region Session
        Task<LogInActionResult> LogInActionAsync(string email, string password, bool isPersistent);
        Task LogOffActionAsync();
        Task<TwoFactorLogInActionResult> TwoFactorLogInActionAsync(string code, bool isPersistent);
        #endregion

        #region Account Management
        Task<SignUpActionResult> SignUpActionAsync(string email, string password);

        Task<SendResetPasswordEmailActionResult> SendResetPasswordEmailActionAsync(string email);

        Task<ResetPasswordActionResult> ResetPasswordActionAsync(string email, string token, string newPassword);

        Task<TAccount> GetAccountDetailsActionAsync();

        Task<SetPasswordActionResult> SetPasswordActionAsync(string currentPassword, string newPassword);

        Task<SetEmailActionResult> SetEmailActionAsync(string currentPassword, string newEmail);

        Task<SetAltEmailActionResult> SetAltEmailActionAsync(string password, string newAltEmail);

        Task<SetDisplayNameActionResult> SetDisplayNameActionAsync(string password, string newDisplayName);

        Task<SetTwoFactorEnabledActionResult> SetTwoFactorEnabledActionAsync(bool enabled);

        Task<SetEmailVerifiedActionResult> SetEmailVerifiedActionAsync(string token);

        Task<SetAltEmailVerifiedActionResult> SetAltEmailVerifiedActionAsync(string token);

        Task<SendEmailVerificationEmailActionResult> SendEmailVerificationEmailActionAsync();

        Task<SendAltEmailVerificationEmailActionResult> SendAltEmailVerificationEmailActionAsync();

        Task<TwoFactorVerifyEmailActionResult> TwoFactorVerifyEmailActionAsync(string code);
        #endregion

        #region Utility
        Task<bool> CheckEmailInUseActionAsync(string email);
        Task<bool> CheckDisplayNameInUseActionAsync(string displayName);
        #endregion
    }
}

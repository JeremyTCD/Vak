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
    public interface IAccountsService<TAccount> where TAccount : IAccount
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
        string ConfirmAltEmailTokenPurpose { get; }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool ValidatePassword(TAccount account, string password);
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

        #region Account Management
        // TODO add tests
        /// <summary>
        /// Creates an account with the credentials <paramref name="email"/> and <paramref name="password"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// <see cref="CreateAccountResult{TAccount}"/> with <see cref="CreateAccountResult{TAccount}.InvalidEmail"/> set to true if <paramref name="email"/> is in use.
        /// <see cref="CreateAccountResult{TAccount}"/> with <see cref="CreateAccountResult{TAccount}.Succeeded"/> set to true if accountis created successfully. 
        /// </returns>
        /// <exception cref="NullReferenceException">Throws exception if account creation fails unexpectedly</exception>
        Task<CreateAccountResult<TAccount>> CreateAccountAsync(string email, string password);

        /// <summary>
        /// Sets password of <paramref name="account"/> to <paramref name="newPassword"/>.
        /// </summary>
        /// <param name="newPassword"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetPasswordResult"/> with <see cref="SetPasswordResult.Succeeded"/> set to true if 
        /// password change succeeds.
        /// <see cref="SetPasswordResult"/> with <see cref="SetPasswordResult.AlreadySet"/> set to true if
        /// account password is already equal to <paramref name="newPassword"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        Task<SetPasswordResult> SetPasswordAsync(TAccount account, string newPassword);

        /// <summary>
        /// Sets email of <paramref name="account"/> to <paramref name="newEmail"/>.
        /// </summary>
        /// <param name="newEmail"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetEmailResult"/> with <see cref="SetEmailResult.Succeeded"/> set to true if 
        /// email change succeeds.
        /// <see cref="SetEmailResult"/> with <see cref="SetEmailResult.InvalidNewEmail"/> set to true if
        /// <paramref name="newEmail"/> is in use.
        /// <see cref="SetEmailResult"/> with <see cref="SetEmailResult.GetAlreadySetResult"/> set to true if
        /// account email is already equal to <paramref name="newEmail"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        Task<SetEmailResult> SetEmailAsync(TAccount account, string newEmail);

        /// <summary>
        /// Sets alternative email of <paramref name="account"/> to <paramref name="newAltEmail"/>.
        /// </summary>
        /// <param name="newAltEmail"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetAltEmailResult"/> with <see cref="SetAltEmailResult.Succeeded"/> set to true if 
        /// alternative email change succeeds.
        /// <see cref="SetAltEmailResult"/> with <see cref="SetAltEmailResult.InvalidNewAltEmail"/> set to true if
        /// <paramref name="newAltEmail"/> is in use.
        /// <see cref="SetAltEmailResult"/> with <see cref="SetAltEmailResult.GetAlreadySetResult"/> set to true if
        /// account alternative email is already equal to <paramref name="newAltEmail"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        Task<SetAltEmailResult> SetAltEmailAsync(TAccount account, string newAltEmail);

        /// <summary>
        /// Sets display name of <paramref name="account"/> to <paramref name="newDisplayName"/>.
        /// </summary>
        /// <param name="newDisplayName"></param>
        /// <param name="account"></param>
        /// <returns>
        /// <see cref="SetDisplayNameResult"/> with <see cref="SetDisplayNameResult.Succeeded"/> set to true if 
        /// display name change succeeds.
        /// <see cref="SetDisplayNameResult"/> with <see cref="SetDisplayNameResult.InvalidNewDisplayName"/> set to true if
        /// <paramref name="newDisplayName"/> is in use.
        /// <see cref="SetDisplayNameResult"/> with <see cref="SetDisplayNameResult.GetAlreadySetResult"/> set to true if
        /// account display name is already equal to <paramref name="newDisplayName"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        Task<SetDisplayNameResult> SetDisplayNameAsync(TAccount account, string newDisplayName);

        /// <summary>
        /// Sets two factor enabled of <paramref name="account"/> to <paramref name="enabled"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="enabled"></param>
        /// <returns>
        /// <see cref="SetTwoFactorEnabledResult"/> with <see cref="SetTwoFactorEnabledResult.Succeeded"/> set to true if 
        /// operation succeeds.
        /// <see cref="SetTwoFactorEnabledResult"/> with <see cref="SetTwoFactorEnabledResult.GetEmailUnverifiedResult"/> set to true if
        /// account email is unverified.
        /// <see cref="SetTwoFactorEnabledResult"/> with <see cref="SetTwoFactorEnabledResult.GetAlreadySetResult"/> set to true if
        /// account two factor enabled is already equal to <paramref name="enabled"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        Task<SetTwoFactorEnabledResult> SetTwoFactorEnabledAsync(TAccount account, bool enabled);

        /// <summary>
        /// Sets email verified of <paramref name="account"/> to <paramref name="verified"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="verified"></param>
        /// <returns>
        /// <see cref="SetEmailVerifiedResult"/> with <see cref="SetEmailVerifiedResult.Succeeded"/> set to true if 
        /// operation succeeds.
        /// <see cref="SetEmailVerifiedResult"/> with <see cref="SetEmailVerifiedResult.GetAlreadySetResult"/> set to true if
        /// account email verified is already equal to <paramref name="verified"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        Task<SetEmailVerifiedResult> SetEmailVerifiedAsync(TAccount account, bool verified);

        /// <summary>
        /// Sets alternative email verified of <paramref name="account"/> to <paramref name="verified"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="verified"></param>
        /// <returns>
        /// <see cref="SetAltEmailVerifiedResult"/> with <see cref="SetAltEmailVerifiedResult.Succeeded"/> set to true if 
        /// operation succeeds.
        /// <see cref="SetAltEmailVerifiedResult"/> with <see cref="SetAltEmailVerifiedResult.GetAlreadySetResult"/> set to true if
        /// account alternative email verified is already equal to <paramref name="verified"/>.
        /// </returns>
        /// <exception cref="Exception">Thrown if database update fails unexpectedly</exception>
        Task<SetAltEmailVerifiedResult> SetAltEmailVerifiedAsync(TAccount account,
            bool verified);

        /// <summary>
        /// Sends reset password email. Inserts <paramref name="linkDomain"/>, token and account email into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        /// <param name="email"></param>
        Task SendResetPasswordEmailAsync(TAccount account, string email, string subject,
            string messageFormat, string linkDomain);

        /// <summary>
        /// Sends email verification email. Inserts <paramref name="linkDomain"/>, token and account id into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        Task SendEmailVerificationEmailAsync(TAccount account, string subject,
            string messageFormat, string linkDomain);

        /// <summary>
        /// Sends alternative email verification email. Inserts <paramref name="linkDomain"/>, token and account id into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        /// <param name="linkDomain"></param>
        /// <returns>
        /// <see cref="SendAltEmailVerificationEmailResult"/> with <see cref="SendAltEmailVerificationEmailResult.Succeeded"/> set to true if 
        /// email is sent successfully.
        /// <see cref="SendAltEmailVerificationEmailResult"/> with <see cref="SendAltEmailVerificationEmailResult.InvalidAltEmail"/> set to true if 
        /// account alt email is null or empty. 
        /// </returns>
        Task<SendAltEmailVerificationEmailResult> SendAltEmailVerificationEmailAsync(TAccount account,
            string subject, string messageFormat, string linkDomain);

        /// <summary>
        /// Sends two factor code email. Inserts token into <paramref name="messageFormat"/>
        /// to form email message.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="subject"></param>
        /// <param name="messageFormat"></param>
        Task SendTwoFactorCodeEmailAsync(TAccount account, string subject, string messageFormat);
        #endregion


    }
}

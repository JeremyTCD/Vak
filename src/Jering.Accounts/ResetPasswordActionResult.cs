using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.ResetPasswordActionAsync(string, string, string)"/>.
    /// </summary>
    public enum ResetPasswordActionResult
    {
        InvalidEmail,
        InvalidToken,
        InvalidNewPassword,
        Success
    }
}

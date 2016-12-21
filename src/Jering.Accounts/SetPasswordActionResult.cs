using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SetPasswordActionAsync(string, string)"/>.
    /// </summary>
    public enum SetPasswordActionResult
    {
        Success,
        InvalidCurrentPassword,
        AlreadySet,
        NoLoggedInAccount
    }
}

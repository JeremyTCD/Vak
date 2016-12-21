using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SetDisplayNameActionAsync(string, string)"/>.
    /// </summary>
    public enum SetDisplayNameActionResult
    {
        Success,
        InvalidPassword,
        AlreadySet,
        DisplayNameInUse,
        NoLoggedInAccount
    }
}

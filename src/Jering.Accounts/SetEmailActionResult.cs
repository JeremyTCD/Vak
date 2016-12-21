using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SetEmailActionAsync(string, string)"/>.
    /// </summary>
    public enum SetEmailActionResult
    {
        Success,
        InvalidPassword,
        AlreadySet,
        EmailInUse,
        NoLoggedInAccount
    }
}

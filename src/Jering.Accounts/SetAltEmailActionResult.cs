using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SetAltEmailActionAsync(string, string)"/>.
    /// </summary>
    public enum SetAltEmailActionResult
    {
        Success,
        InvalidPassword,
        AlreadySet,
        NoLoggedInAccount
    }
}

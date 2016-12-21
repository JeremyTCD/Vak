using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SetEmailVerifiedActionAsync(string)"/>.
    /// </summary>
    public enum SetEmailVerifiedActionResult
    {
        InvalidToken,
        Success,
        AlreadySet,
        NoLoggedInAccount
    }
}

using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SetAltEmailVerifiedActionAsync(string)"/>.
    /// </summary>
    public enum SetAltEmailVerifiedActionResult
    {
        Success,
        AlreadySet,
        InvalidToken,
        NoLoggedInAccount
    }
}

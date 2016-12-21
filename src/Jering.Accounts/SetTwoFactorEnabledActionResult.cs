using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SetTwoFactorEnabledActionAsync(bool)"/>.
    /// </summary>
    public enum SetTwoFactorEnabledActionResult
    {
        Success,
        EmailUnverified,
        AlreadySet,
        NoLoggedInAccount
    }
}

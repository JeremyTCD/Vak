using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SignUpActionAsync(string, string)"/>.
    /// </summary>
    public enum SignUpActionResult
    {
        EmailInUse,
        Success
    }
}

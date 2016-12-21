namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SendAltEmailVerificationEmailActionAsync"/>.
    /// </summary>
    public enum SendAltEmailVerificationEmailActionResult
    {
        NoAltEmail,
        Success,
        NoLoggedInAccount
    }
}

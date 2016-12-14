using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetTwoFactorEnabledAsync(TAccount, bool)()"/>.
    /// </summary>
    public class SetTwoFactorEnabledResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EmailUnverified { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AlreadySet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetTwoFactorEnabledResult GetAlreadySetResult()
        {
            return new SetTwoFactorEnabledResult { AlreadySet = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetTwoFactorEnabledResult GetEmailUnverifiedResult()
        {
            return new SetTwoFactorEnabledResult { EmailUnverified = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetTwoFactorEnabledResult GetSucceededResult()
        {
            return new SetTwoFactorEnabledResult { Succeeded = true };
        }
    }
}

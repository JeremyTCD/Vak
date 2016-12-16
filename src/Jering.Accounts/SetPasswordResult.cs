using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetPasswordHashAsync(TAccount, string)"/>.
    /// </summary>
    public class SetPasswordHashResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AlreadySet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetPasswordHashResult GetSucceededResult()
        {
            return new SetPasswordHashResult { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetPasswordHashResult GetAlreadySetResult()
        {
            return new SetPasswordHashResult { AlreadySet = true };
        }
    }
}

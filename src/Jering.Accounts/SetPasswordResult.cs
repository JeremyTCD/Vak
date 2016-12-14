using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetPasswordAsync(TAccount, string)"/>.
    /// </summary>
    public class SetPasswordResult
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
        public static SetPasswordResult GetSucceededResult()
        {
            return new SetPasswordResult { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetPasswordResult GetAlreadySetResult()
        {
            return new SetPasswordResult { AlreadySet = true };
        }
    }
}

using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetDisplayNameAsync(TAccount, string)"/>.
    /// </summary>
    public class SetDisplayNameResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool InvalidNewDisplayName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AlreadySet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetDisplayNameResult GetAlreadySetResult()
        {
            return new SetDisplayNameResult { AlreadySet = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetDisplayNameResult GetInvalidNewDisplayNameResult()
        {
            return new SetDisplayNameResult { InvalidNewDisplayName = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetDisplayNameResult GetSucceededResult()
        {
            return new SetDisplayNameResult { Succeeded = true };
        }
    }
}

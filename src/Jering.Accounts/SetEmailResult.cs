using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetEmailAsync(TAccount, string)"/>.
    /// </summary>
    public class SetEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool InvalidNewEmail { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool AlreadySet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetEmailResult GetAlreadySetResult()
        {
            return new SetEmailResult { AlreadySet = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetEmailResult GetInvalidNewEmailResult()
        {
            return new SetEmailResult { InvalidNewEmail = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetEmailResult GetSucceededResult()
        {
            return new SetEmailResult { Succeeded = true };
        }
    }
}

using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetEmailVerifiedAsync()"/>.
    /// </summary>
    public class SetEmailVerifiedResult
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
        public static SetEmailVerifiedResult GetAlreadySetResult()
        {
            return new SetEmailVerifiedResult { AlreadySet = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetEmailVerifiedResult GetSucceededResult()
        {
            return new SetEmailVerifiedResult { Succeeded = true };
        }
    }
}

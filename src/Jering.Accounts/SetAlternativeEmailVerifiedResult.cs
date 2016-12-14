using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetAltEmailVerifiedAsync()"/>.
    /// </summary>
    public class SetAltEmailVerifiedResult
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
        public static SetAltEmailVerifiedResult GetAlreadySetResult()
        {
            return new SetAltEmailVerifiedResult { AlreadySet = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetAltEmailVerifiedResult GetSucceededResult()
        {
            return new SetAltEmailVerifiedResult { Succeeded = true };
        }
    }
}

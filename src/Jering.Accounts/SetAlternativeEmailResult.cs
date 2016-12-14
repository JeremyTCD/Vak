using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SetAltEmailAsync(TAccount, string)"/>.
    /// </summary>
    public class SetAltEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool InvalidNewAltEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AlreadySet { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetAltEmailResult GetAlreadySetResult()
        {
            return new SetAltEmailResult { AlreadySet = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetAltEmailResult GetInvalidNewAltEmailResult()
        {
            return new SetAltEmailResult { InvalidNewAltEmail = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SetAltEmailResult GetSucceededResult()
        {
            return new SetAltEmailResult { Succeeded = true };
        }
    }
}

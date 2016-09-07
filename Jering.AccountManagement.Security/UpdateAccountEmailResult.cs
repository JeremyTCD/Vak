using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityServices{TAccount}.UpdateAccountEmailAsync"/>.
    /// </summary>
    public class UpdateAccountEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EmailInUse { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountEmailResult GetSucceededResult()
        {
            return new UpdateAccountEmailResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountEmailResult GetEmailInUseResult()
        {
            return new UpdateAccountEmailResult() { EmailInUse = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountEmailResult GetFailedResult()
        {
            return new UpdateAccountEmailResult() { Failed = true };
        }
    }
}

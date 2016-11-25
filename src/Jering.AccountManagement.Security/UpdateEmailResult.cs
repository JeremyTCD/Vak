using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.UpdateEmailAsync"/>.
    /// </summary>
    public class UpdateEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool InvalidEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateEmailResult GetSucceededResult()
        {
            return new UpdateEmailResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateEmailResult GetInvalidEmailResult()
        {
            return new UpdateEmailResult() { InvalidEmail = true };
        }
    }
}

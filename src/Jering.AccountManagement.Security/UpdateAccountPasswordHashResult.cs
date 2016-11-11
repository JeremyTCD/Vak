using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.UpdateAccountPasswordHashAsync"/>.
    /// </summary>
    public class UpdateAccountPasswordHashResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountPasswordHashResult GetSucceededResult()
        {
            return new UpdateAccountPasswordHashResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountPasswordHashResult GetFailedResult()
        {
            return new UpdateAccountPasswordHashResult() { Failed = true };
        }
    }
}

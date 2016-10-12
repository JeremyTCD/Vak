using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityServices{TAccount}.UpdateAccountTwoFactorEnabledAsync"/>.
    /// </summary>
    public class UpdateAccountTwoFactorEnabledResult
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
        public static UpdateAccountTwoFactorEnabledResult GetSucceededResult()
        {
            return new UpdateAccountTwoFactorEnabledResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountTwoFactorEnabledResult GetFailedResult()
        {
            return new UpdateAccountTwoFactorEnabledResult() { Failed = true };
        }
    }
}

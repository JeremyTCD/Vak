using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityServices{TAccount}.UpdateAccountDisplayNameAsync"/>.
    /// </summary>
    public class UpdateAccountDisplayNameResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool DisplayNameInUse { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountDisplayNameResult GetSucceededResult()
        {
            return new UpdateAccountDisplayNameResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountDisplayNameResult GetDisplayNameInUseResult()
        {
            return new UpdateAccountDisplayNameResult() { DisplayNameInUse = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountDisplayNameResult GetFailedResult()
        {
            return new UpdateAccountDisplayNameResult() { Failed = true };
        }
    }
}

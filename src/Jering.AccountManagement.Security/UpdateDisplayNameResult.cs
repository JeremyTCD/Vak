using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.UpdateDisplayNameAsync"/>.
    /// </summary>
    public class UpdateDisplayNameResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool InvalidDisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateDisplayNameResult GetSucceededResult()
        {
            return new UpdateDisplayNameResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateDisplayNameResult GetInvalidDisplayNameResult()
        {
            return new UpdateDisplayNameResult() { InvalidDisplayName = true };
        }
    }
}

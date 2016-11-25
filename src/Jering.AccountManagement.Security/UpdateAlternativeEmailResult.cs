using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.UpdateAlternativeEmailAsync"/>.
    /// </summary>
    public class UpdateAlternativeEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool InvalidAlternativeEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAlternativeEmailResult GetSucceededResult()
        {
            return new UpdateAlternativeEmailResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAlternativeEmailResult GetInvalidAlternativeEmailResult()
        {
            return new UpdateAlternativeEmailResult() { InvalidAlternativeEmail = true };
        }
    }
}

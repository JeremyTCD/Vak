using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.UpdateAccountAlternativeEmailAsync"/>.
    /// </summary>
    public class UpdateAccountAlternativeEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AlternativeEmailInUse { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountAlternativeEmailResult GetSucceededResult()
        {
            return new UpdateAccountAlternativeEmailResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountAlternativeEmailResult GetAlternativeEmailInUseResult()
        {
            return new UpdateAccountAlternativeEmailResult() { AlternativeEmailInUse = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateAccountAlternativeEmailResult GetFailedResult()
        {
            return new UpdateAccountAlternativeEmailResult() { Failed = true };
        }
    }
}

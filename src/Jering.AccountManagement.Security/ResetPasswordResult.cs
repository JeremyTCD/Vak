using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.ResetPasswordAsync(string, string, string)"/>.
    /// </summary>
    public class ResetPasswordResult
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
        public bool InvalidToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ExpiredToken { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ResetPasswordResult GetExpiredTokenResult()
        {
            return new ResetPasswordResult() { ExpiredToken = true };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ResetPasswordResult GetInvalidEmailResult()
        {
            return new ResetPasswordResult() { InvalidEmail = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ResetPasswordResult GetSucceededResult()
        {
            return new ResetPasswordResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ResetPasswordResult GetInvalidTokenResult()
        {
            return new ResetPasswordResult() { InvalidToken = true };
        }
    }
}

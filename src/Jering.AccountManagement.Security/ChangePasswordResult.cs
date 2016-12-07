using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.ChangePasswordAsync(string, string)"/>.
    /// </summary>
    public class ChangePasswordResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool InvalidCurrentPassword { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool NotLoggedIn { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangePasswordResult GetInvalidCurrentPasswordResult()
        {
            return new ChangePasswordResult() { InvalidCurrentPassword = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangePasswordResult GetSucceededResult()
        {
            return new ChangePasswordResult() { Succeeded = true};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangePasswordResult GetNotLoggedInResult()
        {
            return new ChangePasswordResult() { NotLoggedIn = true };
        }
    }
}

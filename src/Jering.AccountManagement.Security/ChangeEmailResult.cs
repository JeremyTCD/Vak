using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.ChangeEmailAsync(string, string)"/>.
    /// </summary>
    public class ChangeEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool InvalidPassword { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool NotLoggedIn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool InvalidNewEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeEmailResult GetInvalidNewEmailResult()
        {
            return new ChangeEmailResult { InvalidNewEmail = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeEmailResult GetInvalidPasswordResult()
        {
            return new ChangeEmailResult { InvalidPassword = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeEmailResult GetSucceededResult()
        {
            return new ChangeEmailResult { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeEmailResult GetNotLoggedInResult()
        {
            return new ChangeEmailResult { NotLoggedIn = true };
        }
    }
}

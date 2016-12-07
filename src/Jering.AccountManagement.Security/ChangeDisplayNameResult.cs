using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.ChangeDisplayNameAsync(string, string)"/>.
    /// </summary>
    public class ChangeDisplayNameResult
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
        public bool InvalidNewDisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeDisplayNameResult GetInvalidNewDisplayNameResult()
        {
            return new ChangeDisplayNameResult { InvalidNewDisplayName = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeDisplayNameResult GetInvalidPasswordResult()
        {
            return new ChangeDisplayNameResult { InvalidPassword = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeDisplayNameResult GetSucceededResult()
        {
            return new ChangeDisplayNameResult { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeDisplayNameResult GetNotLoggedInResult()
        {
            return new ChangeDisplayNameResult { NotLoggedIn = true };
        }
    }
}

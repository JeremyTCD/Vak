using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.ChangeAlternativeEmailAsync(string, string)"/>.
    /// </summary>
    public class ChangeAlternativeEmailResult
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
        public bool InvalidNewAlternativeEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeAlternativeEmailResult GetInvalidNewAlternativeEmailResult()
        {
            return new ChangeAlternativeEmailResult { InvalidNewAlternativeEmail = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeAlternativeEmailResult GetInvalidPasswordResult()
        {
            return new ChangeAlternativeEmailResult { InvalidPassword = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeAlternativeEmailResult GetSucceededResult()
        {
            return new ChangeAlternativeEmailResult { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ChangeAlternativeEmailResult GetNotLoggedInResult()
        {
            return new ChangeAlternativeEmailResult { NotLoggedIn = true };
        }
    }
}

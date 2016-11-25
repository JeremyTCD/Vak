using Jering.AccountManagement.DatabaseInterface;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Results for <see cref="IAccountSecurityService{TAccount}.CreateAccountAsync"/>.
    /// </summary>
    public class CreateAccountResult<TAccount> where TAccount : IAccount
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
        public TAccount Account { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static CreateAccountResult<TAccount> GetSucceededResult(TAccount account)
        {
            return new CreateAccountResult<TAccount>() { Succeeded = true, Account = account };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CreateAccountResult<TAccount> GetInvalidEmailResult()
        {
            return new CreateAccountResult<TAccount>() { InvalidEmail = true };
        }
    }
}

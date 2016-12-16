using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts.DatabaseInterface
{
    /// <summary>
    /// Results for <see cref="IAccountRepository{TAccount}.CreateAccountAsync(string, string)"/>.
    /// </summary>
    public class CreateResult<TAccount> where TAccount : IAccount
    { 
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool DuplicateRow { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TAccount Account { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CreateResult<TAccount> GetSucceededResult(TAccount account)
        {
            return new CreateResult<TAccount>() { Succeeded = true, Account = account };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CreateResult<TAccount> GetDuplicateRowResult()
        {
            return new CreateResult<TAccount>() { DuplicateRow = true };
        }
    }
}


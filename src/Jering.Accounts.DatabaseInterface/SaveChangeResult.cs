using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts.DatabaseInterface
{
    /// <summary>
    /// Result for <see cref="IAccountRepository{TAccount}.Update*"/>.
    /// </summary>
    public class SaveChangeResult<TAccount> where TAccount : IAccount
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
        public bool InvalidRowVersionOrAccountId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TAccount Account { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SaveChangeResult<TAccount> GetSucceededResult(TAccount account)
        {
            return new SaveChangeResult<TAccount>() { Succeeded = true, Account = account };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SaveChangeResult<TAccount> GetDuplicateRowResult()
        {
            return new SaveChangeResult<TAccount>() { DuplicateRow = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SaveChangeResult<TAccount> GetInvalidRowVersionOrAccountIdResult()
        {
            return new SaveChangeResult<TAccount>() { InvalidRowVersionOrAccountId = true };
        }
    }
}


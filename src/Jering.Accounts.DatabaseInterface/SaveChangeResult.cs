using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts.DatabaseInterface
{
    /// <summary>
    /// Result for <see cref="IAccountRepository{TAccount}.Update*"/>.
    /// </summary>
    public class UpdateResult<TAccount> where TAccount : IAccount
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
        public bool AlreadyUpdated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TAccount Account { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateResult<TAccount> GetSucceededResult(TAccount account)
        {
            return new UpdateResult<TAccount>() { Succeeded = true, Account = account };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateResult<TAccount> GetAlreadyUpdatedResult()
        {
            return new UpdateResult<TAccount>() { AlreadyUpdated = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateResult<TAccount> GetDuplicateRowResult()
        {
            return new UpdateResult<TAccount>() { DuplicateRow = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UpdateResult<TAccount> GetInvalidRowVersionOrAccountIdResult()
        {
            return new UpdateResult<TAccount>() { InvalidRowVersionOrAccountId = true };
        }
    }
}


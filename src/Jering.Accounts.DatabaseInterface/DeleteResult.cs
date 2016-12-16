using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts.DatabaseInterface
{
    /// <summary>
    /// Result for <see cref="IAccountRepository{TAccount}.DeleteAccountAsync(int)"/>.
    /// </summary>
    public class DeleteResult 
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool InvalidRowVersionOrAccountId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DeleteResult GetSucceededResult()
        {
            return new DeleteResult() { Succeeded = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DeleteResult GetInvalidRowVersionOrAccountIdResult()
        {
            return new DeleteResult() { InvalidRowVersionOrAccountId = true };
        }
    }
}


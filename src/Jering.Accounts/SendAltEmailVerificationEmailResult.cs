using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountsService{TAccount}.SendAltEmailVerificationEmailAsync(TAccount, string)"/>.
    /// </summary>
    public class SendAltEmailVerificationEmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool InvalidAltEmail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SendAltEmailVerificationEmailResult GetInvalidAltEmailResult()
        {
            return new SendAltEmailVerificationEmailResult { InvalidAltEmail = true };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SendAltEmailVerificationEmailResult GetSucceededResult()
        {
            return new SendAltEmailVerificationEmailResult { Succeeded = true };
        }
    }
}

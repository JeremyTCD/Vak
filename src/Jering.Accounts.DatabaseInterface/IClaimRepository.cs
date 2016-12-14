using System.Threading.Tasks;

namespace Jering.Accounts.DatabaseInterface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IClaimRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<Claim> CreateClaimAsync(string type, string value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claimId"></param>
        /// <returns></returns>
        Task<bool> DeleteClaimAsync(int claimId);
    }
}

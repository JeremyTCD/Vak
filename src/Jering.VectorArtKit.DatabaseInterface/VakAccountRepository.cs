using Jering.Utilities;
using Jering.Accounts.DatabaseInterface.EfCore;

namespace Jering.VectorArtKit.DatabaseInterface
{
    /// <summary>
    /// Provides an interface for performing CRUD operations on account representations in a database. 
    /// Functions with varying granularity are provided to avoid reading and writing contextually superfluous 
    /// information while minimizing round trips.
    /// </summary>
    public class VakAccountRepository : EfCoreAccountRepository<VakAccount>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="timeService"></param>
        public VakAccountRepository(VakDbContext dbContext,
            ITimeService timeService) : base(dbContext, timeService)
        {
        }
    }
}

using Jering.Accounts.DatabaseInterface;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Jering.Accounts.DatabaseInterface.EfCore
{
    public static class VakDbContextExtensions
    {
        private static int _uniqueConstraintViolationErrorNumber = 2627;
        private static int _uniqueIndexViolationErrorNumber = 2601;

        /// <summary>
        /// Saves changes in repository to database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/> with <see cref="SaveChangesResult.UniqueIndexViolation"/> set to true if database operation failed
        /// due to a unique index or constraint violation.
        /// <see cref="SaveChangesResult"/> with <see cref="SaveChangesResult.ConcurrencyError"/> set to true if a concurrency error 
        /// occurred.
        /// <see cref="SaveChangesResult"/> with <see cref="SaveChangesResult.Success"/> set to true if changes save successfully. 
        /// </returns>
        public static async Task<SaveChangesResult> SaveChangesAndCatchAsync(this DbContext dbContext, 
            CancellationToken cancellationToken) 
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);

                return SaveChangesResult.Success;
            }
            catch (DbUpdateConcurrencyException)
            {
                return SaveChangesResult.ConcurrencyError;
            }
            catch (DbUpdateException exception)
            {
                SqlException sqlException = exception.GetBaseException() as SqlException;

                if (sqlException != null &&
                    sqlException.Number == _uniqueIndexViolationErrorNumber ||
                    sqlException.Number == _uniqueConstraintViolationErrorNumber)
                {
                    return SaveChangesResult.UniqueIndexViolation;
                }

                throw;
            }
        } 
    }
}

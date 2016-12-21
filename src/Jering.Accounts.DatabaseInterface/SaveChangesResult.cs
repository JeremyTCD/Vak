using Jering.Accounts.DatabaseInterface;

namespace Jering.Accounts.DatabaseInterface
{
    public enum SaveChangesResult
    {
        Success,
        UniqueIndexViolation,
        ConcurrencyError
    }
}



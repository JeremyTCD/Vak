using System.Threading.Tasks;
using System;
using Jering.Accounts.DatabaseInterface;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Jering.Utilities;

namespace Jering.Accounts.DatabaseInterface.EfCore
{
    /// <summary>
    /// Provides an interface for performing CRUD operations on account representations in a database. 
    /// Functions with varying granularity are provided to avoid reading and writing contextually superfluous 
    /// information while minimizing round trips.
    /// </summary>
    public class EfCoreAccountRepository<TAccount> : IAccountRepository<TAccount> where TAccount : class, IAccount, new()
    {
        /// <summary>
        /// 
        /// </summary>
        private DbContext _dbContext { get; }
        private ITimeService _timeService { get; }

        /// <summary>
        /// Constructs an instance of <see cref="EfCoreAccountRepository{TAccount}"/> 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="timeService"></param>
        public EfCoreAccountRepository(DbContext dbContext,
            ITimeService timeService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            _dbContext = dbContext;
            _timeService = timeService;
        }

        #region Create
        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// New account if account is created successfuly.
        /// Null if email is in use.
        /// </returns>
        public async Task<TAccount> CreateAsync(string email, string passwordHash, CancellationToken cancellationToken)
        {
            TAccount account = new TAccount
            {
                Email = email,
                PasswordHash = passwordHash,
                PasswordLastChanged = _timeService.UtcNow,
                SecurityStamp = Guid.NewGuid()
            };

            await _dbContext.AddAsync(account, cancellationToken);

            SaveChangesResult result = await _dbContext.SaveChangesAndCatchAsync(cancellationToken);

            if (result == SaveChangesResult.UniqueIndexViolation)
            {
                return null;
            }

            return account;
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates an account's password hash. Automatically updates password last changed and security stamp.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="passwordHash"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/>.
        /// </returns>
        public async Task<SaveChangesResult> UpdatePasswordHashAsync(TAccount account, string passwordHash, CancellationToken cancellationToken)
        {
            account.PasswordHash = passwordHash;
            account.PasswordLastChanged = _timeService.UtcNow;
            account.SecurityStamp = Guid.NewGuid();

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an account's email. Automatically updates security stamp.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/>.
        /// </returns>
        public async Task<SaveChangesResult> UpdateEmailAsync(TAccount account, string email, CancellationToken cancellationToken)
        {
            account.Email = email;
            account.EmailVerified = false;
            account.TwoFactorEnabled = false;
            account.SecurityStamp = Guid.NewGuid();

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an account's alt email. 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="altEmail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/>.
        /// </returns>

        public async Task<SaveChangesResult> UpdateAltEmailAsync(TAccount account, string altEmail, CancellationToken cancellationToken)
        {
            account.AltEmail = altEmail;
            account.AltEmailVerified = false;

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an account's display name. 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="displayName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/>.
        /// </returns>
        public async Task<SaveChangesResult> UpdateDisplayNameAsync(TAccount account, string displayName, CancellationToken cancellationToken)
        {
            account.DisplayName = displayName;

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an account's two factor enabled flag. 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="enabled"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/>.
        /// </returns>
        public async Task<SaveChangesResult> UpdateTwoFactorEnabledAsync(TAccount account, bool enabled, CancellationToken cancellationToken)
        {
            account.TwoFactorEnabled = enabled;

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an account's email verified flag. 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="verified"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/>.
        /// </returns>
        public async Task<SaveChangesResult> UpdateEmailVerifiedAsync(TAccount account, bool verified, CancellationToken cancellationToken)
        {
            account.EmailVerified = verified;

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an account's alt email verified. 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="verified"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// <see cref="SaveChangesResult"/>.
        /// </returns>
        public async Task<SaveChangesResult> UpdateAltEmailVerifiedAsync(TAccount account, bool verified, CancellationToken cancellationToken)
        {
            account.AltEmailVerified = verified;

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an account. Note that this function cannot be used to assign null to any field/column. 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="altEmail"></param>
        /// <param name="altEmailVerified"></param>
        /// <param name="displayName"></param>
        /// <param name="email"></param>
        /// <param name="emailVerified"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordLastChanged"></param>
        /// <param name="securityStamp"></param>
        /// <param name="twoFactorEnabled"></param>
        /// <returns>
        /// <see cref = "SaveChangesResult" />.
        /// </returns>
        public async Task<SaveChangesResult> UpdateAsync(TAccount account,
            CancellationToken cancellationToken,
            string altEmail = null,
            bool? altEmailVerified = null,
            string displayName = null,
            string email = null,
            bool? emailVerified = null, 
            string passwordHash = null,
            DateTimeOffset? passwordLastChanged = null,
            Guid? securityStamp = null,
            bool? twoFactorEnabled = null)
        {
            account.AltEmail = altEmail ?? account.AltEmail;
            account.AltEmailVerified = altEmailVerified ?? account.AltEmailVerified;
            account.DisplayName = displayName ?? account.DisplayName;
            account.Email = email ?? account.Email;
            account.EmailVerified = emailVerified ?? account.EmailVerified;
            account.PasswordHash = passwordHash ?? account.PasswordHash;
            account.PasswordLastChanged = passwordLastChanged ?? account.PasswordLastChanged;
            account.SecurityStamp = securityStamp ?? account.SecurityStamp;
            account.TwoFactorEnabled = twoFactorEnabled ?? account.TwoFactorEnabled;

            return await _dbContext.SaveChangesAndCatchAsync(cancellationToken);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes account <paramref name="account"/> from repository.
        /// </summary>
        /// <param name="account"></param>
        public virtual void RemoveAsync(TAccount account)
        {
            _dbContext.Remove(account);
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets account with specified <paramref name="accountId"/>. Attempts to retrieve account from context.
        /// If account does not exist in context, retrieves account from database.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Account with specified <paramref name="accountId"/> if it exists. 
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetAsync(int accountId, CancellationToken cancellationToken)
        {
            return await _dbContext.
                Set<TAccount>().
                FindAsync(new object[] { accountId }, cancellationToken);
        }

        /// <summary>
        /// Gets account with specified <paramref name="email"/>. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// Account with specified <paramref name="email"/> if it exists. 
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetAsync(string email, CancellationToken cancellationToken)
        {
            return await _dbContext.
                Set<TAccount>().
                FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
        }
        #endregion

        #region Check
        /// <summary>
        /// Checks whether <paramref name="email"/> is in use.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// True if <paramref name="email"/> is in use.
        /// False otherwise.
        /// </returns>
        public virtual async Task<bool> CheckEmailInUseAsync(string email, CancellationToken cancellationToken)
        {
            return await _dbContext.
                Set<TAccount>().
                AnyAsync(a => a.Email == email, cancellationToken);
        }

        /// <summary>
        /// Checks whether <paramref name="displayName"/> is in use.
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns>
        /// True if <paramref name="displayName"/> is in use.
        /// False otherwise.
        /// </returns>
        public virtual async Task<bool> CheckDisplayNameInUseAsync(string displayName, CancellationToken cancellationToken)
        {
            return await _dbContext.
                Set<TAccount>().
                AnyAsync(a => a.DisplayName == displayName, cancellationToken);
        }
        #endregion
    }
}

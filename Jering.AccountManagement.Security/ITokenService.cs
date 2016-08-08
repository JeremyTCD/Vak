using Jering.AccountManagement.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an abstraction for token services.
    /// </summary>
    public interface ITokenService<TAccount> where TAccount : IAccount
    {
        /// <summary>
        /// Generates a token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="account"></param>
        /// <returns>A string representation of the generated token.</returns>
        Task<string> GenerateToken(string purpose, TAccount account);

        /// <summary>
        /// Validates the specified <paramref name="token"/> token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="token"></param>
        /// <param name="account"></param>
        /// <returns>
        /// True if <paramref name="token"/> is valid, false otherwise.
        /// </returns>
        Task<bool> ValidateToken(string purpose, string token, TAccount account);
    }
}

﻿using Jering.Accounts.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Security
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
        string GenerateToken(string purpose, TAccount account);

        /// <summary>
        /// Validates the specified <paramref name="token"/> token for the specified <paramref name="account"/> and <paramref name="purpose"/>.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="token"></param>
        /// <param name="account"></param>
        /// <returns>
        /// True if <paramref name="token"/> is valid, false otherwise.
        /// </returns>
        ValidateTokenResult ValidateToken(string purpose, string token, TAccount account);
    }
}

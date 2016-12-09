// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for password management.
    /// </summary>
    public class PasswordServiceOptions
    {
        // secure PRNG
        private static readonly RandomNumberGenerator _defaultRng = RandomNumberGenerator.Create(); 

        /// <summary>
        /// Gets or sets the number of iterations used when hashing passwords using PBKDF2.
        /// </summary>
        /// <value>
        /// The number of iterations used when hashing passwords using PBKDF2.
        /// </value>
        public int IterationCount { get; set; } = 10000;

        /// <summary>
        /// Size of salt used to generate salt key (in bytes).
        /// </summary>
        public int SaltSize { get; set; } = 128 / 8;

        /// <summary>
        /// Size of subkey produced by PBKDF2 (in bytes, should be greater than or equal to size of hash 
        /// produced by HMAC-SHA256).
        /// </summary>
        public int SubkeySize { get; set; } = 256 / 8;

        /// <summary>
        /// Random number generator used to generate salt
        /// </summary>
        public RandomNumberGenerator Rng { get; set; } = _defaultRng;
    }
}

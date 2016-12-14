// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

namespace Jering.Security
{
    /// <summary>
    /// Implements password management
    /// </summary>
    public class PasswordService : IPasswordService
    {
        /* ====================
         * PASSWORD HASH FORMAT
         * ====================
         * 
         * PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.
         * Format: { 0x01, prf (UInt32), iter count (UInt32), salt length (UInt32), salt, subkey }
         * (All UInt32s are stored big-endian.)
         */

        private readonly int _iterCount;
        private readonly RandomNumberGenerator _rng;
        private readonly int _saltSize;
        private readonly int _subkeySize;

        /// <summary>
        /// Creates a new instance of <see cref="PasswordService"/>.
        /// </summary>
        /// <param name="optionsAccessor">The options for this instance.</param>
        /// <exception cref="InvalidOperationException">Thrown if iteration count is not a positive integer</exception>
        public PasswordService(IOptions<PasswordServiceOptions> optionsAccessor = null)
        {
            _iterCount = optionsAccessor.Value.IterationCount;

            if(_iterCount <= 0)
            {
                throw new InvalidOperationException();
            }

            _rng = optionsAccessor.Value.Rng;
            _saltSize = optionsAccessor.Value.SaltSize;
            _subkeySize = optionsAccessor.Value.SubkeySize;
        }

        /// <summary>
        /// Returns a hashed representation of the supplied <paramref name="password"/>.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed representation of the supplied <paramref name="password"/>.</returns>
        public virtual string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = new byte[_saltSize];
            _rng.GetBytes(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, _iterCount, _subkeySize);

            byte[] passwordHash = new byte[13 + _saltSize + _subkeySize];
            // Include format marker in case of future changes to hashing algorithm
            passwordHash[0] = 0x01;
            WriteNetworkByteOrder(passwordHash, 1, (uint)KeyDerivationPrf.HMACSHA256);
            WriteNetworkByteOrder(passwordHash, 5, (uint)_iterCount);
            WriteNetworkByteOrder(passwordHash, 9, (uint)_saltSize);
            Buffer.BlockCopy(salt, 0, passwordHash, 13, _saltSize);
            Buffer.BlockCopy(subkey, 0, passwordHash, 13 + _saltSize, _subkeySize);

            return Convert.ToBase64String(passwordHash);
        }

        /// <summary>
        /// Determines whether <paramref name="providedPassword"/> matches the password used to 
        /// generate <paramref name="passwordHash"/>.
        /// </summary>
        /// <param name="passwordHash">The hash value for a password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>
        /// True if <paramref name="providedPassword"/> matches password used to 
        /// generate <paramref name="passwordHash"/>, false otherwise.
        /// </returns>
        /// <remarks>Implementations of this method should be time consistent.</remarks>
        public virtual bool ValidatePassword(string passwordHash, string providedPassword)
        {
            if (passwordHash == null)
            {
                throw new ArgumentNullException(nameof(passwordHash));
            }
            if (providedPassword == null)
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            byte[] decodedHashedPassword = Convert.FromBase64String(passwordHash);

            // read the format marker from the hashed password
            if (decodedHashedPassword.Length == 0)
            {
                return false;
            }

            try
            {
                // Read header information
                KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(decodedHashedPassword, 1);
                int iterCount = (int)ReadNetworkByteOrder(decodedHashedPassword, 5);
                int saltLength = (int)ReadNetworkByteOrder(decodedHashedPassword, 9);

                // Read the salt: must be >= 128 bits
                if (saltLength < 128 / 8)
                {
                    return false;
                }
                byte[] salt = new byte[saltLength];
                Buffer.BlockCopy(decodedHashedPassword, 13, salt, 0, salt.Length);

                // Read the subkey (the rest of the payload): must be >= 128 bits
                int subkeyLength = decodedHashedPassword.Length - 13 - salt.Length;
                if (subkeyLength < 128 / 8)
                {
                    return false;
                }
                byte[] expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(decodedHashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // Hash the incoming password and verify it
                byte[] actualSubkey = KeyDerivation.Pbkdf2(providedPassword, salt, prf, iterCount, subkeyLength);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
            catch
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                return false;
            }
        }

        #region
        // Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24)
                | ((uint)(buffer[offset + 1]) << 16)
                | ((uint)(buffer[offset + 2]) << 8)
                | ((uint)(buffer[offset + 3]));
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }
        #endregion
    }
}
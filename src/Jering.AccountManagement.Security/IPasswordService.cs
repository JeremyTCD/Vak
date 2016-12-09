// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an abstraction for managing passwords.
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Returns a hashed representation of the supplied <paramref name="password"/>
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed representation of the supplied <paramref name="password"/>.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Determines whether <paramref name="providedPassword"/> matches the password used to 
        /// generate <paramref name="hashedPassword"/>.
        /// </summary>
        /// <param name="hashedPassword">The hash value for a user's stored password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>True if <paramref name="providedPassword"/> is matches password used to 
        /// generate <paramref name="hashedPassword"/>, false otherwise.</returns>
        /// <remarks>Implementations of this method should be time consistent.</remarks>
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}
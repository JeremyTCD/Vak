// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Jering.Security
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
        /// generate <paramref name="passwordHash"/>.
        /// </summary>
        /// <param name="passwordHash">The hash value for a password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>
        /// True if <paramref name="providedPassword"/> matches password used to 
        /// generate <paramref name="passwordHash"/>, false otherwise.
        /// </returns>
        /// <remarks>Implementations of this method should be time consistent.</remarks>
        bool ValidatePassword(string passwordHash, string providedPassword);
    }
}
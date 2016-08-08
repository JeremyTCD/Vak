// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for <see cref="DataProtectionTokenService{TAccount}"/>
    /// </summary>
    public class DataProtectionTokenOptions
    {
        /// <summary>
        /// Lifespan of tokens.
        /// </summary>
        public TimeSpan TokenLifespan { get; set; } = TimeSpan.FromDays(1);
    }
}
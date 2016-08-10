// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for token services"/>
    /// </summary>
    public class TokenServiceOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Type> TokenServiceMap { get; set; } = new Dictionary<string, Type>();

        /// <summary>
        /// 
        /// </summary>
        public static readonly string TotpTokenService = "TotpTokenService";

        /// <summary>
        /// 
        /// </summary>
        public static readonly string DataProtectionTokenService = "DataProtectionTokenService";

        /// <summary>
        /// Lifespan of data protection tokens.
        /// </summary>
        public TimeSpan DataProtectionTokenLifespan { get; set; } = TimeSpan.FromDays(1);
    }
}
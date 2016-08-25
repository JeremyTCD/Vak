﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// 
    /// </summary>
    public enum PasswordSignInResult
    {
        /// <summary>
        /// 
        /// </summary>
        Succeeded,
        /// <summary>
        /// 
        /// </summary>
        TwoFactorRequired,
        /// <summary>
        /// 
        /// </summary>
        EmailConfirmationRequired,
        /// <summary>
        /// 
        /// </summary>
        Failed
    }
}
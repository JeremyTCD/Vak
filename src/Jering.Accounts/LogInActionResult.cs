using Jering.Accounts.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.LogInActionAsync(string, string, bool)"/>.
    /// </summary>
    public enum LogInActionResult
    {
        Success,
        InvalidEmail,
        InvalidPassword,
        TwoFactorRequired
    }
}

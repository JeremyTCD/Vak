using Jering.Accounts.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.TwoFactorLogInActionAsync(string, bool)"/>.
    /// </summary>
    public enum TwoFactorLogInActionResult
    {
        Success,
        InvalidCode,
        InvalidCredentials
    }
}

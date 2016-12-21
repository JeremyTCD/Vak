using Jering.Accounts.DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.TwoFactorVerifyEmailActionAsync(string)"/>.
    /// </summary>
    public enum TwoFactorVerifyEmailActionResult
    {
        Success,
        InvalidCode,
        AlreadySet,
        NoLoggedInAccount
    }
}

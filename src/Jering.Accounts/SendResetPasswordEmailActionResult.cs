using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Accounts
{
    /// <summary>
    /// Results for <see cref="IAccountService{TAccount}.SendResetPasswordEmailActionAsync(string)"/> 
    /// </summary>
    public enum SendResetPasswordEmailActionResult
    {
        InvalidEmail,
        Success
    }
}

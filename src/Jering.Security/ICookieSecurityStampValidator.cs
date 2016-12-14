using Jering.Accounts.DatabaseInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Jering.Security
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICookieSecurityStampValidator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ValidateAsync(CookieValidatePrincipalContext context);
    }
}

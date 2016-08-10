using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class AccountManagementServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TAccount"></typeparam>
        /// <param name="services"></param>
        public static AccountManagementBuilder AddAccountManagement<TAccount>(this IServiceCollection services, IConfigurationRoot configurationRoot) where TAccount : IAccount
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ClaimsPrincipalFactory<TAccount>, ClaimsPrincipalFactory<TAccount>>();
            services.AddScoped<AccountSecurityServices<TAccount>, AccountSecurityServices<TAccount>>();

            return new AccountManagementBuilder(typeof(TAccount), services);
        }
    }
}

using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.DatabaseInterface.Dapper;
using Jering.AccountManagement.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        public static AccountManagementBuilder AddAccountManagementSecurity<TAccount>(this IServiceCollection services, IConfigurationRoot configurationRoot) where TAccount : IAccount
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICookieSecurityStampValidator, CookieSecurityStampValidator<TAccount>>();
            services.AddScoped<ClaimsPrincipalServices<TAccount>>();
            services.AddScoped<IAccountSecurityServices<TAccount>, AccountSecurityServices<TAccount>>();
            services.AddScoped<IRoleRepository, DapperRoleRepository>();
            services.AddScoped<IClaimRepository, DapperClaimRepository>();

            return new AccountManagementBuilder(typeof(TAccount), services);
        }
    }
}

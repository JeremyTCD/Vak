using Jering.Accounts.DatabaseInterface;
using Jering.Security;
using Jering.Mail;
using Jering.Utilities;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jering.Accounts
{
    /// <summary>
    /// 
    /// </summary>
    public static class AccountServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TAccount"></typeparam>
        /// <param name="services"></param>
        public static AccountsBuilder AddAccounts<TAccount>(this IServiceCollection services) where TAccount : IAccount, new()
        {
            services.AddScoped<IAccountService<TAccount>, AccountService<TAccount>>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Security services
            services.AddScoped<ICookieSecurityStampValidator, CookieSecurityStampValidator<TAccount>>();
            services.AddScoped<IClaimsPrincipalService<TAccount>, ClaimsPrincipalService<TAccount>>();
            services.AddScoped<IPasswordService, PasswordService>();
            // Utility services
            services.AddScoped<ITimeService, TimeService>();

            return new AccountsBuilder(typeof(TAccount), services);
        }
    }
}

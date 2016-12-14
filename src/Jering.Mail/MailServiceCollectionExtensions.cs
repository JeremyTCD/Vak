using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;

namespace Jering.Mail
{
    public static class MailServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddDevelopmentEmailSender(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, DevelopmentEmailService>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddEmailSender(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<SmtpClient>();
        }
    }
}

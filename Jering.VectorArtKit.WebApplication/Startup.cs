using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jering.AccountManagement.Security;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.AccountManagement.Extensions;

namespace Jering.VectorArtKit.WebApplication
{
    public class Startup
    {
        private IConfigurationRoot _configurationRoot { get; }
        private IHostingEnvironment _hostingEnvironment { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _configurationRoot = builder.Build();

            _hostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // MVC
            services.AddMvc();

            // Data
            // TODO different connection string for release
            services.AddScoped(_ => new SqlConnection(_configurationRoot["Data:DefaultConnection:ConnectionString"]));

            // Account Management
            services.AddAccountManagementSecurity<VakAccount>(_configurationRoot).
                AddAccountRepository<VakAccountRepository>().
                AddDefaultTokenServices();

            services.AddEmailSender(_hostingEnvironment);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configurationRoot.GetSection("Logging"));
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // TODO Create actual views
            app.UseStatusCodePages("text/plain", "Response, status code: {0}");

            AccountSecurityOptions securityOptions = app.ApplicationServices.GetRequiredService<IOptions<AccountSecurityOptions>>().Value;
            // CookieAuthenticationMiddleware for TwoFactorCookie must be registered first. Otherwise if ApplicationCookie's middleware runs
            // first and SecurityStamp is invalid, the middleware will call AccountSecurityServices.SignOutAsync before a handler is created
            // for TwoFactorCookies.
            app.UseCookieAuthentication(securityOptions.CookieOptions.TwoFactorCookieOptions);
            app.UseCookieAuthentication(securityOptions.CookieOptions.ApplicationCookieOptions);


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

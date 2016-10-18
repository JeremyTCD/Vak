using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jering.AccountManagement.Security;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Jering.VectorArtKit.WebApi.BusinessModel;
using Jering.AccountManagement.Extensions;
using Jering.VectorArtKit.WebApi.Test;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Jering.DynamicForms;

namespace Jering.VectorArtKit.WebApi
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
            services.AddMvc();

            services.AddAccountManagementSecurity<VakAccount>(_configurationRoot).
                AddAccountRepository<VakAccountRepository>().
                AddDefaultTokenServices();

            services.AddDynamicForms();

            if (_hostingEnvironment.IsDevelopment())
            {
                // Provide means to debug views
                services.AddSingleton<ICompilationService, CustomCompilationService>();
                // Allow cross origin resource sharing for development
                services.AddCors();
                // TODO different connection string for release
                services.AddScoped(_ => new SqlConnection(_configurationRoot["Data:DefaultConnection:ConnectionString"]));
                services.AddDevelopmentEmailSender();
            }
            else
            {
                services.AddEmailSender();
            }


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
                // Allow cross origin resource sharing for development
                app.UseCors(builder => builder.AllowAnyOrigin());
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // TODO not needed for a web api, find someway to return a json error object 
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

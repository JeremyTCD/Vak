﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jering.AccountManagement.Security;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Extensions;

namespace Jering.VectorArtKit.WebApplication
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _configurationRoot = builder.Build();
        }

        private IConfigurationRoot _configurationRoot { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // MVC
            services.AddMvc();

            // Data
            // TODO different connection string for release
            services.AddScoped<SqlConnection>(_ => new SqlConnection(_configurationRoot["Data:DefaultConnection:ConnectionString"]));

            // Account Management
            services.AddAccountManagement<VakAccount>(_configurationRoot).
                AddAccountRepository<VakAccountRepository>().
                AddDefaultTokenServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configurationRoot.GetSection("Logging"));
            
            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            AccountSecurityOptions securityOptions = app.ApplicationServices.GetRequiredService<IOptions<AccountSecurityOptions>>().Value;
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

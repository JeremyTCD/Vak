using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jering.AccountManagement.Security;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.AccountManagement.Extensions;
using Jering.VectorArtKit.WebApi.Test;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Jering.DynamicForms;
using Microsoft.AspNetCore.Antiforgery;
using Jering.VectorArtKit.WebApi.Resources;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

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
            services.Configure<AntiforgeryOptions>(options =>
            {
                options.CookieName = "AF-TOKEN";
                options.HeaderName = "X-XSRF-TOKEN";
            });

            services.AddAccountManagementSecurity<VakAccount>(_configurationRoot).
                AddAccountRepository<VakAccountRepository>().
                AddDefaultTokenServices();

            services.AddDynamicForms();
            services.AddCors();

            if (_hostingEnvironment.IsDevelopment())
            {
                services.AddScoped(_ => new SqlConnection(_configurationRoot["Data:DefaultConnection:ConnectionString"]));
                services.AddDevelopmentEmailSender();
            }
            else
            {
                // TODO different connection string for release
                services.AddScoped(_ => new SqlConnection(_configurationRoot["Data:DefaultConnection:ConnectionString"]));
                services.AddEmailSender();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configurationRoot.GetSection("Logging"));

            // Sets body to a general error message for all responses with no bodies and status code >= 400 or < 600. 
            app.UseStatusCodePages(new StatusCodePagesOptions()
            {
                HandleAsync = (StatusCodeContext context) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    return context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { errorMessage = Strings.ErrorMessage_UnexpectedError }));
                }
            });

            if (env.IsDevelopment())
            {
                app.UseExceptionHandler(new ExceptionHandlerOptions()
                {
                    ExceptionHandler = async (HttpContext context) => {
                        context.Response.ContentType = "application/json";
                        Exception exception = context.Features.Get<IExceptionHandlerFeature>().Error;
                        if (exception != null)
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(exception));
                        }
                    }
                });
                  
                loggerFactory.AddDebug();
                app.UseBrowserLink();
                app.UseCors(builder => builder.
                    AllowAnyOrigin().
                    AllowAnyMethod().
                    AllowAnyHeader().
                    AllowCredentials());
            }
            else
            {
                // Catches exceptions. Sets status code of response to 500 if an exception is caught.
                app.UseExceptionHandler(new ExceptionHandlerOptions()
                {
                    // If not set, middleware will attempt to retry pipeline.
                    ExceptionHandler = (HttpContext context) => {
                        // Do nothing
                        return Task.FromResult(0);
                    }
                });

                // TODO limit requests to client domain
                // app.UseCors(builder => builder.AllowAnyOrigin());
            }

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
                    template: "{controller=Home}/{action=Index}");
            });
        }
    }
}

using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.VectorArtKit.WebApi.Options;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Jering.Accounts;
using Jering.Accounts.DatabaseInterface.Dapper;
using Jering.Mail;

namespace Jering.VectorArtKit.WebApi
{
    public class Startup
    {
        private IConfigurationRoot _configurationRoot { get; }
        private IHostingEnvironment _hostingEnvironment { get; }
        // Matches default resolver settings used by MVC
        private JsonSerializerSettings _jsonSerializationSettings { get; } =
            new JsonSerializerSettings() { ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() } };

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

            services.AddAccounts<VakAccount>(_configurationRoot).
                AddAccountRepository<VakAccountRepository>().
                AddRoleRepository<DapperRoleRepository>().
                AddClaimRepository<DapperClaimRepository>().
                AddDefaultTokenServices();

            services.AddDynamicForms();
            services.AddCors();

            if (_hostingEnvironment.IsDevelopment())
            {
                services.AddScoped(_ => new SqlConnection(_configurationRoot["Data:DefaultConnection:ConnectionString"]));
                services.AddDevelopmentEmailSender();
                services.Configure<UrlOptions>(options =>
                {
                    options.ClientDomain = "http://localhost:4200/";
                });
            }
            else
            {
                // TODO different connection string for release
                services.AddScoped(_ => new SqlConnection(_configurationRoot["Data:DefaultConnection:ConnectionString"]));
                services.AddEmailSender();
                services.Configure<UrlOptions>(options =>
                {
                    // TODO
                    options.ClientDomain = "http://localhost:4200/";
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // TODO read up on logging
            //loggerFactory.AddConsole(_configurationRoot.GetSection("Logging"));

            //Sets body to a general error message for all unexpected error responses with no bodies and status code >= 400 or < 600.
            app.UseStatusCodePages(new StatusCodePagesOptions()
            {
                HandleAsync = (StatusCodeContext context) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    ErrorResponseModel responseModel = new ErrorResponseModel()
                    {
                        ExpectedError = false,
                        ErrorMessage = Strings.ErrorMessage_UnexpectedError
                    };
                    return context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(responseModel, _jsonSerializationSettings));
                }
            });

            if (env.IsDevelopment())
            {
                app.UseExceptionHandler(new ExceptionHandlerOptions()
                {
                    ExceptionHandler = async (HttpContext context) =>
                    {
                        context.Response.ContentType = "application/json";
                        Exception exception = context.Features.Get<IExceptionHandlerFeature>().Error;
                        if (exception != null)
                        {
                            ErrorResponseModel responseModel = new ErrorResponseModel()
                            {
                                ExpectedError = false,
                                ErrorMessage = exception.Message
                            };
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(responseModel, _jsonSerializationSettings));
                        }
                    }
                });

                loggerFactory.AddDebug(LogLevel.Information);
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
                    ExceptionHandler = (HttpContext context) =>
                    {
                        // Do nothing
                        return Task.FromResult(0);
                    }
                });

                // TODO limit requests to client domain
                // app.UseCors(builder => builder.AllowAnyOrigin());
            }

            AccountsServiceOptions securityOptions = app.ApplicationServices.GetRequiredService<IOptions<AccountsServiceOptions>>().Value;
            // CookieAuthMiddleware for TwoFactorCookie must be registered first. Otherwise if ApplicationCookie's middleware runs
            // first and SecurityStamp is invalid, the middleware will call AccountSecurityService.SignOutAsync before a handler is created
            // for TwoFactorCookies.
            app.UseCookieAuthentication(securityOptions.CookieOptions.TwoFactorCookieOptions);
            app.UseCookieAuthentication(securityOptions.CookieOptions.ApplicationCookieOptions);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}");
            });
        }
    }
}

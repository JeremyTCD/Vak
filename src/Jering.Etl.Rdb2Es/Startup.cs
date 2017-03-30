using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.IO;

namespace Jering.VectorArtKit.Indexer
{
    public class Startup
    {
        public static IConfigurationRoot _configurationRoot { get; set; }

        public Startup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().
                SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configurationRoot = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.
                //AddLogging().
                AddSingleton(_ => new SqlConnection(_configurationRoot["Extractor:ConnectionString"])).
                AddSingleton<Extractor>();

            // This is AddLogging with the last options set to AddTransient
            services.
                AddSingleton(typeof(IOptions<>), typeof(OptionsManager<>)).
                AddSingleton(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>)).
                AddTransient(typeof(IOptionsSnapshot<>), typeof(OptionsSnapshot<>));

            services.Configure<ExtractorOptions>(_configurationRoot.GetSection("Extractor"));

            //services.Configure<EtlOptions>(options =>
            //{
            //    options.IntervalMs = Convert.ToInt32(_configurationRoot["Etl:IntervalMs"]);
            //});

            services.Configure<EtlOptions>(_configurationRoot.GetSection("Etl"));
        }

        public void Configure(ILoggerFactory loggerFactory)
        {
            loggerFactory.
                AddConsole((LogLevel)Enum.Parse(typeof(LogLevel), _configurationRoot["Logging:LogLevel:Console"])).
                AddDebug((LogLevel)Enum.Parse(typeof(LogLevel), _configurationRoot["Logging:LogLevel:Debug"]));
        }
    }
}

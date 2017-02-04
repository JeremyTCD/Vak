using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading;

namespace Jering.VectorArtKit.Indexer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();

            Startup startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            startup.Configure(loggerFactory);

            Loop(serviceProvider).Wait();

            //do the actual work here
            //var bar = serviceProvider.GetService<IBarService>();
            //bar.DoSomeRealWork();

            //logger.LogDebug("All done!");
        }

        public async static Task Loop(IServiceProvider serviceProvider)
        {
            ILogger logger = serviceProvider.GetService<ILogger<Program>>();
            Stopwatch stopwatch = new Stopwatch();

            while (true)
            {
                // appsettings.json may change
                EtlOptions options = serviceProvider.GetService<IOptionsSnapshot<EtlOptions>>().Value;
                Extractor extractor = serviceProvider.GetService<Extractor>();

                stopwatch.Start();

                IEnumerable<Row> result = await extractor.ExtractRows();

                stopwatch.Stop();
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();

                int difference = (int) (options.IntervalMs - elapsedMilliseconds);

                if(difference > 0)
                {
                    logger.LogInformation("[Etl complete]\n{statistics}", $"Duration: {elapsedMilliseconds}ms");
                    Thread.Sleep(difference);
                }
                else
                {
                    logger.LogWarning("Etl duration exceeded specified interval by {difference}", -difference);
                }
            }
        }
    }
}

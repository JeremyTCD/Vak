using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Jering.VectorArtKit.Indexer
{
    public class Extractor
    {
        private ExtractorOptions _extractorOptions { get; }
        private SqlConnection _sqlConnection { get; }
        private ILogger _logger { get; }

        public Extractor(IOptionsSnapshot<ExtractorOptions> extractorOptionsAccessor, SqlConnection sqlConnection, ILogger<Extractor> logger)
        {
            // TODO
            // Setup sqlConnection service

            _extractorOptions = extractorOptionsAccessor.Value;
            _sqlConnection = sqlConnection;
            _logger = logger;
        }

        public async Task<IEnumerable<Row>> ExtractRows()
        {
            // TODO 
            // Add synchronization

            string queryStatement = File.ReadAllText(_extractorOptions.QueryStatementFile);

            _logger.LogInformation("[Extracting rows]\n{queryStatement}", queryStatement);
            
            IEnumerable<Row> result = await _sqlConnection.QueryAsync<Row>(queryStatement, commandType: CommandType.Text);//, new {Synchronization_Version = }

            if (_logger.IsEnabled(LogLevel.Information)) {
                string rows = "";
                foreach (Row row in result)
                {
                    rows += $"\nId: {row.Id}, Version: {row.Version}, Source: {row.Source}";
                }
                _logger.LogInformation("[Result]{rows}", rows);
            }

            return result;
        }
    }
}

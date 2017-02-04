using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using Xunit;
using Newtonsoft.Json;
using Nest;
using Elasticsearch.Net;

namespace Jering.Etl.Rdb2Es.Tests
{
    [CollectionDefinition("DataPipelineCollection")]
    public class DataPipelineCollection : ICollectionFixture<DataPipelineFixture>
    {

    }

    public class DataPipelineFixture : IDisposable
    {
        public SqlConnection SqlConnection { get; }
        public IConfigurationRoot ConfigurationRoot { get; }
        public VakUnit[] VakUnits { get; }
        public ElasticClient ElasticClient { get; }

        // Move to config file
        private const string _elasticSearchUri = "http://localhost:9200";
        // Eventually, a configuration manager like puppet should be used
        private const string _logStashConfigFile = "C:\\Users\\Jeremy\\Desktop\\LibrariesAndTools\\logstash-5.1.2\\logstash-config.conf";

        public DataPipelineFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "../../../../project.json"));
            ConfigurationRoot = configurationBuilder.Build();
            SqlConnection = new SqlConnection(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);

            VakUnits = JsonConvert.DeserializeObject<VakUnit[]>(DataGenerator.GetData());

            Uri uri = new Uri(_elasticSearchUri);
            ConnectionSettings settings = new ConnectionSettings(uri);
            ElasticClient = new ElasticClient(settings);

            ResetDatabase();
            ResetElasticSearch();
        }

        public void Dispose()
        {
            SqlConnection.Close();
        }

        public void ResetDatabase()
        {
            // Pause change tracking
            SqlConnection.Execute(@"IF((SELECT is_track_columns_updated_on 
                FROM sys.change_tracking_tables
                WHERE object_id = OBJECT_ID('VakUnits')) = 1)
                BEGIN
                ALTER TABLE ""VakUnits""
                DISABLE CHANGE_TRACKING
                END", commandType: System.Data.CommandType.Text);

            // Delete rows, reset identity
            SqlConnection.Execute(@"DELETE FROM [dbo].[VakUnits;
                 DBCC CHECKIDENT('[dbo].[VakUnits]', RESEED, 0);", commandType: System.Data.CommandType.Text);
            SqlConnection.Execute(@"DELETE FROM [dbo].[Accounts];
                 DBCC CHECKIDENT('[dbo].[VakUnits]', RESEED, 0);", commandType: System.Data.CommandType.Text);

            // Create dummy account to own VakUnits
            SqlConnection.Execute(@"INSERT INTO [dbo].[Accounts](SecurityStamp, PasswordHash, PasswordLastChanged, Email)
                VALUES(NEWID(), 'DUMMY', GETDATE(), 'DUMMY');", commandType: System.Data.CommandType.Text);

            // Restart change tracking
            SqlConnection.Execute(@"IF((SELECT is_track_columns_updated_on 
                FROM sys.change_tracking_tables
                WHERE object_id = OBJECT_ID('Accounts')) = 0)
                BEGIN
                ALTER TABLE ""VakUnits""
                ENABLE CHANGE_TRACKING
                WITH (TRACK_COLUMNS_UPDATED = ON)
                END", commandType: System.Data.CommandType.Text);
        }

        public void ResetElasticSearch()
        {
            ElasticClient.LowLevel.DeleteByQuery<VoidResponse>("_all", @"{""query"": {""match_all"": { }}}");
        }
    }
}

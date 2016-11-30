using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Jering.VectorArtKit.EndToEnd.Tests
{
    [CollectionDefinition("E2ECollection")]
    public class E2ECollection : ICollectionFixture<E2EFixture>
    {

    }

    public class E2EFixture : IDisposable
    {
        public SqlConnection SqlConnection { get; }
        public IConfigurationRoot ConfigurationRoot { get; }
        public IWebDriver WebDriver { get; }

        public E2EFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "../../../../project.json"));
            ConfigurationRoot = configurationBuilder.Build();
            SqlConnection = new SqlConnection(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);
            WebDriver = new ChromeDriver();
        }

        public void Dispose()
        {
            WebDriver.Close();
            SqlConnection.Close();
        }

        public void ResetAccountsTable()
        {
            SqlConnection.Execute("Delete from [dbo].[Accounts];" +
                "DBCC CHECKIDENT('[dbo].[Accounts]', RESEED, 0);", commandType: System.Data.CommandType.Text);
        }
    }
}

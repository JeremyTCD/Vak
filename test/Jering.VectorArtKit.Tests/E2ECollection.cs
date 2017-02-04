using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using Xunit;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

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
        public ReusableWebDriver WebDriver { get; }

        private string Domain { get; } = "http://localhost:9515";

        public E2EFixture()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.
                AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "../../../../project.json"));
            ConfigurationRoot = configurationBuilder.Build();
            SqlConnection = new SqlConnection(ConfigurationRoot["Data:DefaultConnection:ConnectionString"]);
            WebDriver = GetWebDriver();
        }

        /// <summary>
        /// Starts chromedriver process if there isn't already one running. Instantiates a <see cref="ReusableWebDriver"/> instance
        /// that latches on to the existing chromedriver process and attempts to reuse last chrome session. If the last chrome session 
        /// has ended, instantiates a <see cref="ReusableWebDriver"/> instance that starts a new chrome session.
        /// </summary>
        public ReusableWebDriver GetWebDriver()
        {
            Process[] processes = Process.GetProcessesByName("chromedriver");
            if (processes.Length <= 0)
            {
                Process.Start("chromedriver.exe");
            }

            ReusableWebDriver.NewSession = false;
            ReusableWebDriver webDriver = new ReusableWebDriver(new Uri(Domain), DesiredCapabilities.Chrome());

            try
            {
                // Session may have been ended (chrome window may have been closed)
                string test = webDriver.Url;
                return webDriver;
            }
            catch { }

            ReusableWebDriver.NewSession = true;
            return new ReusableWebDriver(new Uri(Domain), DesiredCapabilities.Chrome());
        }

        public void Dispose()
        {
            SqlConnection.Close();
        }

        public void ResetAccountsTable()
        {
            SqlConnection.Execute("Delete from [dbo].[Accounts];" +
                "DBCC CHECKIDENT('[dbo].[Accounts]', RESEED, 0);", commandType: System.Data.CommandType.Text);
        }
    }
}

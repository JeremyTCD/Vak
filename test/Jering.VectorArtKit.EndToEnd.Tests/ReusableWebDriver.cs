using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;

public class ReusableWebDriver : RemoteWebDriver
{
    public static bool NewSession = false;

    private string _sessiodIdPath = $"{Environment.GetEnvironmentVariable("TMP")}\\SessionId.txt";

    public ReusableWebDriver(Uri remoteAddress, DesiredCapabilities dd)
        : base(remoteAddress, dd)
    {
    }

    protected override Response Execute(string driverCommandToExecute, Dictionary<string, object> parameters)
    {
        if (driverCommandToExecute == DriverCommand.NewSession)
        {
            if (!NewSession)
            {
                return new Response
                {
                    SessionId = File.ReadAllText(_sessiodIdPath)
                };
            }
            else
            {
                Response response = base.Execute(driverCommandToExecute, parameters);
                File.WriteAllText(_sessiodIdPath, response.SessionId);
                return response;
            }
        }
        else
        {
            return base.Execute(driverCommandToExecute, parameters);
        }
    }
}
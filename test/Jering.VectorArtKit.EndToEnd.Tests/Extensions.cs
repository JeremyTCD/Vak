using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.EndToEnd.Tests
{
    public static class Extensions
    {
        public static void Wait(this IWebDriver webDriver, Func<IWebDriver, bool> condition, int milliseconds)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromMilliseconds(milliseconds));
            wait.Until(condition);
        }
    }
}

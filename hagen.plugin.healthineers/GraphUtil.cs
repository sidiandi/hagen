using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace hagen
{
    public class GraphUtil
    {
        public static Task<string> GetTokenFromGraphExplorer() => Task.Factory.StartNew(() =>
        {
            var options = new ChromeOptions();
            // options.AddArgument("--window-position=-32000,-32000");
            options.AddArgument("--window-position=0,0");
            var userDataDir = Path.Combine(Path.GetTempPath(), "GraphUtil");
            options.AddArgument(@"user-data-dir=" + userDataDir);
            using (var driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl("https://developer.microsoft.com/en-us/graph/graph-explorer");
                var wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(60));
                var accessTokenButton = wait.Until(driver => driver.FindElement(By.XPath("//button[@name='Access token']")));
                accessTokenButton.Click();
                var label = wait.Until(driver => driver.FindElement(By.XPath("//label[contains(@class, 'ms-Label accessToken-')]")));
                return label.Text;
            }
        }, TaskCreationOptions.LongRunning);

        [TestFixture]
        public class Test
        {
            [Test]
            public async Task Token()
            {
                var token = await GraphUtil.GetTokenFromGraphExplorer();
                Console.WriteLine(token);
            }
        }
    }
}
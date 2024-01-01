using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.IO;

namespace Client
{
    internal class Cookies
    {
        public static IWebDriver initCookies()
        {
            ChromeOptions options = new ChromeOptions();
            string path = "user-data-dir=C:/Users/ADMIN/AppData/Local/Google/Chrome/User Data";
            options.AddArguments(path, "headless");
            IWebDriver driver = new ChromeDriver(options);
            return driver;
        }
        public static bool IsUrlValid(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        public static void getCookies(IWebDriver driver, string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
                string currentUrl = driver.Url;
                if (IsUrlValid(currentUrl))
                {
                    var cookies = driver.Manage().Cookies.AllCookies;
                    string cookiesString = "";
                    foreach (var cookie in cookies)
                    {
                        cookiesString += $"{cookie.Name}={cookie.Value};";
                    }
                    string logNameToWrite = "cookies.txt" ;
                    StreamWriter sw = new StreamWriter(logNameToWrite, false);

                    sw.WriteLine(cookiesString);

                    sw.Close();

                }
                else
                {
                    Console.WriteLine("Invalid URL.");
                    string logNameToWrite = "cookies.txt" ;
                    StreamWriter sw = new StreamWriter(logNameToWrite, true);

                    sw.WriteLine("Url invalid. Url must starts with https://www...");
                    sw.Close();
                }

            }
            catch (Exception e)
            {
                string logNameToWrite = "cookies.txt" ;
                StreamWriter sw = new StreamWriter(logNameToWrite, true);

                sw.WriteLine("error: " + e.Message);

                sw.Close();
            }
            finally
            {
            }
        }

    }
}

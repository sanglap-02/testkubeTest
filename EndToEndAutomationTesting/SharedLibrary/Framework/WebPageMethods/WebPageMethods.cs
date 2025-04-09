using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SharedLibrary.Framework.Reports;
using SharedLibrary.Framework.TestBase;

namespace SharedLibrary.Framework.WebPageMethods
{
    public static class WebPageMethods
    {
        /// <summary>
        /// Method to Open WebPage
        /// </summary>
        /// <param name="webUrl">URL to Pass</param>
        /// <exception cref="Exception"></exception>
        public static void openWebPage(string webUrl)
        {
            try
            {
                openWebPage(webUrl, false);
            }
            catch (Exception e)
            {
                throw new Exception("Error While opening Link ::" + webUrl + " :: " + e);
            }
        }


        /// <summary>
        /// Method to Open WebPage
        /// </summary>
        /// <param name="webUrl">URL to Pass</param>
        /// <param name="newTab">To be opened in Tab true/false</param>
        /// <exception cref="Exception"></exception>
        public static void openWebPage(string webUrl, bool newTab)
        {
            try
            {
                openWebPage(webUrl, false, false);
            }
            catch (Exception e)
            {
                throw new Exception("Error While opening Link ::" + webUrl + " :: " + e);
            }
        }


        /// <summary>
        /// Method to Open WebPage
        /// </summary>
        /// <param name="webUrl"> URL to Pass</param>
        /// <param name="newTab">To be opened in Tab true/false</param>
        /// <param name="wait">should the Method wait for Page Load</param>
        /// <exception cref="Exception"></exception>
        public static void openWebPage(string webUrl, bool newTab, bool wait)
        {
            try
            {
                if (newTab)
                {
                    BaseTest.driver.SwitchTo().NewWindow(WindowType.Tab);
                }
                ExtentManager.LogInfo("Opening Page :: " + webUrl);
                BaseTest.driver.Navigate().GoToUrl(webUrl);
                if (wait)
                {
                    Thread.Sleep(10 * 1000);
                }
                Thread.Sleep(2000);
                waitForPageLoad();
                if (wait)
                {
                    Thread.Sleep(10 * 1000);
                }
                Thread.Sleep(3000);
                waitForPageLoad();
            }
            catch (Exception e)
            {
                throw new Exception("Error While opening Link ::" + webUrl + " :: " + e);
            }
        }

        /// <summary>
        /// Method to wait for page Load
        /// </summary>
        public static void waitForPageLoad()
        {
            WebDriverWait wait = new WebDriverWait(BaseTest.driver, TimeSpan.FromSeconds(50));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
    }
}

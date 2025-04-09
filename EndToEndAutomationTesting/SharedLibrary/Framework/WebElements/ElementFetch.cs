using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SharedLibrary.Framework.TestBase;

namespace SharedLibrary.Framework.WebElements
{
    public static class ElementFetch
    {
        /// <summary>
        /// Method to return WebElement Based on Identifier Type and Value with Wait
        /// </summary>
        /// <param name="identifierType"></param>
        /// <param name="indentifierValue"></param>
        /// <param name="waitSeconds"></param>
        /// <returns></returns>
        public static IWebElement GetWebElement(string identifierType, string indentifierValue, int waitSeconds)
        {
            switch (identifierType)
            {
                case "NAME":
                    return BaseTest.driver.FindElement(By.Name(indentifierValue));

                case "ID":
                    return BaseTest.driver.FindElement(By.Id(indentifierValue));

                case "CSS":
                    return BaseTest.driver.FindElement(By.CssSelector(indentifierValue));

                case "TAGNAME":
                    return BaseTest.driver.FindElement(By.TagName(indentifierValue));

                case "XPATH":
                    {
                        if (waitSeconds > 0)
                        {
                            DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(BaseTest.driver);
                            fluentWait.Timeout = TimeSpan.FromSeconds(waitSeconds);
                            fluentWait.PollingInterval = TimeSpan.FromMilliseconds(250);

                            fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                            fluentWait.Message = "Element to be searched not found";

                            IWebElement _element = fluentWait.Until(x => x.FindElement(By.XPath(indentifierValue)));

                            return _element;
                        }
                        else
                            return BaseTest.driver.FindElement(By.XPath(indentifierValue));
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Method to return WebElement Based on Identifier Type and Value without wait
        /// </summary>
        /// <param name="identifierType"></param>
        /// <param name="indentifierValue"></param>
        /// <returns></returns>
        public static IWebElement GetWebElement(string identifierType, string indentifierValue)
        {
            return GetWebElement(identifierType, indentifierValue, 0);
        }

        /// <summary>
        /// Method to return list of webelements
        /// </summary>
        /// <param name="identifierType"></param>
        /// <param name="indentifierValue"></param>
        /// <returns></returns>
        public static IList<IWebElement> GetListWebElements(string identifierType, string indentifierValue)
        {
            BaseTest.driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            switch (identifierType)
            {
                case "ID":
                    return BaseTest.driver.FindElements(By.Id(indentifierValue));

                case "CSS":
                    return BaseTest.driver.FindElements(By.CssSelector(indentifierValue));

                case "TAGNAME":
                    return BaseTest.driver.FindElements(By.TagName(indentifierValue));

                case "XPATH":
                    return BaseTest.driver.FindElements(By.XPath(indentifierValue));

                default:
                    return null;
            }
        }


    }
}

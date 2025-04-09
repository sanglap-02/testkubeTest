using OpenQA.Selenium;

namespace SharedLibrary.Framework.Reports
{
    public class ScreenShot
    {
        private IWebDriver driver;

        public ScreenShot(IWebDriver driver)
        {
            this.driver = driver;
        }

        public string GetScreeenshot()
        {
            if (driver != null)
            {
                var file = ((ITakesScreenshot)driver).GetScreenshot();
                var img = file.AsBase64EncodedString;

                return img;
            }
            else
            {
                return null;
            }
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using Serilog;
using Serilog.Core;
using SharedLibrary.Framework.Reports;

namespace SharedLibrary.Framework.TestBase
{
    public class BaseTest : ExtentManager
    {
        public static IWebDriver driver { get; private set; }
        protected ScreenShot screenShot { get; private set; }
        public bool EnablePerfLogging = false;

        public static IServiceProvider ServiceProvider;
        public BaseTest() : this(false)
        {
        }

        public BaseTest(bool enablePerfLogging)
        {
            EnablePerfLogging = enablePerfLogging;
        }
        
        [SetUp]
        public void Init()
        {
            LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Debug);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.File(main.auto.Constants.Constants.LogPath + @"\Logs", 
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Message} {NewLine}",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Starting Test Case {0}.", TestContext.CurrentContext.Test.MethodName);
            SetupDriver();
            screenShot = new ScreenShot(driver);
        }
        
        
        [TearDown]
        public void Cleanup()
        {
            Log.Information("Cleanup Test Case {0}.", TestContext.CurrentContext.Test.MethodName);

            EndTest();
            EndReporting();
            driver.Quit();
            driver.Dispose();

        }

        /// <summary>
        /// Method to Setup Chrome Options
        /// </summary>
        public void SetupDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            var service = ChromeDriverService.CreateDefaultService("/usr/local/bin");
            service.SuppressInitialDiagnosticInformation = true;
            service.EnableVerboseLogging = false;

            driver = new ChromeDriver(service, options);
        }





        private void EndTest()
        {
            Log.Information("End Test Case {0}.", TestContext.CurrentContext.Test.MethodName);

            var testStatus = TestContext.CurrentContext.Result.Outcome.Status;
            var message = TestContext.CurrentContext.Result.Message;

            switch (testStatus)
            {
                case TestStatus.Failed:
                    LogFail("Test has failed {message}");
                    break;

                case TestStatus.Skipped:
                    LogInfo("Test has skipped {message}");
                    break;

                default:
                    break;
            }
            LogScreenshot("Ending Test", screenShot.GetScreeenshot());
        }
    }
}

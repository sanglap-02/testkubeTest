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

            if (driver != null)
            {
                try { driver.Quit(); } catch (Exception ex) { Log.Warning("Quit failed: " + ex.Message); }
                try { driver.Dispose(); } catch (Exception ex) { Log.Warning("Dispose failed: " + ex.Message); }
            }
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

            try
            {
                var service = ChromeDriverService.CreateDefaultService("/usr/local/bin");
                service.SuppressInitialDiagnosticInformation = true;
                service.EnableVerboseLogging = false;

                driver = new ChromeDriver(service, options);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to initialize ChromeDriver: {0}", ex.Message);
                throw; // Optional: rethrow if you want test to fail early
            }
        }





        private void EndTest()
        {
            Log.Information("End Test Case {0}.", TestContext.CurrentContext.Test.MethodName);

            var testStatus = TestContext.CurrentContext.Result.Outcome.Status;
            var message = TestContext.CurrentContext.Result.Message;

            if (screenShot != null)
            {
                LogScreenshot("Ending Test", screenShot.GetScreeenshot());
            }
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

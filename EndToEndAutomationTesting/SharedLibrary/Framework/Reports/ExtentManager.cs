using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace SharedLibrary.Framework.Reports
{
    public class ExtentManager
    {
        private static ExtentReports extentReports;
        public static ExtentTest extentTest;
        // static string path = Directory.GetParent(@"../../../").FullName + Path.DirectorySeparatorChar + "Result";


        private static ExtentReports StartReporting()
        {

            if (extentReports == null)
            {
                Directory.CreateDirectory(main.auto.Constants.Constants.ReportPath);
                extentReports = new ExtentReports();
                var htmlReporter = new ExtentSparkReporter(main.auto.Constants.Constants.ReportPath + Path.DirectorySeparatorChar + "AutomationReport.html");

                extentReports.AttachReporter(htmlReporter);

            }
            return extentReports;
        }


        public static void CreateTest(string testName)
        {
            extentTest = StartReporting().CreateTest(testName);
        }

        public static void EndReporting()
        {
            if (extentTest == null)
                CreateTest(TestContext.CurrentContext.Test.MethodName);
            StartReporting().Flush();
        }

        public static void LogInfo(string info)
        {
            if (extentTest == null)
                CreateTest(TestContext.CurrentContext.Test.MethodName);
            extentTest.Info(info);
        }

        public static void LogPass(string pass)
        {
            if (extentTest == null)
                CreateTest(TestContext.CurrentContext.Test.MethodName);
            extentTest.Pass(pass);
        }

        public static void LogFail(string fail)
        {
            if (extentTest == null)
                CreateTest(TestContext.CurrentContext.Test.MethodName);
            extentTest.Fail(fail);
        }

        public static void LogWarning(string warning)
        {
            if (extentTest == null)
                CreateTest(TestContext.CurrentContext.Test.MethodName);
            extentTest.Warning(warning);
        }

        public static void LogScreenshot(string info, string image)
        {
            if (extentTest == null)
                CreateTest(TestContext.CurrentContext.Test.MethodName);
            extentTest.Info(info, MediaEntityBuilder.CreateScreenCaptureFromBase64String(image).Build());
        }
    }
}

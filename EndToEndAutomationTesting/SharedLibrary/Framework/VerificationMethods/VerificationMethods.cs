using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Reports;

namespace SharedLibrary.Framework.VerificationMethods
{
    public class VerificationMethods(ILogging logging)
    {
        // private readonly ILogging _logging;
        // public VerificationMethods(ILogging logging) => _logging = logging;
        /// <summary>
        /// Method to Verify if Boolean value matches
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="entityName"></param>
        public void VerifyBooleanEquals(bool expected, bool actual, string entityName)
        {
            VerifyBooleanEquals(expected, actual, entityName, false);
        }

        /// <summary>
        /// Method to Verify if Boolean value matches and Fails Tests
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="entityName"></param>
        /// <param name="fail"></param>
        private static void VerifyBooleanEquals(bool expected, bool actual, string entityName, bool fail)
        {
            if (expected == actual)
            {
                ExtentManager.LogPass("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
            }
            else
            {
                ExtentManager.LogFail("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
            }

            if (fail)
                Assert.Fail("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
        }

        public bool VerifyStringEquals(string expected, string actual, string entityName)
        {
            logging.LogInformation("VerificationMethods - VerifyStringEquals");
            
            if (expected == actual)
            {
                logging.LogInformation("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
                ExtentManager.LogPass("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
                return true;
            }
            else
            {
                logging.LogError("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
                ExtentManager.LogFail("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
                Assert.Fail("[Entity-" + entityName + "] [Expected-" + expected + "] [Actual-" + actual + "]");
                return false;
            }
        }
        
        public  void VerifyIntegerEquals(int expected, int actual, string entityName)
        {
            if (expected == actual)
            {
                logging.LogInformation($"[Entity-{entityName}] [Expected-{expected}] [Actual-{actual}]");
                ExtentManager.LogPass($"[Entity-{entityName}] [Expected-{expected}] [Actual-{actual}]");
            }
            else
            {
                ExtentManager.LogFail($"[Entity-{entityName}] [Expected-{expected}] [Actual-{actual}]");
                Assert.Fail($"[Entity-{entityName}] [Expected-{expected}] [Actual-{actual}]");
            }
        }

    }
}

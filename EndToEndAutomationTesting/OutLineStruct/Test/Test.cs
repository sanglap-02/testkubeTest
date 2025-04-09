using NUnit.Framework;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.Framework.Reports;
using SharedLibrary.Framework.TestBase;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.StepsMethods;

namespace SLM.Test;

public class Test : BaseTest
{   
    private const string Database = DatabaseConstants.QatBrokerConnectorServer;
    private const string BrokerTable = TableConstants.ServerConnectorBrokerHub;
    private const string LicSerTable = TableConstants.ServerConnectorLicSer;
    
    [Test]
    public static async Task TestRepo()
    {
        CreateTest("TestRepo");
        extentTest.AssignAuthor("XXXXXXXXXXXXXXXX");
        extentTest.AssignCategory("XXXXXXXXXXXXXXXX Test");
        extentTest.AssignDevice("chrome");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());

        const string testId = "AuthenticationTest";
        Console.WriteLine(":: XXXXXXXXXXXXXXXX :: STARTED");
        ExtentManager.LogInfo(":: XXXXXXXXXXXXXXXX :: STARTED");
        
        try
        {
            var brokerDetails = new Dictionary<string, string>();
            
            var testEnvironment = sqlTableUtilsQat.GetCellData(Database, BrokerTable,
                testId, "TEST_ID", "ENVIRONMENT");
            var envDetails = sqlTableUtilsQat.GetMapFromTwoTables(TableConstants.Environment, TableConstants.Services,
                testEnvironment, Database);
            
            brokerDetails = await ApproveBroker.ApproveBrokerStep(Database, BrokerTable, testId, brokerDetails, envDetails);
            
            brokerDetails = await AddLicenseServer.AddLicenseServerStep(Database, testId, brokerDetails, envDetails,
                LicSerTable);

            brokerDetails.Add("Database", Database);
            await ApproveLicenseServer.ApproveLicenseServerStep(brokerDetails, envDetails);
            
            await DeleteLicenseServer.DeleteLicenseServerStep(brokerDetails, envDetails);
            await DeleteBroker.DeleteBrokerStep(brokerDetails, envDetails);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Test Case Failed.");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
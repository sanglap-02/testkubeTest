using NPOI.Util;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.Broker.Hub;
using SharedLibrary.main.auto.Constants;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.token;

namespace SharedLibrary.main.StepsMethods;

public static class ApproveBroker
{
    private const string ProductBrokerApi = "brokerhubapi";
    
    public static async Task<Dictionary<string, string>> ApproveBrokerStep(string database, string tableName, string testId, Dictionary<string, string> brokerDetails, Dictionary<string, string> envDetails)
    {
        var accessTokenMethods = new AccessTokenMethods(new Logging());
        var urlMethods = new UrlMethods(new Logging());
        var openLmBrokerHubMethods = new OpenLmBrokerHubMethods(new Logging());
        
        try
        {
            envDetails.Add("TestId", testId);
            envDetails.Add("Database", database);

            var brokerHubToken = await accessTokenMethods.GetBrokerHubTokenFromApi(envDetails);
            if (brokerHubToken != null &&
                string.Equals(envDetails["AuthEnabled"], "yes", StringComparison.OrdinalIgnoreCase))
                Assert.Fail("Error While Generating Broker Hub Token");

            envDetails.Add("brokerHubToken", brokerHubToken ?? "");
            
            var apiBaseUrl = urlMethods.GetUrl(envDetails, ProductBrokerApi);
            envDetails.Add("APIBaseUrl", apiBaseUrl);
            
            brokerDetails = openLmBrokerHubMethods.SetBrokerDetails(database, tableName, testId);
            brokerDetails = openLmBrokerHubMethods.AddBrokerSql(brokerDetails, testId, database, tableName, testId);
            
            await openLmBrokerHubMethods.AddBroker(brokerDetails, envDetails);
            await openLmBrokerHubMethods.ApproveBroker(brokerDetails, envDetails);
            
            await Task.CompletedTask;
            return brokerDetails;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Approve Broker step Failed.");
            Console.WriteLine(ex.StackTrace);
            throw new RuntimeException();
        }
    }
}
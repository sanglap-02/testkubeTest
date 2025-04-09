using NPOI.Util;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.Broker.Hub;
using SharedLibrary.main.apiMethods.LicenseServer;
using SharedLibrary.main.auto.framework.constants;

namespace SharedLibrary.main.StepsMethods;

public static class AddLicenseServer
{
    public static async Task<Dictionary<string, string>> AddLicenseServerStep(string database, string testId, Dictionary<string, string> brokerDetails, Dictionary<string, string> envDetails, string licenseTable)
    {
        var licenseServerMethods = new LicenseServerMethods(new Logging());
        var openLmBrokerHubMethods = new OpenLmBrokerHubMethods(new Logging());
        
        try
        {
            var brokerHubConfig = licenseServerMethods.SetLicenseServerDetails(database,
                TableConstants.ServerConnectorLicSer, TableConstants.ServerConnectorBrokerHub, testId);
            brokerHubConfig = licenseServerMethods.AddLicenseServerSql(brokerHubConfig,
                TableConstants.ServerConnectorLicSer, testId, database, testId);
            brokerDetails = brokerDetails.Concat(brokerHubConfig)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
            
            await openLmBrokerHubMethods.BrokerConfig(brokerDetails, envDetails);

            brokerDetails["ServerTable"] = licenseTable;

            await Task.CompletedTask;
            return brokerDetails;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Add License Server step Failed.");
            Console.WriteLine(ex.StackTrace);
            throw new RuntimeException();
        }
    }
}
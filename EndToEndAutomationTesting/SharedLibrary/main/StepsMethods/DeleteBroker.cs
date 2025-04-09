using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.Broker.Hub;

namespace SharedLibrary.main.StepsMethods;

public static class DeleteBroker
{
    public static async Task DeleteBrokerStep(Dictionary<string, string> brokerData, Dictionary<string, string> envDetails)
    {
        var deleteBroker = brokerData["DeleteBrokerData"];
        var openLmBrokerHubMethods = new OpenLmBrokerHubMethods(new Logging());
 
        try
        {
            if (string.Equals(deleteBroker, "yes", StringComparison.OrdinalIgnoreCase))
            {
                await openLmBrokerHubMethods.DeleteBroker(brokerData, envDetails);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Delete Broker step Failed.");
            Console.WriteLine(ex.StackTrace);
            Assert.Fail();
        }
    }
}
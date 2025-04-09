using Microsoft.IdentityModel.Tokens;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.LicenseServer;
using SharedLibrary.main.auto.Constants;
using SharedLibrary.main.auto.framework.token;

namespace SharedLibrary.main.StepsMethods;

public static class ApproveLicenseServer
{
    private const string ProductOperationalApi = "operationalApi";
    
    public static async Task ApproveLicenseServerStep(Dictionary<string, string> licenseDetails, Dictionary<string, string> envDetails)
    {
        var accessTokenMethods = new AccessTokenMethods(new Logging());
        var urlMethods = new UrlMethods(new Logging());
        var licenseServerMethods = new LicenseServerMethods(new Logging());
        
        try
        {
            var serverToken = await accessTokenMethods.GetAuthTokenFromOperationalApi(envDetails);
            var serverUrl = urlMethods.GetUrl(envDetails, ProductOperationalApi);
           
            envDetails.Add("ServerAuthToken", serverToken);
            envDetails.Add("ServerUrl", serverUrl);

            var licenseServerId = await licenseServerMethods.ApproveLicenseServer(licenseDetails, envDetails);

            if (licenseServerId.IsNullOrEmpty())
            {
                Thread.Sleep(2 * 1000);
                licenseServerId = await licenseServerMethods.ApproveLicenseServer(licenseDetails, envDetails);
            }
            
            licenseDetails.Add("LicenseServerId", licenseServerId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Approve License Server step Failed.");
            Console.WriteLine(ex.StackTrace);
            Assert.Fail();
            throw new RuntimeException();
        }
    }
}
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.LicenseServer;

namespace SharedLibrary.main.StepsMethods;

public static class DeleteLicenseServer
{
    public static async Task DeleteLicenseServerStep(Dictionary<string, string> licenseDetails, Dictionary<string, string> envDetails)
    {
        var licenseServerMethods = new LicenseServerMethods(new Logging());
        var deleteLicense = licenseDetails["DeleteData"];
        
        try
        {
            if (string.Equals(deleteLicense, "yes", StringComparison.OrdinalIgnoreCase))
            {
                await licenseServerMethods.DeleteLicenseServer(licenseDetails, envDetails);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Delete License Server step Failed.");
            Console.WriteLine(ex.StackTrace);
            Assert.Fail();
        }
    }
}
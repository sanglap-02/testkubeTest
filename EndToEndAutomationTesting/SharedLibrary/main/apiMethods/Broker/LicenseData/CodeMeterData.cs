using MongoDB.Driver.Linq;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.apiMethods.Broker.LicenseData;

public class CodeMeterData
{
    private readonly ILogging _logging;
    public CodeMeterData(ILogging logging) => _logging = logging;

    public string GenerateCodeMeterLogData(Dictionary<string, string> licenseData)
    {
        _logging.LogInformation("CodeMeterData - GenerateCodeMeterLogData");

        try
        {
            var userName = licenseData["username"];
            var userHost = licenseData["userhost"];
            var userText = licenseData["userText"];
            var feature = licenseData["Feature1"];

            var timeZone = licenseData["Host_TimeZone"];
            var denialTime = DateTime.UtcNow.ToString("G", timeZone);

            var data =
                $"{denialTime}: Entry (123:{feature}:1 RD=2021-10-30) not found - " +
                $"Event WB0212 (NO MORE LICENSES), Request IP-Address {userHost.ToLower()}(VCN\\{userName.ToLower()}," +
                $"{userText}) (SID 0xc03d / 0x00) with StationShare Mode";

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating CodeMeter Log Data. \n" + ex.Message);
        }
    }
}
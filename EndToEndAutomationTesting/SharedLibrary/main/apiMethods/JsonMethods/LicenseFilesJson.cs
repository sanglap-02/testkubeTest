using Newtonsoft.Json;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.apiMethods.JsonMethods;

public class LicenseFilesJson
{
    private readonly ILogging _logging;
    public LicenseFilesJson(ILogging logging) => _logging = logging;

    public string LicenseFileJson(Dictionary<string, string> licenseFileDetails)
    {
        _logging.LogInformation("LicenseFilesJson - LicenseFileJson");

        try
        {
            var body = new Dictionary<string, object>();

            body["Port"] = int.Parse(licenseFileDetails["Port"]);
            body["ReadInterval"] = int.Parse(licenseFileDetails["ReadInterval"]);
            body["ServerHost"] = licenseFileDetails["ServerHost"];
            body["IsLogsOnly"] = bool.Parse(licenseFileDetails["IsLogsOnly"]);

            var licenseFileList = new List<Dictionary<string, object>>();
            var innerBody = new Dictionary<string, object>();

            innerBody["Crc"] = int.Parse(licenseFileDetails["licenseCrc"]);
            innerBody["LicenseFilePath"] = licenseFileDetails["LicenseFilePath"];
            innerBody["Data"] = licenseFileDetails["licenseFileData"];
            innerBody["CrcStatus"] = licenseFileDetails["CrcStatus"];

            licenseFileList.Add(innerBody);

            body["LicenseFileList"] = licenseFileList;

            body["ServerLocalTime"] = licenseFileDetails["ServerLocalTime"];
            body["MsgId"] = int.Parse(licenseFileDetails["MsgId"]);
            body["offline"] = bool.Parse(licenseFileDetails["Offline"]);
            body["BrokerId"] = licenseFileDetails["BrokerId"];

            return JsonConvert.SerializeObject(body);
        } catch(Exception ex)
        {
            _logging.LogError("Error while creating License Files Json.", ex);
            Assert.Fail("Error while creating License Files JSON. \n");
            throw new RuntimeException();
        }
    }
}
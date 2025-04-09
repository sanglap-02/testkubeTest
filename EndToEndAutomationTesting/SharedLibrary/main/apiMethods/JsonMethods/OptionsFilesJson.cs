using Newtonsoft.Json;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.apiMethods.JSONmethods
{
    public class OptionsFilesJson
    {
        private readonly ILogging _logging;
        public OptionsFilesJson(ILogging logging) => _logging = logging;

        public string OptionFilesJson(Dictionary<string, string> optionFileDetails)
        {
            _logging.LogInformation("OptionsFilesJSON - optionFilesJSON");
            try
            {
                var body = new Dictionary<string, object>
                {
                    { "AfterRestart", bool.Parse(optionFileDetails["AfterRestart"]) },
                    { "Crc", int.Parse(optionFileDetails["Crc"]) },
                    { "CrcStatus", optionFileDetails["CrcStatus"] },
                    { "OptionFileDescription", optionFileDetails["OptionFileDescription"] },
                    { "OptionFilePath", optionFileDetails["OptionFilePath"] },
                    { "Port", int.Parse(optionFileDetails["Port"]) },
                    { "ReadInterval", int.Parse(optionFileDetails["ReadInterval"]) },
                    { "ServerHost", optionFileDetails["ServerHost"] },
                    { "Vendor", optionFileDetails["Vendor"] },
                    { "Data", optionFileDetails["Data"] },
                    { "LMType", optionFileDetails["LMType"] },
                    { "MsgId", int.Parse(optionFileDetails["MsgId"]) },
                    { "offline", "false" },
                    { "BrokerId", optionFileDetails["BrokerId"] }
                };

                var jsonBody = JsonConvert.SerializeObject(body, Formatting.Indented);
                return jsonBody;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Creating Options Files JSON. \n");
            }
        }
    }
}
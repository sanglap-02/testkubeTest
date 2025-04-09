using Newtonsoft.Json;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.sqlDatabase;

namespace SharedLibrary.main.apiMethods.JsonMethods;

public class BrokerJson
{
    private readonly ILogging _logging;
    public BrokerJson(ILogging logging) => _logging = logging;

    public string BrokerCompositeBuildJson(Dictionary<string, string> brokerDetails,
        Dictionary<string, string> compositeDetails)
    {
        _logging.LogInformation("BrokerJson - BrokerCompositeBuildJson");

        try
        {
            var final = BrokerCompositeBuildJsonIntermediate(brokerDetails, compositeDetails, "");
            var json = JsonConvert.SerializeObject(final);

            _logging.LogInformation("Json :: \n" + json);
            return json;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error While Creating JSON. \n" + ex.Message);
        }
    }

    public string BrokerCompositeBuildJsonOne(Dictionary<string, string> brokerDetails,
        Dictionary<string, string> compositeDetails)
    {
        _logging.LogInformation("BrokerJson - BrokerCompositeBuildJson");

        try
        {
            var final = BrokerCompositeBuildJsonIntermediate(brokerDetails, compositeDetails, "1");
            var json = JsonConvert.SerializeObject(final);

            _logging.LogInformation("Json :: \n" + json);
            return json;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error While Creating JSON. \n");
        }
    }

    public string BrokerCompositeBuildJsonTwo(Dictionary<string, string> brokerDetails,
        Dictionary<string, string> compositeDetails)
    {
        _logging.LogInformation("BrokerJson - BrokerCompositeBuildJson");

        try
        {
            var final = BrokerCompositeBuildJsonIntermediate(brokerDetails, compositeDetails, "2");
            var json = JsonConvert.SerializeObject(final);

            _logging.LogInformation("Json :: \n" + json);
            return json;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error While Creating JSON. \n");
        }
    }

    private Dictionary<object, object> BrokerCompositeBuildJsonIntermediate(Dictionary<string, string> brokerDetails,
        Dictionary<string, string> compositeDetails, string condition)
    {
        _logging.LogInformation("BrokerJson - BuildJsonIntermediate");

        try
        {
            string[] hostSynonyms = new string[] { brokerDetails["HostName"].ToLower() };

            var server = new Dictionary<object, object>
            {
                { "HostSynonyms", hostSynonyms },
                { "OutputFormat", brokerDetails["TEST_ID"] },
                { "LmName", brokerDetails["Type"] }
            };

            if (!string.IsNullOrWhiteSpace(brokerDetails["Port"]))
                server.Add("Port", int.Parse(brokerDetails["Port"]));

            server.Add("ServerHost", brokerDetails["HostName"].ToLower());
            server.Add("Timezone", brokerDetails["TimeZone"]);
            server.Add("ReadLicFile", bool.Parse(compositeDetails["ReadLicFile"]));
            server.Add("Culture", "en-US");

            // LicenseOutput Dictionary
            var licenseOutput = new Dictionary<string, object>
            {
                { "CommandKey", "data_inquiry" }
            };

            var commandLine = compositeDetails["CommandLine"].Replace("$<HOST>$", brokerDetails["HostName"]);
            commandLine = commandLine.Replace("$<PORT>$", brokerDetails["Port"]);
            licenseOutput.Add("CommandLine", commandLine);

            var time = compositeDetails["ExecutionLocalDate"];
            if (string.IsNullOrWhiteSpace(time))
            {
                time = DateTime.UtcNow.ToString(brokerDetails["TimeZoneFormat"]);
            }

            licenseOutput.Add("ExecutionLocalDate", time);
            licenseOutput.Add("OutputFormat", "Raw");
            licenseOutput.Add("ReadInterval", int.Parse(compositeDetails["ReadInterval"]));

            string brokerData = null;
            switch (condition)
            {
                case "1":
                    brokerData = compositeDetails["data_inquiry"];
                    brokerData = brokerData.Replace("cmu32 - CodeMeter Universal Support Tool",
                        "\ncmu32 - CodeMeter Universal Support Tool");
                    licenseOutput.Add("Data", brokerData);
                    break;
                case "2":
                    brokerData = compositeDetails["data_inquiry"];
                    brokerData = brokerData.Replace("Certified Time:", "\r\n  Certified Time:");
                    brokerData = brokerData.Replace("  \r\n", "\r\n");
                    brokerData = brokerData.Replace("cmu32 - CodeMeter Universal Support Tool",
                        "\ncmu32 - CodeMeter Universal Support Tool");
                    licenseOutput.Add("Data", brokerData);
                    break;
                default:
                    licenseOutput.Add("Data", compositeDetails["data_inquiry"]);
                    break;
            }

            var logsParts = new List<object>();
            var logData = new Dictionary<string, object>();

            if (compositeDetails["LogData"].Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var logsPartsData = new Dictionary<string, object>
                {
                    { "LogType", compositeDetails["LogType"] },
                    { "Vendor", brokerDetails["Vendor"] }
                };

                var dataParts = new List<object>();
                var dataPartsData = new Dictionary<string, object>
                {
                    { "ServerDateLocal", time },
                    { "Data", compositeDetails["Data"] }
                };

                dataParts.Add(dataPartsData);
                logsPartsData.Add("DataParts", dataParts);
                logsParts.Add(logsPartsData);
            }

            logData.Add("LogsParts", logsParts);
            logData.Add("ZeroSessionIndication", 0);

            var final = new Dictionary<object, object>
            {
                { "ServerInfo", server },
                { "MachineName", null },
                { "LicenseOutput", licenseOutput },
                { "LogData", logData },
                {
                    "LocalDateTimePattern", 
                    brokerDetails.ContainsKey("Type") 
                        ? (brokerDetails["Type"] == "DSLS" 
                            ? "dd.MM.yyyy, HH:mm:ss" 
                            : null) 
                        : ""
                },
                { "PortStatusData", "" },
                { "BrokerId", brokerDetails["BrokerId"] },
                { "ServerLocalTime", time },
                { "ServerUtcTime", time },
                { "MsgId", int.Parse(brokerDetails["MsgId"]) },
                { "Timezone", brokerDetails["TimeZone"] },
                { "offline", false }
            };
            
            return final;
        }
        catch (Exception e)
        {
            _logging.LogError("Error While Creating JSON.\n", e);
            throw new Exception("Error While Creating JSON.\n" + e.Message, e);
        }
    }
}
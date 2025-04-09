using Newtonsoft.Json;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.apiMethods.JsonMethods
{
    public class BrokerHubJson
    {
        private readonly ILogging _logging;
        public BrokerHubJson(ILogging logging) => _logging = logging;

        private static string GetTime(string timeZoneId)
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var utcTime = DateTime.UtcNow;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
            const string formatString = "yyyy-MM-ddTHH:mm:ss.fffZ";

            return localTime.ToString(formatString);
        }

        public string BrokerHubKeepAliveJson(Dictionary<string, string> brokerDetails)
        {
            _logging.LogInformation("BrokerHubJson - BrokerHubKeepAliveJSON");

            try
            {
                var body = new Dictionary<string, Object>();

                body.Add("BrokerInstallationPath", brokerDetails["BrokerInstallationPath"]);
                body.Add("BrokerVersion", brokerDetails["BrokerVersion"]);
                body.Add("Ip", brokerDetails["BrokerIP"]);
                body.Add("JavaVersion", brokerDetails["JavaVersion"]);
                body.Add("MachineName", brokerDetails["HostName"]);
                body.Add("MachineOperatingSystem", brokerDetails["MachineOperatingSystem"]);
                body.Add("LmTimeZone", brokerDetails["LmTimeZone"]);
                body.Add("BrokerId", brokerDetails["BrokerId"]);
                body.Add("ServerLocalTime", GetTime(brokerDetails["LmTimeZone"]));
                body.Add("MsgId", 0);
                body.Add("Offline", true);

                var jsonString = JsonConvert.SerializeObject(body);
                _logging.LogInformation(jsonString);

                return jsonString;
            }
            catch (Exception e)
            {
                _logging.LogError("Error", e);
                throw new RuntimeException("Error While Creating Broker Keep Alive JSON. \n");
            }
        }

        public string ApproveBrokerJson(Dictionary<string, string> brokerDetails)
        {
            _logging.LogInformation("BrokerHubJson - ApproveBrokerJson");

            try
            {
                var body = new Dictionary<string, Object>();
                body.Add("Brokerid", brokerDetails["BrokerId"]);
                body.Add("Hostname", brokerDetails["HostName"].ToLower());

                var brokerIds = new List<Dictionary<string, object>>();
                brokerIds.Add(body);

                var final = new Dictionary<string, object>();
                final.Add("brokerIds", brokerIds);

                var jsonString = JsonConvert.SerializeObject(final);
                _logging.LogInformation(jsonString);

                return jsonString;
            }
            catch (Exception e)
            {
                _logging.LogError("Error", e);
                throw new RuntimeException("Error While Creating Approve Broker JSON. \n");
            }
        }

        public string DeleteBrokerJson(Dictionary<string, string> brokerDetails)
        {
            _logging.LogInformation("BrokerHubJson - DeleteBrokerJson");

            try
            {
                var body = new Dictionary<string, Object>();
                body.Add("Brokerid", brokerDetails["BrokerId"]);
                body.Add("Hostname", brokerDetails["HostName"].ToLower());

                var brokerIds = new List<Dictionary<string, object>>();
                brokerIds.Add(body);

                var final = new Dictionary<string, object>();
                final.Add("brokerIds", brokerIds);

                var jsonString = JsonConvert.SerializeObject(final);
                _logging.LogInformation(jsonString);

                return jsonString;
            }
            catch (Exception e)
            {
                _logging.LogError("Error", e);
                throw new RuntimeException("Error While Creating Approve Broker JSON. \n");
            }
        }
        
        public string BrokerHubConfigSingleJson(Dictionary<string, string> brokerDetails,
            Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("BrokerHubJson - BrokerHubConfigSingleJson");

            try
            {
                var body = new Dictionary<string, object>();

                var configuration = new Dictionary<string, object>
                {
                    { "WebUIPort", brokerDetails["WebPort"] }
                };

                var servers = new List<Dictionary<string, object>>();
                var serversMap = new Dictionary<string, object>
                {
                    { "Host", brokerDetails["HostName"] }
                };

                var ports = new List<Dictionary<string, object>>();

                if (!string.IsNullOrEmpty(brokerDetails["Type"]))
                {
                    var portMap = new Dictionary<string, object>
                    {
                        { "LmType", brokerDetails["Type"] },
                        { "Name", brokerDetails["ServerName"] },
                        { "PortNumber", brokerDetails["Port"] },
                        { "Charset", "UTF-8" },
                        { "LicenseFilePath", "" },
                        { "ManualLicenseFilePath", "" },
                        { "IsLicensePathManuallyConfigured", false },
                        { "IsWatchLicenseFile", false },
                        { "IntervalWatchLicenseFile", 300 },
                        {
                            "IsActive",
                            !string.IsNullOrEmpty(brokerDetails["IsActive"])
                                ? bool.Parse(brokerDetails["IsActive"])
                                : true
                        },
                        { "SortingAllowed", false }
                    };

                    var commands = new List<Dictionary<string, object>>();

                    var commandsStatusMap = new Dictionary<string, object>
                    {
                        { "Format", "Raw" },
                        { "Key", "status" },
                        { "CommandLine", brokerDetails["status_C_Line"] + brokerDetails["ServerName"] + " -i" },
                        { "Type", brokerDetails["status_C_Type"] },
                        { "TimeInterval", 55 },
                        { "Enabled", true },
                        { "NeedPromptExec", false },
                        { "CommandSeparator", ";" },
                        { "SuppressTimeoutErrorOutput", false },
                        { "SuppressExitCodeValidation", false },
                        { "CommandTimeout", 57 }
                    };
                    commands.Add(commandsStatusMap);

                    var commandsDataMap = new Dictionary<string, object>
                    {
                        { "Format", "Raw" },
                        { "Key", "data_inquiry" },
                        { "CommandLine", brokerDetails["data_inquiry_C_Line"] + brokerDetails["ServerName"] + " -i" },
                        { "Type", brokerDetails["data_inquiry_C_Type"] },
                        { "TimeInterval", int.Parse(brokerDetails["data_inquiry_TimeInterval"]) },
                        { "Enabled", true },
                        { "NeedPromptExec", false },
                        { "CommandSeparator", ";" },
                        { "SuppressTimeoutErrorOutput", false },
                        { "SuppressExitCodeValidation", false },
                        { "CommandTimeout", 57 }
                    };
                    commands.Add(commandsDataMap);

                    var commandsStartMap = new Dictionary<string, object>
                    {
                        { "Format", "Raw" },
                        { "Key", "start" },
                        { "CommandLine", brokerDetails["start_C_Line"] + brokerDetails["ServerName"] + " -i" },
                        { "Type", brokerDetails["start_C_Type"] },
                        { "TimeInterval", 55 },
                        { "Enabled", true },
                        { "NeedPromptExec", false },
                        { "CommandSeparator", ";" },
                        { "SuppressTimeoutErrorOutput", false },
                        { "SuppressExitCodeValidation", false },
                        { "CommandTimeout", 60 }
                    };
                    commands.Add(commandsStartMap);

                    var commandsStopMap = new Dictionary<string, object>
                    {
                        { "Format", "Raw" },
                        { "Key", "stop" },
                        { "CommandLine", brokerDetails["stop_C_Line"] + brokerDetails["ServerName"] + " -i" },
                        { "Type", brokerDetails["stop_C_Type"] },
                        { "TimeInterval", 55 },
                        { "Enabled", true },
                        { "NeedPromptExec", false },
                        { "CommandSeparator", ";" },
                        { "SuppressTimeoutErrorOutput", false },
                        { "SuppressExitCodeValidation", false },
                        { "CommandTimeout", 60 }
                    };
                    commands.Add(commandsStopMap);

                    portMap["Commands"] = commands;

                    var logFiles = new List<Dictionary<string, object>>();
                    var logFilesMap = new Dictionary<string, object>
                    {
                        { "FilePath", "C:\\Users\\Test\\Downloads\\cert.zip" },
                        { "SizeLimit", 100 },
                        { "TimeInterval", 10 },
                        { "Description", "log" },
                        { "Enabled", true },
                        { "FileNamePattern", true },
                        { "Type", "other" },
                        { "Vendor", brokerDetails["VendorName"] },
                        { "Charset", "UTF-8" }
                    };
                    logFiles.Add(logFilesMap);
                    portMap["LogFiles"] = logFiles;

                    var vendors = new List<Dictionary<string, object>>();
                    var vendorsMap = new Dictionary<string, object>
                    {
                        { "Vendor", brokerDetails["VendorName"] }
                    };
                    var optionFile = new Dictionary<string, object>
                    {
                        { "Path", "" },
                        {
                            "BackupFolderPath", "C:\\Program Files\\OpenLM\\OpenLM Broker\\OpenLM Backup\\Options Files"
                        },
                        { "FileDescription", "" },
                        { "TimeInterval", 600 },
                        { "Enabled", false }
                    };
                    vendorsMap["OptionFile"] = optionFile;
                    vendors.Add(vendorsMap);
                    portMap["Vendors"] = vendors;

                    var assetOrder = new List<Dictionary<string, object>>();
                    portMap["AssetOrder"] = assetOrder;
                    portMap["IsDemo"] = false;
                    portMap["CommandPath"] = "";
                    portMap["UseService"] = true;
                    portMap["Cluster"] = false;
                    portMap["ClusterName"] = "";
                    portMap["SehUtnLayout"] = "";
                    portMap["Locale"] = "";
                    portMap["DatetimeFormat"] = "";
                    ports.Add(portMap);
                }

                serversMap["Ports"] = ports;
                servers.Add(serversMap);
                configuration["Servers"] = servers;

                var receivers = new List<Dictionary<string, object>>();
                var receiversMap = new Dictionary<string, object>();
                var saas = new Dictionary<string, object>
                {
                    { "WsdlLocation", envDetails["broker_wsdl"] },
                    { "Login", envDetails["client_id_broker_hub"] },
                    { "Password", envDetails["client_secret_broker_hub"] },
                    { "LoginSessionMinutes", 20 }
                };
                receiversMap["Saas"] = saas;
                receiversMap["ReceiverUrl"] = "";
                receiversMap["IsActive"] = true;
                receiversMap["Ssl"] = false;
                receiversMap["ActivateBuffering"] = false;
                receiversMap["Compressed"] = false;
                receiversMap["MaxNumberOfMessagesInBuffer"] = 1000;
                receiversMap["MaxBufferFileSizeKb"] = 5120;
                receiversMap["SendingTimeOut"] = 45;
                receiversMap["EntryId"] = envDetails["broker_receiver_entry"];
                receivers.Add(receiversMap);
                configuration["Receivers"] = receivers;

                configuration["FileName"] = "C:\\Program Files\\OpenLM\\OpenLM Broker\\broker.xml";
                var advanceSettingMap = new Dictionary<string, object>
                {
                    { "ServiceLogLevel", "ALL" },
                    { "ConfiguratorLogLevel", "ALL" }
                };
                configuration["AdvanceSetting"] = advanceSettingMap;

                body["Configuration"] = configuration;
                body["MachineName"] = brokerDetails["HostName"];
                body["ServerLocalTime"] = DateTime.UtcNow;
                body["MsgId"] = 0;
                body["offline"] = false;
                body["BrokerId"] = brokerDetails["BrokerId"];

                var json = JsonConvert.SerializeObject(body, Formatting.Indented);

                return json;
            }
            catch (Exception e)
            {
                _logging.LogError("Error :: ", e);
                throw new RuntimeException("Error While Creating Broker Config JSON. \n");
            }
        }

        public string BrokerHubConfigWithoutPortSingleJson(Dictionary<string, string> brokerDetails,
            Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("BrokerHubJson - BrokerHubConfigWithoutPortSingleJson");

            try
            {
                var body = new Dictionary<string, object>();

                var configuration = new Dictionary<string, object>
                {
                    { "WebUIPort", brokerDetails["WebPort"] }
                };

                var servers = new List<Dictionary<string, object>>();
                var serversMap = new Dictionary<string, object>
                {
                    { "Host", brokerDetails["HostName"] }
                };

                // var Ports = new List<Dictionary<string, object>>();

                // ServersMap["Ports"] = Ports;
                servers.Add(serversMap);
                configuration["Servers"] = servers;

                var receivers = new List<Dictionary<string, object>>();
                var receiversMap = new Dictionary<string, object>();
                var saas = new Dictionary<string, object>
                {
                    { "WsdlLocation", "https://qat-microservice-a1.openlm.com:7015" },
                    { "Login", envDetails["client_id_broker_hub"] },
                    { "Password", envDetails["client_secret_broker_hub"] },
                    { "LoginSessionMinutes", 20 }
                };
                receiversMap["Saas"] = saas;
                receiversMap["ReceiverUrl"] = null;
                receiversMap["IsActive"] = true;
                receiversMap["Ssl"] = false;
                receiversMap["ActivateBuffering"] = false;
                receiversMap["Compressed"] = false;
                receiversMap["MaxNumberOfMessagesInBuffer"] = 1000;
                receiversMap["MaxBufferFileSizeKb"] = 5120;
                receiversMap["SendingTimeOut"] = 45;
                receiversMap["EntryId"] = "entry-1999";
                receivers.Add(receiversMap);
                configuration["Receivers"] = receivers;

                configuration["FileName"] = "C:\\Program Files\\OpenLM\\OpenLM Broker\\broker.xml";
                var advanceSettingMap = new Dictionary<string, object>
                {
                    { "ServiceLogLevel", "ALL" },
                    { "ConfiguratorLogLevel", "ALL" }
                };
                configuration["AdvanceSetting"] = advanceSettingMap;

                body["Configuration"] = configuration;
                body["MachineName"] = brokerDetails["HostName"];
                body["ServerLocalTime"] = DateTime.UtcNow;
                body["MsgId"] = 2;
                body["offline"] = false;
                body["BrokerId"] = brokerDetails["BrokerId"];

                var json = JsonConvert.SerializeObject(body, Formatting.Indented);

                return json;
            }
            catch (Exception e)
            {
                _logging.LogError("Error :: ", e);
                throw new RuntimeException("Error While Creating Broker Config JSON. \n");
            }
        }
    }
}
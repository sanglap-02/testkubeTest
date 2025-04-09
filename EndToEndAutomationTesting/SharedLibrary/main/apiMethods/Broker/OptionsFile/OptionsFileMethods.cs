using Microsoft.IdentityModel.Tokens;
using NPOI.Util;
using SharedLibrary.Framework.Reports;
using SharedLibrary.main.apiMethods.JSONmethods;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;
using System.Text;
using System.Text.RegularExpressions;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;

namespace SharedLibrary.main.apiMethods.Broker.OptionsFile
{
    public class OptionsFileMethods
    {
        private readonly ILogging _logging;

        public OptionsFileMethods(ILogging logging) => _logging = logging;
        
        private string GetTime(string timeZoneId)
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var utcTime = DateTime.UtcNow;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);

            var formatString = "yyyy-MM-ddTHH:mm:ss.fffZ";

            return localTime.ToString(formatString);
        }
        
        public Dictionary<string, string> SetOptionFilesDetails(string database, string tableName, string tableRow, Dictionary<string, string> lmData)
        {
            _logging.LogInformation("OptionsFileMethods - SetOptionsFileDetails");
            try
            {
                return SetOptionsFileDetails(database, tableName, tableRow, null, lmData);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Setting Options File Details." + ex);
            }
        }
        
        public Dictionary<string, string> SetOptionsFileDetails(string database, string tableName, string tableRow, string? tableRowCrc, Dictionary<string, string> lmData)
        {
            _logging.LogInformation("OptionsFileMethods - setOptionsFileDetails");

            var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
            var sqlServerQueries = new SqlServerQueries(new Logging());
            
            try
            {
                _logging.LogInformation("Setting Options File Details in Test Sheet.");
                ExtentManager.LogInfo("Setting Options File Details in Test Sheet.");

                var generateData = sqlTableUtilsQat.GetCellData(database, tableName, tableRow, "TEST_ID", "GenerateCrc");

                var optionsFile = new Dictionary<string, string>();

                if (string.Equals(generateData, "yes", StringComparison.OrdinalIgnoreCase))
                {
                    Thread.Sleep(1);
                    _logging.LogInformation("Generating Options File Details in SQL Table.");
                    if (!lmData.IsNullOrEmpty())
                    {
                        optionsFile.Add("Port", lmData["Port"]);
                        optionsFile.Add("ServerHost", lmData["HostName"]);
                        optionsFile.Add("Vendor", lmData["VendorName"]);
                        optionsFile.Add("BrokerId", lmData["BrokerId"]);
                        optionsFile.Add("LMType", lmData["Type"]);
                        optionsFile.Add("ServerLocalTime", GetTime(lmData["LmTimeZone"]));

                        var rand = new Random();
                        var minValue = 100000000;
                        var maxValue = 2147483647;
                        var randomNumber = rand.Next(minValue, maxValue);
                        var crc = randomNumber.ToString("D9");
                        optionsFile.Add("Crc", crc);
                    }
                }
                else if (string.Equals(generateData, "no", StringComparison.OrdinalIgnoreCase))
                {
                    _logging.LogInformation("Using Same ServerHost.");
                    var serverHost = sqlTableUtilsQat.GetCellData(database, tableName, tableRowCrc, "TEST_ID", "ServerHost");
                    optionsFile.Add("ServerHost", serverHost);

                    _logging.LogInformation("Using Same Port.");
                    var port = sqlTableUtilsQat.GetCellData(database, tableName, tableRowCrc, "TEST_ID", "Port");
                    optionsFile.Add("Port", port);

                    _logging.LogInformation("Using Same CRC.");
                    var crc = sqlTableUtilsQat.GetCellData(database, tableName, tableRowCrc, "TEST_ID", "Crc");
                    optionsFile.Add("Crc", crc);

                    _logging.LogInformation("Using Same BrokerId.");
                    var brokerId = sqlTableUtilsQat.GetCellData(database, tableName, tableRowCrc, "TEST_ID", "BrokerId");
                    optionsFile.Add("BrokerId", brokerId);

                    optionsFile.Add("ServerLocalTime", GetTime(lmData["LmTimeZone"]));

                    _logging.LogInformation("Using Options File Details in Test Sheet.");
                }
                else if (string.Equals(generateData, "yes", StringComparison.OrdinalIgnoreCase))
                {
                    _logging.LogInformation("Using Same ServerHost.");
                    var serverHost = sqlTableUtilsQat.GetCellData(database, tableName, tableRowCrc, "TEST_ID", "ServerHost");
                    optionsFile.Add("ServerHost", serverHost);

                    _logging.LogInformation("Using Same Port.");
                    var port = sqlTableUtilsQat.GetCellData(database, tableName, tableRowCrc, "TEST_ID", "Port");
                    optionsFile.Add("Port", port);

                    var rand = new Random();
                    var minValue = 100000000;
                    var maxValue = 2147483647;
                    var randomNumber = rand.Next(minValue, maxValue);
                    var crc = randomNumber.ToString("D9");
                    optionsFile.Add("Crc", crc);

                    _logging.LogInformation("Using Same BrokerId.");
                    var brokerId = sqlTableUtilsQat.GetCellData(database, tableName, tableRowCrc, "TEST_ID",  "BrokerId");
                    optionsFile.Add("BrokerId", brokerId);

                    optionsFile.Add("ServerLocalTime", GetTime(lmData["LmTimeZone"]));
                }

                // update sql table
                var query = sqlServerQueries.UpdateQuerySql(tableName, optionsFile, tableRow);
                _logging.LogInformation(query);
                sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

                // format options file 
                var optionsFileData = sqlTableUtilsQat.GetCellData(database, tableName, tableRow, "TEST_ID", "Data");

                var trimmedOptionsFile = optionsFileData.Replace("\r\n\r\n", "\r\n");
                trimmedOptionsFile = trimmedOptionsFile.Replace("\n\n", "\n");

                string[] lines = Regex.Split(trimmedOptionsFile, @"\r\n|\r|\n");
                var optionsFileRows = lines.Length;

                var optionFileDetails = sqlTableUtilsQat.GetSqlTableMap(tableName, database, tableRow);

                optionFileDetails.Add("optionFileData", optionsFileData);
                optionFileDetails.Add("optionsFileRows", optionsFileRows.ToString());

                return optionFileDetails;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Setting Options File Details." + ex);
            }
        }

        public async Task AddOptionsFiles(Dictionary<string, string> optionsFileDetails, Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("OptionsFileMethods - addOptionsFiles");

            var optionsFileJson = new OptionsFilesJson(new Logging());
            
            try
            {
                ExtentManager.LogInfo("Adding Options Files to Broker-Hub");

                var apiUrl = envDetails["APIBaseUrl"] + UrlConstants.BrokerOptionsFiles;
                _logging.LogInformation("Options Files URL to be use :: " + apiUrl);
                ExtentManager.LogInfo("Options Files URL to be use :: " + apiUrl);

                // Get API Payload for options Files
                string optionsFilesJson = optionsFileJson.OptionFilesJson(optionsFileDetails);
                _logging.LogInformation($"optionsFilesJson - {optionsFilesJson}");
                ExtentManager.LogInfo("Options Files Payload :: \n " + optionsFilesJson);

                await BrokerOptionFiles(apiUrl, optionsFilesJson, envDetails["brokerHubToken"]);
                
                _logging.LogInformation("Options Files Added.");
                ExtentManager.LogPass("Options Files Added.");
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Adding Options Files to Broker Hub. \n");
            }
        }
        
        private async Task BrokerOptionFiles(string apiUrl, string payLoad, string token)
        {
            _logging.LogInformation("OptionsFileMethods - BrokerOptionFiles");
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var content = new StringContent(payLoad, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiUrl, content);

                    if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                    {
                        _logging.LogInformation($"Failed while approving Broker. Expected Status Code: 204, Actual Status Code: {(int)response.StatusCode}");
                    }
                }
            }
            catch (Exception e)
            {
                _logging.LogError("Error :: ", e);
                throw new Exception("Error While calling Broker Options Call. \n");
            }
        }
    }
}

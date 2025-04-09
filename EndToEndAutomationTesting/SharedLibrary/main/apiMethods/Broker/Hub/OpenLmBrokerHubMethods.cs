using Newtonsoft.Json.Linq;
using NPOI.Util;
using SharedLibrary.Framework.Reports;
using SharedLibrary.Framework.VerificationMethods;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;
using System.Text;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.JsonMethods;

namespace SharedLibrary.main.apiMethods.Broker.Hub;

public class OpenLmBrokerHubMethods
{
    private readonly ILogging _logging;
    public OpenLmBrokerHubMethods(ILogging logging) => _logging = logging;
    
    private static readonly HttpClient Client = new HttpClient();

    public Dictionary<string, string> SetBrokerDetails(string database, string tableName, string testId)
    {
        _logging.LogInformation("OpenLmBrokerHubMethods - SetBrokerDetails");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var brokerHubDetails = new Dictionary<string, string>();
        
        try
        {
            var generateData = sqlTableUtilsQat.GetCellData(database, tableName, testId, "TEST_ID", "GenerateData");

            if (string.Equals(generateData, "yes", StringComparison.OrdinalIgnoreCase))
            {
                var rand = new Random();
                var randInt = rand.Next(1, 9);

                var randomInt = string.Format("{0:D4}", rand.Next(1001, 9999));

                var hostname = "TEST-HOST-" + randomInt;
                brokerHubDetails.Add("HostName", hostname);

                brokerHubDetails.Add("WebPort", randomInt);

                var brokerIp = "10." + rand.Next(256) + "." + rand.Next(256) + "." + rand.Next(256);
                brokerHubDetails.Add("BrokerIP", brokerIp);

                var brokerVersion = "23." + rand.Next(9) + "." + rand.Next(9) + "." + rand.Next(999);
                brokerHubDetails.Add("BrokerVersion", brokerVersion);

                const string javaVersion = "11.0.15";
                brokerHubDetails.Add("JavaVersion", javaVersion);

                const string machineOperatingSystem = "Windows 10 (10.0) amd64";
                brokerHubDetails.Add("MachineOperatingSystem", machineOperatingSystem);

                //string timeZone = SqlTableUtilsQAT.getCellData(database, tableName, test_ID, "LmTimeZone");
                //if (timeZone.Trim().Length == 0)
                //{
                //    brokerHubDetails.Add("LmTimeZone", "India Standard Time");
                //}

                brokerHubDetails.Add("LmTimeZone", "Israel Standard Time");

                var brokerId = "broker-" + hostname.ToLower();
                brokerHubDetails.Add("BrokerId", brokerId);

                var brokerInstallationPath = $@"C:\Program Files\OpenLM\OpenLM Broker {randInt}\.";
                brokerHubDetails.Add("BrokerInstallationPath", brokerInstallationPath);

                brokerHubDetails.Add("kafkaEventType", "");
            }
            else
            {
                _logging.LogInformation("Using Existing Broker Details From Db for Row :: " + testId);
            }
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new Exception("Error While Setting Broker Details.");
        }

        return brokerHubDetails;
    }

    public async Task AddBroker(Dictionary<string, string> brokerData, Dictionary<string, string> envDetails)
    {
        _logging.LogInformation("OpenLMBrokerHubMethods - AddBroker");

        var brokerHubJson = new BrokerHubJson(new Logging());
        
        try
        {
            var useExisting = brokerData["UseExisting"];
            if (string.Equals(useExisting, "no", StringComparison.OrdinalIgnoreCase))
            {
                var apiUrl = envDetails["APIBaseUrl"] + UrlConstants.BrokerKeepAlive;
                _logging.LogInformation("Keep Alive URL to be use:: " + apiUrl);
                ExtentManager.LogInfo("Keep Alive URL to be use :: " + apiUrl);

                var brokerKeepAlive = brokerHubJson.BrokerHubKeepAliveJson(brokerData);

                ExtentManager.LogInfo("Broker Keep Alive payLoad :: \n " + brokerKeepAlive);
                await BrokerKeepAliveCall(apiUrl, brokerKeepAlive, envDetails["brokerHubToken"]);

                _logging.LogInformation("New Broker Added.");
                ExtentManager.LogPass("New Broker Added.");
            }
            else
            {
                ExtentManager.LogInfo("Using Existing Broker in Broker-Hub from SQL Database Table.");
            }
        }
        catch (Exception e)
        {
            _logging.LogError("Error :: ", e);
            throw new RuntimeException("Error While Adding Broker to Broker Hub. \n" + e);
        }
    }
    
    private async Task BrokerKeepAliveCall(string apiUrl, string payLoad, string token)
    {
        _logging.LogInformation("OpenLMBrokerHub_Methods - BrokerKeepAliveCall");
        try
        {
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response =
                await Client.PutAsync(apiUrl, new StringContent(payLoad, Encoding.UTF8, "application/json"));
            
            Assert.That((int)response.StatusCode, Is.EqualTo(204), "The status code is not as expected.");

        }
        catch (Exception e)
        {
            _logging.LogError("Error :: ", e);
            throw new RuntimeException("Error While calling Broker Keep Alive. \n" + e);
        }
    }

    public Dictionary<string, string> AddBrokerSql(Dictionary<string, string> brokerDetails, string whereClause,
        string database, string tableName, string testId)
    {
        _logging.LogInformation("OpenLMBrokerHub_Methods - AddBrokerSql");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());
        
        try
        {
            var query = sqlServerQueries.UpdateQuerySql(tableName, brokerDetails,
                whereClause);
            _logging.LogInformation("Query: " + query);

            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);
            brokerDetails = sqlTableUtilsQat.GetSqlTableMap(tableName, database, testId);
            _logging.LogInformation("Broker Details successfully added to sql database.");

            return brokerDetails;
        }
        catch (Exception e)
        {
            _logging.LogError("Error :: ", e);
            throw new RuntimeException("Error While adding broker details to sql server.\n" + e);
        }
    }

    // public Dictionary<string, string> AddBrokerSql(Dictionary<string, string> brokerDetails, string whereClause,
    //     string database, string table, string testId)
    // {
    //     _logging.LogInformation("OpenLMBrokerHub_Methods - AddBrokerSql");
    //     
    //     var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
    //     
    //     try
    //     {
    //         var query = SqlServerQueries.UpdateQuerySql(table, brokerDetails,
    //             whereClause);
    //         _logging.LogInformation("Query: " + query);
    //
    //         sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);
    //         brokerDetails = sqlTableUtilsQat.GetSqlTableMap(table, database, testId);
    //         _logging.LogInformation("Broker Details successfully added to sql database.");
    //
    //         return brokerDetails;
    //     }
    //     catch (Exception e)
    //     {
    //         _logging.LogError("Error :: ", e);
    //         throw new RuntimeException("Error While adding broker details to sql server.\n" + e);
    //     }
    // }

    public async Task ApproveBroker(Dictionary<string, string> brokerData, Dictionary<string, string> envDetails)
    {
        _logging.LogInformation("OpenLMBrokerHub_Methods - ApproveBroker");

        var brokerHubJson = new BrokerHubJson(new Logging());
        var verificationMethods = new VerificationMethods(new Logging());
        
        try
        {
            var useExisting = brokerData["UseExisting"];
            if (string.Equals(useExisting, "no", StringComparison.OrdinalIgnoreCase))
            {
                var apiUrl = envDetails["APIBaseUrl"] + UrlConstants.BrokerHubBrokers;
                _logging.LogInformation("Approve Broker URL to be use :: " + apiUrl);
                ExtentManager.LogInfo("Approve Broker URL to be use :: " + apiUrl);

                var approveBroker = brokerHubJson.ApproveBrokerJson(brokerData);
                _logging.LogInformation($"approveBroker {approveBroker}");

                var brokerApproved = await ApproveBrokerCall(apiUrl, approveBroker, envDetails["brokerHubToken"]);

                verificationMethods.VerifyBooleanEquals(true, brokerApproved, "Broker Approved");

                Assert.That(brokerApproved, Is.True);

                ExtentManager.LogInfo("Broker Approve payLoad :: \n " + approveBroker);
            }
        }
        catch (Exception e)
        {
             _logging.LogError("Error :: ", e);
            throw new RuntimeException("Error While Approving Broker in Broker Hub. \n" + e);
        }
    }

    private async Task<bool> ApproveBrokerCall(string apiUrl, string payLoad, string token)
    {
        _logging.LogInformation("OpenLmBrokerHubMethods - ApproveBrokerCall");
        
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent(payLoad, Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(apiUrl, content);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logging.LogInformation(
                        $"Failed while approving Broker. Expected Status Code: 200, Actual Status Code: {(int)response.StatusCode}");
                    return false;
                }

                try
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseBody);
                    var responseValue = jsonResponse["Errormsg"]?.ToString();

                    return string.IsNullOrWhiteSpace(responseValue);
                }
                catch (Exception e)
                {
                    _logging.LogError("Error :: ", e);
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            _logging.LogError("Error :: ", e);
            return false;
        }
    }

    private async Task<bool> DeleteBrokerCall(string apiUrl, string payLoad, string token)
    {
        _logging.LogInformation("OpenLmBrokerHubMethods - DeleteBrokerCall");

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var request = new HttpRequestMessage(HttpMethod.Delete, apiUrl)
                {
                    Content = new StringContent(payLoad, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && 
                    response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    _logging.LogInformation(
                        $"Failed to delete resource. Expected 200/204, got {(int)response.StatusCode} - {response.ReasonPhrase}");
                    return false;
                }
                
                try
                {
                    return (response.StatusCode == System.Net.HttpStatusCode.OK);
                }
                catch (Exception e)
                {
                    _logging.LogError("Error parsing response: ", e);
                    return false;
                }
            }

        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            return false;
        }
    }

    public async Task BrokerConfigWithoutPort(Dictionary<string, string> brokerData,
        Dictionary<string, string> envDetails)
    {
        _logging.LogInformation("OpenLMBrokerHub_Methods - BrokerConfigWithoutPort");

        var brokerHubJson = new BrokerHubJson(new Logging());
        var verificationMethods = new VerificationMethods(new Logging());
        
        try
        {
            var useExisting = brokerData["UseExisting"];
            if (string.Equals(useExisting, "no", StringComparison.OrdinalIgnoreCase))
            {
                _logging.LogInformation("Add Update Broker Config");

                var apiUrl = envDetails["APIBaseUrl"] + UrlConstants.BrokerHubBrokerConfig;
                _logging.LogInformation("Broker Config URL to be use :: " + apiUrl);
                ExtentManager.LogInfo("Broker Config URL to be use :: " + apiUrl);

                // Get API payLoad for Broker Config
                var brokerConfig = brokerHubJson.BrokerHubConfigWithoutPortSingleJson(brokerData, envDetails);
                _logging.LogInformation($"brokerConfig - {brokerConfig}");
                ExtentManager.LogInfo("Broker Config payLoad :: \n " + brokerConfig);

                var brokerConfigResult = await BrokerConfigCall(apiUrl, brokerConfig, envDetails["brokerHubToken"]);
                verificationMethods.VerifyBooleanEquals(true, brokerConfigResult, "Broker Config call");

                Assert.That(brokerConfigResult, Is.True);

                ExtentManager.LogInfo("Broker Config payLoad :: \n " + brokerConfigResult);
            }
        }
        catch (Exception e)
        {
             _logging.LogError("Error :: ", e);
            throw new RuntimeException("Error While calling Broker Config Call. \n" + e);
        }
    }

    public async Task BrokerConfig(Dictionary<string, string> brokerData, Dictionary<string, string> envDetails)
    {
        _logging.LogInformation("OpenLMBrokerHubMethods - BrokerConfig");
        
        var brokerHubJson = new BrokerHubJson(new Logging());
        var verificationMethods = new VerificationMethods(new Logging());
        
        try
        {
            var useExisting = brokerData["UseExisting"];
            if (string.Equals(useExisting, "no", StringComparison.OrdinalIgnoreCase))
            {
                _logging.LogInformation("Add Update Broker Config");

                var apiUrl = envDetails["APIBaseUrl"] + UrlConstants.BrokerHubBrokerConfig;
                _logging.LogInformation("Broker Config URL to be use :: " + apiUrl);
                ExtentManager.LogInfo("Broker Config URL to be use :: " + apiUrl);

                // Get API payLoad for Broker Config
                var brokerConfig = brokerHubJson.BrokerHubConfigSingleJson(brokerData, envDetails);
                _logging.LogInformation($"brokerConfig - {brokerConfig}");
                ExtentManager.LogInfo("Broker Config payLoad :: \n " + brokerConfig);

                var brokerConfigResult = await BrokerConfigCall(apiUrl, brokerConfig, envDetails["brokerHubToken"]);
                verificationMethods.VerifyBooleanEquals(true, brokerConfigResult, "Broker Config call");

                Assert.That(brokerConfigResult, Is.True);

                ExtentManager.LogInfo("Broker Config payLoad :: \n " + brokerConfigResult);
            }
        }
        catch (Exception e)
        {
             _logging.LogError("Error :: ", e);
            throw new RuntimeException("Error While calling Broker Config Call. \n" + e);
        }
    }

    private  async Task<bool> BrokerConfigCall(string apiUrl, string payLoad, string token)
    {
        _logging.LogInformation("OpenLMBrokerHubMethods - BrokerConfigCall");
        
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent(payLoad, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);

                if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    _logging.LogInformation(
                        $"Failed while approving Broker. Expected Status Code: 204, Actual Status Code: {(int)response.StatusCode}");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            _logging.LogError("Error :: ", e);
            throw new Exception("Error While calling Broker Config Call. \n" + e);
        }
    }

    public Dictionary<string, string> SetCloudBrokerDetails(string database, string tableName, string tesId)
    {
        _logging.LogInformation("OpenLmBrokerHubMethods - SetCloudBrokerDetails");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var brokerHubDetails = new Dictionary<string, string>();
        
        try
        {
            var generateData = sqlTableUtilsQat.GetCellData(database, tableName, tesId, "TEST_ID", "GenerateData");

            if (string.Equals(generateData, "yes", StringComparison.OrdinalIgnoreCase))
            {
                var rand = new Random();
                var randInt = rand.Next(1, 9);

                var randomInt = string.Format("{0:D4}", rand.Next(1001, 9999));

                var hostname = "TEST-HOST-" + randomInt;
                brokerHubDetails.Add("HostName", hostname);

                brokerHubDetails.Add("WebPort", randomInt);

                var brokerIp = "10." + rand.Next(256) + "." + rand.Next(256) + "." + rand.Next(256);
                brokerHubDetails.Add("BrokerIP", brokerIp);

                var brokerVersion = "23." + rand.Next(9) + "." + rand.Next(9) + "." + rand.Next(999);
                brokerHubDetails.Add("BrokerVersion", brokerVersion);

                const string javaVersion = "11.0.15";
                brokerHubDetails.Add("JavaVersion", javaVersion);

                const string machineOperatingSystem = "Windows 10 (10.0) amd64";
                brokerHubDetails.Add("MachineOperatingSystem", machineOperatingSystem);

                //string timeZone = SqlTableUtilsQAT.getCellData(database, tableName, test_ID, "LmTimeZone");
                //if (timeZone.Trim().Length == 0)
                //{
                //    brokerHubDetails.Add("LmTimeZone", "India Standard Time");
                //}

                brokerHubDetails.Add("LmTimeZone", "India Standard Time");

                const string brokerId = "CloudBroker";
                brokerHubDetails.Add("BrokerId", brokerId);

                var brokerInstallationPath = $@"C:\Program Files\OpenLM\OpenLM Broker {randInt}\.";
                brokerHubDetails.Add("BrokerInstallationPath", brokerInstallationPath);

                brokerHubDetails.Add("kafkaEventType", "");
            }
            else
            {
                _logging.LogInformation("Using Existing Broker Details From Sheet for Row :: " + tesId);
            }
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new Exception("Error While Setting Broker Details.");
        }

        return brokerHubDetails;
    }

    public async Task DeleteBroker(Dictionary<string, string> brokerData, Dictionary<string, string> envDetails)
    {
        _logging.LogInformation("OpenLmBrokerHubMethods - DeleteBroker");
        
        var brokerHubJson = new BrokerHubJson(new Logging());
        var verificationMethods = new VerificationMethods(new Logging());
        
        try
        {
            var apiUrl = envDetails["APIBaseUrl"] + UrlConstants.BrokerHubBrokers;
            _logging.LogInformation("Delete Broker URL to be use :: " + apiUrl);
            ExtentManager.LogInfo("Delete Broker URL to be use :: " + apiUrl);
            
            var deleteBroker = brokerHubJson.DeleteBrokerJson(brokerData);
            _logging.LogInformation($"deleteBroker {deleteBroker}");

            var brokerDelete = await DeleteBrokerCall(apiUrl, deleteBroker, envDetails["brokerHubToken"]);
            
            verificationMethods.VerifyBooleanEquals(true, brokerDelete, "Broker Deleted");

            Assert.That(brokerDelete, Is.True);

            ExtentManager.LogInfo("Broker Approve payLoad :: \n " + brokerDelete);
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new Exception("Error while Delete Broker.");
        }
    }
}
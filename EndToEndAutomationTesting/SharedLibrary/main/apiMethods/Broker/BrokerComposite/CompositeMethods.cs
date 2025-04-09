using System.Net;
using Microsoft.IdentityModel.Tokens;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.Framework.Reports;
using SharedLibrary.main.apiMethods.JsonMethods;
using SharedLibrary.main.auto.framework.apicalls;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.Broker.BrokerComposite;

public class CompositeMethods
{
    private readonly ILogging _logging;
    public CompositeMethods(ILogging logging) => _logging = logging;
    
    private static string _query = "";

    private string GetTime(string timeZoneFormat, string flag)
    {
        _logging.LogInformation("CompositeMethods - GetTime");

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneFormat);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            return flag switch
            {
                "zone" => localTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                "session" => localTime.ToString("ddd MM/dd HH:mm"),
                _ => localTime.ToString("HH:mm:ss")
            };
        }
        catch (Exception ex)
        {
            _logging.LogError("An error occurred", ex);
            return string.Empty;
        }
    }

    public Dictionary<string, string> SetBrokerData(Dictionary<string, string> licenseData, string testId,
        string table, string database)
    {
        _logging.LogInformation("CompositeMethods - SetBrokerData");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());
        
        try
        {
            var brokerId = licenseData["BrokerId"];
            _query = sqlServerQueries.UpdateQuerySql(table, "BrokerId", brokerId, "TEST_ID",
                testId);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(_query, database);

            var generateDate = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "GenerateDate");

            if (generateDate.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                return new Dictionary<string, string>();
            }

            var timeZone = licenseData["TimeZoneFormat"];

            var zoneDate = GetTime(timeZone, "zone");
            var setZoneTime = sqlServerQueries.UpdateQuerySql(table, "ExecutionLocalDate", zoneDate, "TEST_ID", testId);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(setZoneTime, database);

            setZoneTime = sqlServerQueries.UpdateQuerySql(table, "ServerDateLocal", zoneDate, "TEST_ID", testId);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(setZoneTime, database);
            // _logging.LogInformation("setZoneTime \n");

            setZoneTime = sqlServerQueries.UpdateQuerySql(table, "ServerLocalTime", zoneDate, "TEST_ID", testId);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(setZoneTime, database);
            // _logging.LogInformation("setZoneTime \n");

            var sessionDate = GetTime(timeZone, "session");
            var setSessionTime = sqlServerQueries.UpdateQuerySql(table, "SessionDate",
                sessionDate, "TEST_ID", testId);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(setSessionTime, database);

            var denialsData = GetTime(timeZone, "denials");
            var setDenialTime = sqlServerQueries.UpdateQuerySql(table, "DenialTime",
                denialsData, "TEST_ID", testId);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(setDenialTime, database);

            return sqlTableUtilsQat.GetSqlTableMap(table, database, testId);
        }
        catch (Exception ex)
        {
            throw new RuntimeException("Error While Setting Broker Data. \n" + ex.StackTrace);
        }
    }

    public async Task BrokerCompositeCall(Dictionary<string, string> data, Dictionary<string, string> envDetails, string apiBaseUrl, string database,
        string table)
    {
        _logging.LogInformation("CompositeMethods - BrokerCompositeCall");
        try
        {
             await BrokerCompositeCall(data, envDetails, apiBaseUrl, false, database, table);
        }
        catch (Exception ex)
        {
            throw new RuntimeException("Error In Broker Call. \n" + ex.StackTrace);
        }
    }

    private async Task BrokerCompositeCall(Dictionary<string, string> data, Dictionary<string, string> envDetails, string apiBaseUrl, bool closeSession,
        string database, string table)
    {
        _logging.LogInformation("CompositeMethods - BrokerCompositeCall");
        try
        {
            if (closeSession)
            {
                _logging.LogInformation("CompositeMethods - BrokerCompositeCall :: Close");
                await BrokerCompositeCall(data, envDetails, apiBaseUrl, "close", database, table);
            }
            else
            {
                _logging.LogInformation("CompositeMethods - BrokerCompositeCall :: Start");
                await BrokerCompositeCall(data, envDetails, apiBaseUrl, "start", database, table);
            }
        }
        catch (Exception ex)
        {
            throw new RuntimeException("Error In Broker Call. \n" + ex.StackTrace);
        }
    }

    private async Task BrokerCompositeCall(Dictionary<string, string> data, Dictionary<string, string> envDetails, string apiBaseUrl,
        string sessionType, string database, string table)
    {
        _logging.LogInformation("CompositeMethods - BrokerCompositeCall");

        var compositeDataMethods = new CompositeDataMethods(new Logging());
        var brokerJson = new BrokerJson(new Logging());
        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        
        try
        {
            switch (sessionType)
            {
                case "start":
                    ExtentManager.LogInfo("Generating Session via Broker Call");
                    break;
                case "mid":
                    ExtentManager.LogInfo("Session Continue via Broker Call");
                    break;
                case "close":
                    ExtentManager.LogInfo("Closing Session via Broker Call");
                    break;
            }

            data["data_inquiry"] = "";
            var brokerApiUrl = apiBaseUrl + UrlConstants.BrokerComposite;
            _logging.LogInformation($"BrokerCompositeCall == API URL to be use :: {brokerApiUrl}");
            ExtentManager.LogInfo($"BrokerCompositeCall == API URL to be use :: {brokerApiUrl}");

            var dataInquiry = "";
            if (data["data_inquiry"].IsNullOrEmpty() || data["data_inquiry"].Trim().IsNullOrEmpty())
            {
                dataInquiry = compositeDataMethods.GenerateCompositeData(data, sessionType, database, table, data["TEST_ID"]);
            }

            var dateNameSmall = DateTime.Now.ToString("MMddHHmm");
            data["MsgId"] = dateNameSmall;

            var logDataDb = sqlTableUtilsQat.GetCellData(database, table, data["TEST_ID"], "TEST_ID", "LogData");
            data["LogData"] = logDataDb;
            
            if (sessionType.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                data["LogData"] = "FALSE";
            }
            else
            {
                var logData = "";
                if (data["LogData"].Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    if (data["Data"].Trim().IsNullOrEmpty())
                    {
                        logData = compositeDataMethods.GenerateLogData(data);
                    }
                    else
                    {
                        logData = data["Data"];
                    }
                }
            
                data["Data"] = logData;
            }

            var compositeData = sqlTableUtilsQat.GetSqlTableMap(table, database, data["TEST_ID"]);
            
            var brokerCompositeJson = "";
            switch (sqlTableUtilsQat.GetCellData(database, table, data["TEST_ID"], "TEST_ID", "BrokerJsonType"))
            {
                case "1":
                    break;
                case "2":
                    break;
                default:
                    brokerCompositeJson = brokerJson.BrokerCompositeBuildJson(data, compositeData);
                    break;
            }
            
            _logging.LogInformation($"BrokerCall  {brokerCompositeJson}");
            ExtentManager.LogInfo($"Broker Composite Json :: \n {brokerCompositeJson}");
            
            var authToken = sqlTableUtilsQat.GetCellData(database, table, data["TEST_ID"], "TEST_ID", "BrokerJsonType").Equals("BrokerHub", StringComparison.OrdinalIgnoreCase) ? envDetails["brokerHubToken"] : envDetails["ServerAuthToken"];
            
            await CallBrokerComposite(brokerApiUrl, brokerCompositeJson, authToken);

            _logging.LogInformation("Broker Call Made Successfully");
            ExtentManager.LogInfo("Broker Call Made Successfully.");
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Broker Call. \n" + ex.StackTrace);
        }
    }

    private async Task<string> CallBrokerComposite(string apiUrl, string payLoad, string token)
    {
        _logging.LogInformation("CompositeMethods - CallBrokerComposite");

        var restApiCalls = new RestApiCalls(new Logging());
        
        try
        {
            var response = await restApiCalls.RestPostCall(apiUrl, payLoad, token);
            Assert.That(
                response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Created, 
                Is.True, 
                $"Response Status is :: {response.StatusCode}"
            );


            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logging.LogError("Error : ", ex);
            throw new RuntimeException("Error While calling Broker Composite Call. \n" + ex.StackTrace);
        }
    }
}
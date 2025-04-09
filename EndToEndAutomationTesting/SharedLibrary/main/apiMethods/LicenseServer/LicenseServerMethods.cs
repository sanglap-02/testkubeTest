using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.Framework.Reports;
using SharedLibrary.main.apiMethods.JsonMethods;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.graphqlcalls;
using SharedLibrary.main.auto.framework.queries;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.LicenseServer
{
    public class LicenseServerMethods
    {
        private readonly ILogging _logging;
        public LicenseServerMethods(ILogging logging) => _logging = logging;

        public Dictionary<string, string> SetLicenseServerDetails(string database, string tableNameLicSer,
            string tableNameBroker, string testId)
        {
            _logging.LogInformation("LicenseServerMethods - SetLicenseServerDetails");

            var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());

            var licenseServerDetails = new Dictionary<string, string>();
            try
            {
                var generateData =
                    sqlTableUtilsQat.GetCellData(database, tableNameLicSer, testId, "TEST_ID", "GenerateData");

                if (string.Equals(generateData, "yes", StringComparison.OrdinalIgnoreCase))
                {
                    var rand = new Random();

                    var randomInt = string.Format("{0:D4}", rand.Next(1001, 9999));

                    var port = randomInt;
                    licenseServerDetails.Add("Port", port);

                    var hostname =
                        sqlTableUtilsQat.GetCellData(database, tableNameBroker, testId, "TEST_ID", "HostName");
                    licenseServerDetails.Add("HostName", hostname);

                    _logging.LogInformation("Hostname - " + hostname);

                    var lmName = port + "@" + hostname;
                    licenseServerDetails.Add("ServerName", lmName);

                    _logging.LogInformation("LM Name - " + lmName);

                    licenseServerDetails["INSERT_DATE_UTC"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                else
                {
                    _logging.LogInformation("Using Existing Broker Config / Port Details from Test Sheet.");
                }
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Setting Broker Config / License Server Details.");
            }

            return licenseServerDetails;
        }

        public Dictionary<string, string> AddLicenseServerSql(Dictionary<string, string> licenseServerDetails,
            string tableNameLicSer, string whereClause, string database, string testId)
        {
            _logging.LogInformation("LicenseServerMethods - AddLicenseServerSql");

            var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
            var sqlServerQueries = new SqlServerQueries(new Logging());

            try
            {
                var query = sqlServerQueries.UpdateQuerySql(tableNameLicSer, licenseServerDetails, whereClause);
                _logging.LogInformation(query);

                sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

                licenseServerDetails = sqlTableUtilsQat.GetSqlTableMap(tableNameLicSer, database, testId);

                _logging.LogInformation("License Server Details successfully added to sql database.");

                return licenseServerDetails;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While adding license server details to sql server.\n");
            }
        }

        public async Task<string> ApproveLicenseServer(Dictionary<string, string> licenseDetails,
            Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("LicenseServerMethods - ApproveLicenseServer");

            var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
            var operationalApiCalls = new OperationalApiCalls(new Logging());
            var sqlDatabaseQuery = new SqlDatabaseQuery(new Logging());
            var openLmServerGraphQlQuery = new OpenLmServerGraphQlQuery(new Logging());

            try
            {
                var licenseServerId = "";

                if (string.Equals(licenseDetails["UseExisting"], "yes", StringComparison.OrdinalIgnoreCase))
                {
                    ExtentManager.LogInfo("Using existing LM from DB, Port Approval in License Server Skipped.");

                    return sqlTableUtilsQat.GetCellData(envDetails["Database"], envDetails["ServerTable"],
                        envDetails["TestId"], "LicenseID",
                        "TEST_ID");
                }

                var apiUrl = envDetails["ServerUrl"] + UrlConstants.OperationalApiNewGen;

                if (!await VerifyPortInServer(licenseDetails["HostName"].ToLower(), envDetails))
                {
                    return "";
                }

                var query = openLmServerGraphQlQuery.ApproveLicenseServerQuery(licenseDetails);
                _logging.LogInformation("Approve License Server Query :: " + query);

                var response = await operationalApiCalls.GQlPostCall(apiUrl, query, envDetails["ServerAuthToken"]);
                _logging.LogInformation("Response :: " + response);
                ExtentManager.LogInfo("Response :: " + response);

                licenseServerId = response["data"]?["addLicenseServer"]?.ToString() ?? "";
                _logging.LogInformation("LicenseServerId :: " + licenseServerId);
                ExtentManager.LogInfo("License Server Id :: " + licenseServerId);
                ExtentManager.LogPass("License Server Approved.");

                if(!licenseDetails.ContainsKey("ServerTable"))  return licenseServerId;
                
                var idUpdateQuery = sqlDatabaseQuery.UpdateQuery(licenseDetails["ServerTable"],
                    "LicenseID", licenseServerId, licenseDetails["TEST_ID"]);
                _logging.LogInformation("Query: " + idUpdateQuery);
                sqlTableUtilsQat.RunInsertUpdateQuerySql(idUpdateQuery, licenseDetails["Database"]);

                return licenseServerId;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error While Searching License Server. \n", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }

        private async Task<bool> VerifyPortInServer(string hostName, Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("LicenseServerMethods - VerifyPortInServer");

            try
            {
                return await VerifyPortInServer(hostName, envDetails, true);
            }
            catch (Exception ex)
            {
                _logging.LogError("Not verified Port in Server.", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }

        private async Task<bool> VerifyPortInServer(string hostName, Dictionary<string, string> envDetails,
            bool expected)
        {
            _logging.LogInformation("LicenseServerMethods - VerifyPortInServer");

            var licenseServerMethods = new LicenseServerMethods(new Logging());

            try
            {
                ExtentManager.LogInfo("Verify Service/Port in Server.");

                return await licenseServerMethods.SearchLicenseServer(envDetails["ServerUrl"],
                    envDetails["ServerAuthToken"], hostName);
            }
            catch (Exception ex)
            {
                _logging.LogError("Not verified Port in Server.", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }

        private async Task<bool> SearchLicenseServer(string apiUrl, string token, string searchCriteria)
        {
            _logging.LogInformation("LicenseServerMethods - SearchLicenseServer");

            var operationalApiCalls = new OperationalApiCalls(new Logging());
            var openLmServerGraphQlQuery = new OpenLmServerGraphQlQuery(new Logging());

            try
            {
                var apiSearchUrl = apiUrl + UrlConstants.OperationalApiNewGen;
                _logging.LogInformation("API URL to Search License Server :: " + apiSearchUrl);
                ExtentManager.LogInfo("API URL to Search License Server :: " + apiSearchUrl);

                var query = openLmServerGraphQlQuery.PendingLicenseServerQuery(apiUrl);
                _logging.LogInformation($"Pending Query :: {query}");
                var response = await operationalApiCalls.GQlPostCall(apiSearchUrl, query, token);

                var matchingNode = response["data"]?["candidateServersHeaders"]?["nodes"]
                    ?.FirstOrDefault(node => node["hostName"]?.ToString() == searchCriteria);

                if (matchingNode != null)
                {
                    _logging.LogInformation($"Found node with hostName '{searchCriteria}':");
                    _logging.LogInformation(matchingNode.ToString());
                    return true;
                }

                _logging.LogInformation($"No node found with hostName '{searchCriteria}':");
                return false;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error While Searching License Server. \n", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }

        public async Task FindLicenseServerById(Dictionary<string, string> licenseDetails,
            Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("LicenseServerMethods - FindLicenseServerById");

            var operationalApiCalls = new OperationalApiCalls(new Logging());
            var openLmServerGraphQlQuery = new OpenLmServerGraphQlQuery(new Logging());

            try
            {
                var query = openLmServerGraphQlQuery.FindLicenseServerByIdQuery(licenseDetails["licenseServerId"]);
                _logging.LogInformation("Approve License Server Query :: " + query);

                var apiUrl = envDetails["ServerUrl"] + UrlConstants.OperationalApiNewGen;
                var response = await operationalApiCalls.GQlPostCall(apiUrl, query, envDetails["ServerAuthToken"]);
                _logging.LogInformation("Response :: " + response);
                ExtentManager.LogInfo("Response :: " + response);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error While Searching License Server by Id. \n", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }

        public async Task<string> GetLicenseServerStatistics(Dictionary<string, string> licenseDetails,
            Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("LicenseServerMethods - GetLicenseServerStatistics");

            var operationalApiCalls = new OperationalApiCalls(new Logging());
            var openLmServerGraphQlQuery = new OpenLmServerGraphQlQuery(new Logging());
            
            try
            {
                var query = openLmServerGraphQlQuery.GetServerStatisticsQuery(licenseDetails["ServerName"]);
                _logging.LogInformation("Approve License Server Query :: " + query);

                var apiUrl = envDetails["ServerUrl"] + UrlConstants.OperationalApiNewGen;
                var response = await operationalApiCalls.GQlPostCall(apiUrl, query, envDetails["ServerAuthToken"]);
                _logging.LogInformation("Response :: " + response);
                ExtentManager.LogInfo("Response :: " + response);
                
                return response.ToString();
            }
            catch (Exception ex)
            {
                _logging.LogError("Error While getting License Server statistics. \n", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }
        
        public async Task UpdateLicenseServer(string enabled, Dictionary<string, string> licenseDetails,
            Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("LicenseServerMethods - UpdateLicenseServer");

            var operationalApiCalls = new OperationalApiCalls(new Logging());
            var openLmServerGraphQlQuery = new OpenLmServerGraphQlQuery(new Logging());

            try
            {
                var query = openLmServerGraphQlQuery.UpdateLicenseServerQuery(enabled, licenseDetails);
                _logging.LogInformation("Update License Server Query :: " + query);
                ExtentManager.LogInfo("Update License Server Query :: " + query);

                var apiUrl = envDetails["ServerUrl"] + UrlConstants.OperationalApiNewGen;

                Thread.Sleep(2 * 1000);
                var response = await operationalApiCalls.GQlPostCall(apiUrl, query, envDetails["ServerAuthToken"]);
                _logging.LogInformation("Update Response :: " + response);
                ExtentManager.LogInfo("Update Response :: " + response);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error While Updating License Server. \n", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }

        public async Task DeleteLicenseServer(Dictionary<string, string> licenseDetails,
            Dictionary<string, string> envDetails)
        {
            _logging.LogInformation("LicenseServerMethods - DeleteLicenseServer");

            var operationalApiCalls = new OperationalApiCalls(new Logging());
            var openLmServerGraphQlQuery = new OpenLmServerGraphQlQuery(new Logging());

            try
            {
                var query = openLmServerGraphQlQuery.DeleteLicenseServerQuery(licenseDetails["LicenseServerId"]);
                _logging.LogInformation("Update License Server Query :: " + query);
                ExtentManager.LogInfo("Update License Server Query :: " + query);

                var apiUrl = envDetails["ServerUrl"] + UrlConstants.OperationalApiNewGen;
                
                var response = await operationalApiCalls.GQlPostCall(apiUrl, query, envDetails["ServerAuthToken"]);
                _logging.LogInformation("Delete Response :: " + response);
                ExtentManager.LogInfo("Delete Response :: " + response);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error While Deleting License Server. \n", ex);
                throw new RuntimeException(ex.StackTrace);
            }
        }
    }
}
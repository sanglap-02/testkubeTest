using System.Text;
using Microsoft.IdentityModel.Tokens;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.Framework.Reports;
using SharedLibrary.main.apiMethods.JsonMethods;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.Broker.LicenseFile;

public class LicenseFileMethods
{
    private readonly ILogging _logging;

    public LicenseFileMethods(ILogging logging) => _logging = logging;

    private string GetTime(string timeZoneId)
    {
        var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        var utcTime = DateTime.UtcNow;
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);

        const string formatString = "yyyy-MM-ddTHH:mm:ss.fffZ";

        return localTime.ToString(formatString);
    }

    public Dictionary<string, string> SetLicenseFileDetails(string database, string table, string tableRow,
        Dictionary<string, string> lmData)
    {
        _logging.LogInformation("LicenseFileMethods - SetLicenseFileDetails");

        try
        {
            return SetLicenseFileDetails(database, table, tableRow, null, lmData);
        }
        catch (Exception ex)
        {
            _logging.LogError("Not able to set the License File data.", ex);
            Assert.Fail("Not able to set the License File data.");
            throw new RuntimeException();
        }
    }

    public Dictionary<string, string> SetLicenseFileDetails(string database, string table, string tableRow,
        string tableRowCrc,
        Dictionary<string, string> lmData)
    {
        _logging.LogInformation("LicenseFileMethods - SetLicenseFileDetails");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());

        try
        {
            _logging.LogInformation("Setting License File Details in Test Database.");
            ExtentManager.LogInfo("Setting License File Details in Test Database.");

            var licenseCrc = "";

            var generateData = sqlTableUtilsQat.GetCellData(database, table, tableRow, "TEST_ID", "GenerateData");

            if (string.Equals(generateData, "yes", StringComparison.OrdinalIgnoreCase))
            {
                _logging.LogInformation("Generating License File Details in Test Database.");
                if (!lmData.IsNullOrEmpty())
                {
                    var query = sqlServerQueries.UpdateQuerySql(table, "Port", lmData["Port"], "TEST_ID", tableRow);
                    sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

                    query = sqlServerQueries.UpdateQuerySql(table, "ServerHost", lmData["HostName"], "TEST_ID",
                        tableRow);
                    sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

                    query = sqlServerQueries.UpdateQuerySql(table, "BrokerId", lmData["BrokerId"], "TEST_ID",
                        tableRow);
                    sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

                    var timeZone = GetTime(lmData["LmTimeZone"]);
                    query = sqlServerQueries.UpdateQuerySql(table, "ServerLocalTime", timeZone, "TEST_ID",
                        tableRow);
                    sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);
                }

                var rand = new Random();
                var minValue = 100000000;
                var maxValue = 2147483647;
                var randomNumber = rand.Next(minValue, maxValue);
                var crc = randomNumber.ToString("D9");
                licenseCrc = crc;
            }

            var licenseFileData = sqlTableUtilsQat.GetCellData(database, table, tableRow, "Test_ID", "Data");
            if (licenseFileData.IsNullOrEmpty())
            {
                Assert.Fail("License File data can not be empty.");
            }

            var trimmedLicenseFile = licenseFileData.Replace("\r\n\r\n", "\r\n")
                .Replace("\n\n", "\n");

            string[] lines = trimmedLicenseFile.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var licenseFileRows = lines.Length;

            var licenseFileDetails = sqlTableUtilsQat.GetSqlTableMap(table, database, tableRow);

            licenseFileDetails.Add("licenseFileData", licenseFileData);
            licenseFileDetails.Add("licenseFileRows", licenseFileRows.ToString());
            licenseFileDetails.Add("licenseCrc", licenseCrc);

            return licenseFileDetails;
        }
        catch (Exception ex)
        {
            _logging.LogError("No Data set.\n", ex);
            throw new RuntimeException("No Data set." + ex.StackTrace);
        }
    }

    public async Task AddLicenseFile(Dictionary<string, string> licenseFileDetails,
        Dictionary<string, string> envDetails)
    {
        _logging.LogInformation("LicenseFileMethods - AddLicenseFile");

        var licenseFilesJson = new LicenseFilesJson(new Logging());

        try
        {
            ExtentManager.LogInfo("Adding License Files to Broker-Hub");
            var apiUrl = envDetails["APIBaseUrl"] + UrlConstants.BrokerLicenseFiles;
            _logging.LogInformation("License Files Url to be use :: " + apiUrl);
            ExtentManager.LogInfo("License Files Url to be use :: " + apiUrl);

            var licenseFileJson = licenseFilesJson.LicenseFileJson(licenseFileDetails);
            _logging.LogInformation("LicenseFilesJson\n ::" + licenseFileJson);
            ExtentManager.LogInfo("License Files Payload :: \n" + licenseFileJson);

            await BrokerLicenseFiles(apiUrl, licenseFileJson, envDetails["brokerHubToken"]);

            _logging.LogInformation("License Files Added.");
            ExtentManager.LogPass("License Files Added.");
        }
        catch (Exception ex)
        {
            _logging.LogError("Not added License File to Broker Hub.", ex);
            throw new RuntimeException("Error while Adding License Files to Broker Hub" + ex.StackTrace);
        }
    }

    private async Task BrokerLicenseFiles(string apiUrl, string payLoad, string token)
    {
        _logging.LogInformation("LicenseFileMethods - BrokerLicenseFiles");

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
                }
            }
        }
        catch (Exception e)
        {
            _logging.LogError("Error :: ", e);
            throw new Exception("Error While calling Broker Options Call. \n" + e);
        }
    }
}
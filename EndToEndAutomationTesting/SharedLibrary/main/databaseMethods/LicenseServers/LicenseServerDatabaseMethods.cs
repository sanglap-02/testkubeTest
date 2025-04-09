using System.Text;
using Newtonsoft.Json;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.Framework.Reports;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.framework.sqlDatabase;

namespace SharedLibrary.main.databaseMethods.LicenseServers;

public class LicenseServerDatabaseMethods
{
    private readonly ILogging _logging;

    public LicenseServerDatabaseMethods(ILogging logging) => _logging = logging;
    
    public string GetSampleRate(string database, string rowName)
    {
        _logging.LogInformation("LicenseServerDatabaseMethods - GetSampleRate");
        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        
        try
        {
            var candidateJson = sqlTableUtilsQat.GetCellData(database, "[server-qatk8sonprem].[dbo].[OLM_CANDIDATE_HOSTS]", rowName, "HOSTNAME", "CANDIDATE_JSON");

            var jsonValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(candidateJson);

            string? sampleRateAsString = "";
            if (jsonValue.TryGetValue("SampleRate", out var sampleRateValue))
            {
                sampleRateAsString = sampleRateValue.ToString();
            }

            _logging.LogInformation("SampleRate: " + sampleRateAsString);

            return sampleRateAsString ?? "";
        }
        catch (Exception e)
        {
            _logging.LogError("Error while fetch Candidate Json value", e);
            throw new RuntimeException(e.StackTrace);
        }
    }
    
    public Dictionary<string, string> GetAllocationDetails(Dictionary<string, string> envDetails,
        Dictionary<string, string> dataDetails)
    {
        _logging.LogInformation("LicenseServerDatabaseMethods - GetAllocationDetails");
        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        
        try
        {
            var userName = dataDetails["username"];
            
            var query = $"SELECT allocation.ID as allocationID, allocation.LICENSE_ID as LicenseID, allocation.START_DATE_UTC as StartTime, allocation.END_DATE_UTC as EndTime, allocation.USER_NAME as userName FROM [server-qatk8sonprem].[dbo].[OLM_LICENSE_ALLOCATIONS] allocation  where allocation.USER_NAME = '{userName.ToLower()}' ORDER by allocationID DESC;";
            _logging.LogInformation($"Query :: {query}");

            Thread.Sleep(1000);
            var dbValues = sqlTableUtilsQat.GetSqlTableMap(DatabaseConstants.QatServerK8SOnPrem, query);

            Thread.Sleep(120 * 1000);
            
            if (dbValues == null || !dbValues.Any())
            {
                _logging.LogInformation("DatabaseDetails: No details available.");
                ExtentManager.LogInfo("DatabaseDetails: No details available.");
                Assert.Fail("DatabaseDetails: No details available.");
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var pair in dbValues)
                {
                    sb.AppendLine($"{pair.Key}: {pair.Value}, ");
                }
            
                _logging.LogInformation($"DatabaseDetails: {sb}");
                ExtentManager.LogInfo($"DatabaseDetails:: {sb}");
            }
            
            query = $"SELECT ID FROM [server-qatk8sonprem].[dbo].[OLM_LICENSE_SERVERS] WHERE DESCRIPTION = '{dataDetails["ServerName"].ToLower()}';";
            _logging.LogInformation($"Query :: {query}");
            var id = sqlTableUtilsQat.GetCellData(DatabaseConstants.QatServerK8SOnPrem, query);
            Thread.Sleep(90 * 1000);
            _logging.LogInformation($"ID :: {id}");
            dbValues.Add("licenseServerId", Convert.ToString(id));
            
            return dbValues;
        }
        catch (Exception ex)
        {
            throw new RuntimeException("Can not get database record for License Server. \n" + ex.Message);
        }
    }
}
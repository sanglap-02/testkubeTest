using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.Broker.LicenseData;

public class OpenLmGeneric
{
    private readonly ILogging _logging;
    public OpenLmGeneric(ILogging logging) => _logging = logging;
    
    private static string GetDateTime(string timeZoneHost, string flag)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneHost);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        if (flag == "grantedDate")
            return localTime.ToString("MM/dd/yyyy");
        return localTime.ToString("HH:mm:ss tt");
    }

    public string GenerateGenericData(Dictionary<string, string> licenseData, int userCount, bool midCall,
        string database, string table, string testId)
    {
        _logging.LogInformation("OpenLmGeneric - GenerateGenericData");
        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());

        try
        {
            var host = licenseData["HostName"];
            var port = licenseData["Port"];
            var username = licenseData["username"];
            var userHost = licenseData["userhost"].ToLower();
            var vendor = licenseData["VendorName"];
            
            var data =
                $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n<SERVER name=\"{host}\" port=\"{port}\" request_time_utc=\"1726833165000\" server_status=\"ok\">\r\n    <FEATURE exp_date_utc=\"permanent\" inuse=\"1\" license_type=\"Floating\" name=\"Release: {vendor} Preplanning Professional (Add-on) 2.8\" total=\"1\" vendor=\"{vendor}\">\r\n        <USER borrowed=\"false\" linger_time=\"3600\" name=\"{username}\" number_of_lics=\"1\" start_time_utc=\"1726833105000\" workstation=\"{userHost}\"/>\r\n    </FEATURE>\r\n</SERVER>";
            
            var query = sqlServerQueries.UpdateQuerySql(table, "data_inquiry", data, "TEST_ID", licenseData["TEST_ID"]);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating DSLS Data \n" + ex.StackTrace);
        }
    }

}
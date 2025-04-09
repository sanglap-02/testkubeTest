using System.Globalization;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.Broker.LicenseData;

public class RlmData
{
    private readonly ILogging _logging;
    public RlmData(ILogging logging) => _logging = logging;

    private static string GetFeatureValue(Dictionary<string, string> feature, string key)
    {
        return feature.TryGetValue(key, out var value) ? value : "";
    }

    public string GenerateRlmData(Dictionary<string, string> licenseData, int userCount, bool midCall,
        string database, string table, string testId)
    {
        _logging.LogInformation("RlmData - GenerateRlmData");
        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());

        try
        {
            var serverName = licenseData["ServerName"].ToLower();
            var host = licenseData["HostName"];
            var port = licenseData["Port"];
            var vendor = licenseData["VendorName"];

            var feature = SetFeatureDetails(licenseData, userCount, 1, midCall, database, table);
            var featureUsage = GetFeatureValue(feature, "featureUser");
            var vendorStat = GetFeatureValue(feature, "vendorStat");

            var data =
                $"Setting license file path to {serverName}\r\nrlmutil v16.1\r\nCopyright (C) 2006-2024, Reprise Software, Inc. All rights reserved.\r\n\r\n\r\n\trlm status on {host} (port {port}), up 5d 03:09:01\r\n\trlm software version v16.0 (build:2)\r\n\trlm comm version: v1.2\r\n\tStartup time: Thu Jan  9 07:07:30 2025\r\n\tTodays Statistics (10:16:23), init time: Tue Jan 14 00:00:08 2025\r\n\tRecent Statistics (00:06:23), init time: Tue Jan 14 10:10:08 2025\r\n\r\n\t             Recent Stats         Todays Stats         Total Stats\r\n\t              00:06:23             10:16:23          5d 03:09:01\r\n\tMessages:    38 (0/sec)           3142 (0/sec)          37545 (0/sec)\r\n\tConnections: 30 (0/sec)           2520 (0/sec)          30131 (0/sec)\r\n\r\n\t{vendorStat}{vendor} license pool status on {host} (port 1042)\r\n\r\n\t{featureUsage}";

            var query = sqlServerQueries.UpdateQuerySql(table, "data_inquiry", data, "TEST_ID", licenseData["TEST_ID"]);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating RLM Data \n" + ex.StackTrace);
        }
    }

    private Dictionary<string, string> SetFeatureDetails(Dictionary<string, string> licenseDetails,
        int userCount, int featureCount, bool mid, string database, string table)
    {
        _logging.LogInformation("RlmData - SetFeatureDetails");
        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());

        try
        {
            var host = licenseDetails["HostName"];
            var username = licenseDetails["username"];
            var userHost = licenseDetails["userhost"].ToLower();
            var vendor = licenseDetails["VendorName"];

            var serverExecutionTime =
                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(sqlServerQueries.UpdateQuerySql(table, "ExecutionLocalDate",
                serverExecutionTime, "TEST_ID", licenseDetails["TEST_ID"]), database);

            var feature = $"Feature{featureCount}";
            var featureName =
                sqlTableUtilsQat.GetCellData(database, table, licenseDetails["TEST_ID"], "TEST_ID", feature);
            var featureStartTime = sqlTableUtilsQat.GetCellData(database, table, licenseDetails["TEST_ID"], "TEST_ID", $"{feature}StartTime");

            var vendorStat =
                $"--------- ISV servers ----------\r\n\t   Name           Port Running Restarts\r\n\t{vendor}        1042   Yes      0\r\n\r\n\t------------------------\r\n\r\n\t{vendor} ISV server status on {host} (port 1042), up 5d 03:09:00\r\n\t{vendor} software version v16.0 (build: 2)\r\n\t{vendor} comm version: v1.2\r\n\t{vendor} Debug log filename: C:\\ProgramData\\RLM\\16.0.2.0\\{vendor}.dlog\r\n\t{vendor} Report log filename: <stdout>\r\n\tStartup time: Thu Jan  9 07:07:31 2025\r\n\tTodays Statistics (10:16:23), init time: Tue Jan 14 00:00:08 2025\r\n\tRecent Statistics (00:06:24), init time: Tue Jan 14 10:10:07 2025\r\n\r\n\t             Recent Stats         Todays Stats         Total Stats\r\n\t              00:06:24             10:16:23          5d 03:09:00\r\n\tMessages:    63 (0/sec)           4250 (0/sec)          49803 (0/sec)\r\n\tConnections: 23 (0/sec)           1904 (0/sec)          22760 (0/sec)\r\n\tCheckouts:   3 (0/sec)           63 (0/sec)          633 (0/sec)\r\n\tDenials:     3 (0/sec)           184 (0/sec)          907 (0/sec)\r\n\tRemovals:    0 (0/sec)           0 (0/sec)          0 (0/sec)\r\n\r\n\r\n\t------------------------\r\n\r\n\t";

            var featureUser =
                $"{featureName}, pool: 2\r\n\t\tcount: 1, # reservations: 0, inuse: {userCount}, exp: 30-oct-2027\r\n\t\tobsolete: 0, min_remove: 120, total checkouts: 30\r\n\t\r\n\r\n\t------------------------\r\n\r\n\t{vendor} license usage status on {host} (port 1042)\r\n\r\n\t{featureName}: {username}@{userHost} 1/0 at {featureStartTime}  (handle: 807)  \r\n";

            return new Dictionary<string, string>()
            {
                { "vendorStat", vendorStat },
                { "featureUser", featureUser }
            };
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating RLM Feature Details. \n" + ex.StackTrace);
        }
    }
}
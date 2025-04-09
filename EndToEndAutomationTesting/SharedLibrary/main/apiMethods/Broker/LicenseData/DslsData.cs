using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.Broker.LicenseData;

public class DslsData
{
    private readonly ILogging _logging;
    public DslsData(ILogging logging) => _logging = logging;

    private static string GetDateTime(string timeZoneHost, string flag)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneHost);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        if (flag == "grantedDate")
            return localTime.ToString("MM/dd/yyyy");
        return localTime.ToString("HH:mm:ss tt");
    }

    public string GenerateDslsData(Dictionary<string, string> licenseData, int userCount, bool midCall,
        string database, string table, string testId)
    {
        _logging.LogInformation("DslsData - GenerateDslsData");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());

        try
        {
            var host = licenseData["HostName"];
            var port = licenseData["Port"];
            var vendor = licenseData["VendorName"];
            var vendorData = vendor.PadRight(17);
            var username = licenseData["username"];
            var userHost = licenseData["userhost"];
            var userHostIp = licenseData["userhostip"];
            var feature = licenseData["Feature1"];
            var featureData = feature.PadRight(16);
            var timeZoneHost = licenseData["TimeZone"];

            var grantedDate = GetDateTime(timeZoneHost, "grantedDate");
            var grantedTime = GetDateTime(timeZoneHost, "grantedTime");

            var sessionTime = $"{grantedDate}, {grantedTime}";
            var sessionDateUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            if (midCall)
            {
                sessionTime = sqlTableUtilsQat.GetCellData(database, table, testId, "TEST_ID", "Feature1StartTime");
            }
            else
            {
                var querySessionTime = sqlServerQueries.UpdateQuerySql(table, "Feature1StartTime", sessionTime,
                    "TEST_ID",
                    licenseData["TEST_ID"]);
                _logging.LogInformation($"Query:: {querySessionTime}");
                sqlTableUtilsQat.RunInsertUpdateQuerySql(querySessionTime, database);

                var querySessionDateUtc = sqlServerQueries.UpdateQuerySql(table, "Feature1StartUTC", sessionDateUtc,
                    "TEST_ID", licenseData["TEST_ID"]);
                _logging.LogInformation($"Query:: {querySessionDateUtc}");
                sqlTableUtilsQat.RunInsertUpdateQuerySql(querySessionDateUtc, database);
            }

            var query = sqlServerQueries.UpdateQuerySql(table, "FeatureUpdateTimeUTC", sessionDateUtc,
                "TEST_ID", licenseData["TEST_ID"]);
            _logging.LogInformation($"Query:: {query}");
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            // var sessionData = "";
            //
            // if (userCount > 0)
            // {
            //     sessionData = $"                {userHost} (FFFFFFFFFFFFFFFF-0a3409f5.0)/{userHostIp}  "
            //                   + $"{username}          C:\\Program Files\\{vendor}"
            //                   + $"3DX\\bin\\3DEXPERIENCE.exe     6.422  200001848B1ADA21 granted since: {sessionTime}\n";
            // }

            // var data = "License Administration Tool Version 6.425.0 Built on Jun 1, 2022, 5:10:11 AM.\n"
            //            + "admin >	Software version: 6.425.0\n" + "	Build date: Jun 1, 2022, 5:10:11 AM\n"
            //            + "	Standalone mode\n" + "	Ready: yes\n" + "	Server name: " + host
            //            + "   Server id: DYP-41CA10215A9B2D81\n"
            //            + "Warning: restricted connection, some operations are not permitted.\n"
            //            + "admin >    Server configuration\n" + "        Standalone mode\n"
            //            + "        Computer name:                     " + host
            //            + "  Computer ID:                      DYP-41CA10215A9B2D81\n"
            //            + "        Licensing port:                    " + port
            //            + "           Administration port:              " + port + "\n"
            //            + "        Password protected:                no             Remote administration:            restricted\n"
            //            + "        Automatic recycling enabled:       yes            License usage statistics enabled: yes\n"
            //            + "        Enable offline license extraction: yes\n"
            //            +
            //            "admin >Status  Editor               Model          Feature                                                                      Quantity StartDate            EndDate              MaxReleaseNumber MaxReleaseDate       MaxUsageDuration MaxUsagePeriod LicenseType CommercialType LicenseId                     RepGroupIndex RepFileIndex RepFileQuantity ComputerId           ComputerName CustomerSite CustomerCountry CustomerId      GenerationDate       EnrollDate           GenCompany        GeneratorId      EditorId                             PricingStructure          AdditionalInfo               \n"
            //            + "active  " + vendorData + "    NamedUser      " + featureData
            //            + "                                                             1        2023-02-09.00:01.UTC 2024-12-30.23:59.UTC 2                2024-12-30.23:59.UTC 0                0              Floating    STD            EEP43-KLHXO-TOBIT-5HO3F-N8MRT 11            2            28              DYP-41CA10215A9B2D82 unknown      BOEING       USA             200000000001451 2023-02-10.20:10.UTC 2023-02-13.18:04.UTC "
            //            + vendorData
            //            + " 524D1394663DA865 5E756A80-1C80-478D-B83A-1D5913677621 Evaluation license key    simulia:type=usn             \n"
            //            + "admin > " + vendorData + "    (5E756A80-1C80-478D-B83A-1D5913677621)\n" + "		" + featureData
            //            + "maxReleaseNumber: 0  maxReleaseDate: 6/30/24, 4:59:00 PM expirationDate: 6/30/24, 4:59:00 PM type: NamedUser       count:   1   inuse:   1 customerId: 200000000001451 pricing structure: Evaluation license key   \n"
            //            + "			internal Id: " + username + "   granted since:  01/01/24, 12:01:38 PM  last used at: "
            //            + sessionTime + "  by user: " + username + "  on host: " + userHost
            //            + " (FFFFFFFFFFFFFFFF-0a3409f5.0)/" + userHostIp + "   \n" + sessionData + "admin >admin >";

            var data = $"License Administration Tool Version 6.425.0 Built on Jun 1, 2022, 5:10:11 AM.\nadmin >\tSoftware version: 6.425.0\n\tBuild date: Jun 1, 2022, 5:10:11 AM\n\tStandalone mode\n\tReady: yes\n\tServer name: {host}   Server id: DYP-41CA10215A9B2D81\nWarning: restricted connection, some operations are not permitted.\nadmin >    Server configuration\n        Standalone mode\n        Computer name:                     {host}  Computer ID:                      DYP-41CA10215A9B2D81\n        Licensing port:                    {port}           Administration port:              {port}\n        Password protected:                no             Remote administration:            restricted\n        Automatic recycling enabled:       yes            License usage statistics enabled: yes\n        Enable offline license extraction: yes\nadmin >Status  Editor               Model          Feature                                                                      Quantity StartDate            EndDate              MaxReleaseNumber MaxReleaseDate       MaxUsageDuration MaxUsagePeriod LicenseType CommercialType LicenseId                     RepGroupIndex RepFileIndex RepFileQuantity ComputerId           ComputerName CustomerSite CustomerCountry CustomerId      GenerationDate       EnrollDate           GenCompany        GeneratorId      EditorId                             PricingStructure          AdditionalInfo               \nactive  DSLSVendor1          NamedUser      DRTEST                                                                       1        2023-02-09.00:01.UTC 2024-12-30.23:59.UTC 2                2024-12-30.23:59.UTC 0                0              Floating    STD            EEP43-KLHXO-TOBIT-5HO3F-N8MRT 11            2            28              DYP-41CA10215A9B2D82 unknown      BOEING       USA             200000000001451 2023-02-10.20:10.UTC 2023-02-13.18:04.UTC DSLSVendor1       524D1394663DA865 5E756A80-1C80-478D-B83A-1D5913677621 Evaluation license key    simulia:type=usn             \nadmin > DSLSVendor1          (5E756A80-1C80-478D-B83A-1D5913677621)\n\t\tDRTEST          maxReleaseNumber: 0  maxReleaseDate: 13.09.2025, 01:59:00 expirationDate: 13.09.2025, 01:59:00 type: NamedUser       count:   1   inuse:   1 customerId: 200000000001451 pricing structure: Evaluation license key   \n\t\t\tinternal Id: {username}   granted since:  22.07.2024, 11:26:01  last used at: 22.07.2024, 11:35:05  by user: {username}  on host: {userHost} (FFFFFFFFFFFFFFFF-0a3409f5.0)/   \n                {userHost} (FFFFFFFFFFFFFFFF-0a3409f5.0)/  {username}          C:\\Program Files\\DSLSVendor13DX\\bin\\3DEXPERIENCE.exe     6.422  200001848B1ADA21 granted since: 22.07.2024, 11:26:01\nadmin >admin >";

            query = sqlServerQueries.UpdateQuerySql(table, "data_inquiry", data, "TEST_ID", licenseData["TEST_ID"]);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating DSLS Data \n" + ex.StackTrace);
        }
    }

    public string GenerateDslsLogData(Dictionary<string, string> licenseData)
    {
        _logging.LogInformation("DslsData - GenerateDslsLogData");

        try
        {
            var userName = licenseData["username"];
            var formattedDate = DateTime.Now.ToString("dd/MM/yyyy");

            var data = $"0:59:08 (lmgrd) TIMESTAMP {formattedDate}" +
                       $"5:45:38 (saltd) OUT: \"NX93300_features_modeling\" {userName}" +
                       $"5:51:08 (saltd) DENIED: \"NX93300_features_modeling\" {userName}" +
                       $"5:55:08 (saltd) DENIED: \"NX93300_features_modeling-nx\" {userName}  (User/host on EXCLUDE list for feature. (-38,348))";

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating DSLS Log Data. \n" + ex.Message);
        }
    }
}
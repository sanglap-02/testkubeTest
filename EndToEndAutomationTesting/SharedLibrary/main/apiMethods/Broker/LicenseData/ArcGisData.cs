using System.Globalization;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Linq;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.sqlDatabase;
using SharedLibrary.main.sqlQueries;

namespace SharedLibrary.main.apiMethods.Broker.LicenseData;

public class ArcGisData
{
    private readonly ILogging _logging;
    public ArcGisData(ILogging logging) => _logging = logging;

    private string FormatDate(string utc, string flag)
    {
        _logging.LogInformation("ArcGisData - FormatDate");

        var parsedDate = DateTimeOffset.ParseExact(utc, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);

        if (flag == "licenseSession")
            return parsedDate.ToString("ddd MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);

        if (flag == "sessionStart")
        {
            var adjustedDate = parsedDate.AddMinutes(-2);
            return adjustedDate.ToString("ddd MM/dd HH:mm", CultureInfo.InvariantCulture);
        }

        return parsedDate.AddMinutes(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }

    private static string GetFeatureValue(Dictionary<string, string> feature, string key)
    {
        return feature.TryGetValue(key, out var value) ? value : "";
    }


    public string GenerateArcGisData(Dictionary<string, string> licenseDetails, int userCount, bool midCall,
        string database, string table)
    {
        _logging.LogInformation("ArcGisData - GenerateArcGisData");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());

        try
        {
            var name = licenseDetails["ServerName"].ToLower();
            var vendor = licenseDetails["VendorName"];
            var host = licenseDetails["HostName"].ToLower();

            var feature1 = SetFeatureDetails(licenseDetails, userCount, 1, midCall, database, table);
            var feature2 = SetFeatureDetails(licenseDetails, userCount, 2, midCall, database, table);
            var feature3 = SetFeatureDetails(licenseDetails, userCount, 3, midCall, database, table);

            var feature1Usage = GetFeatureValue(feature1, "featureUser");
            var feature2Usage = GetFeatureValue(feature2, "featureUser");
            var feature3Usage = GetFeatureValue(feature3, "featureUser");

            var feature1Detail = GetFeatureValue(feature1, "featureDetails");
            var feature2Detail = GetFeatureValue(feature2, "featureDetails");
            var feature3Detail = GetFeatureValue(feature3, "featureDetails");

            var licStatusTime = sqlTableUtilsQat.GetCellData(database, table, licenseDetails["TEST_ID"], "TEST_ID",
                "ServerLocalTime");

            var data =
                $"lmutil - Copyright (c) 1989-2017 Flexera Software LLC. All Rights Reserved.\r\nFlexible License Manager status on {licStatusTime}\r\n[Detecting lmgrd processes...]\r\nLicense server status: {name}\r\n    License file(s) on {host}: C:\\OpenLM\\ArcGis\\{vendor}\\ARCGIS\\arcgis.lic:\r\n{host}: license server UP v11.14.1\r\nVendor daemon status (on {host}):\r\n  {vendor}: UP v11.14.1\r\nFeature usage info::{feature1Usage}{feature2Usage}{feature3Usage}\r\nNOTE: lmstat -i does not give information from the server,\r\n      but only reads the license file.  For this reason, \r\n      lmstat -a is recommended instead.\r\nFeature                         Version     #licenses    Vendor        Expires\r\n_______                         _________   _________    ______        ________{feature1Detail}{feature2Detail}{feature3Detail}";

            _logging.LogInformation($"Data::  {data}");

            var query = sqlServerQueries.UpdateQuerySql(table, "data_inquiry", data, "TEST_ID",
                licenseDetails["TEST_ID"]);
            sqlTableUtilsQat.RunInsertUpdateQuerySql(query, database);

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating ArcGis Data \n" + ex.StackTrace);
        }
    }

    private Dictionary<string, string> SetFeatureDetails(Dictionary<string, string> licenseDetails,
        int userCount, int featureCount, bool mid, string database, string table)
    {
        _logging.LogInformation("ArcGisData - SetFeatureDetails");

        var sqlTableUtilsQat = new SqlTableUtilsQat(new Logging());
        var sqlServerQueries = new SqlServerQueries(new Logging());

        try
        {
            var featureText = new Dictionary<string, string>();

            var vendor = licenseDetails["VendorName"];
            var host = licenseDetails["HostName"].ToLower();
            var port = licenseDetails["Port"];

            var dateTimeUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            var licenseSessionEntry = FormatDate(dateTimeUtc, "licenseSession");
            var sessionStartTime = FormatDate(dateTimeUtc, "sessionStart");
            var serverExecutionTime = dateTimeUtc;

            var feature = $"Feature{featureCount}";
            var featureName =
                sqlTableUtilsQat.GetCellData(database, table, licenseDetails["TEST_ID"], "TEST_ID", feature);
            
            if (featureName.Trim().IsNullOrEmpty()) return featureText;

            var featureUser = "";
            var featureDetails = "";
            var licUseFeature = new StringBuilder();

            for (var i = 1; i <= userCount; i++)
            {
                if (mid)
                {
                    sessionStartTime = sqlTableUtilsQat.GetCellData(database, table, licenseDetails["TEST_ID"],
                        "TEST_ID", $"{feature}StartTime");
                }
                else
                {
                    var sessionDateQuery = sqlServerQueries.UpdateQuerySql(table, $"{feature}StartTime",
                        sessionStartTime, "TEST_ID", licenseDetails["TEST_ID"]);
                    sqlTableUtilsQat.RunInsertUpdateQuerySql(sessionDateQuery, database);

                    var sessionDateUtcQuery = sqlServerQueries.UpdateQuerySql(table, $"{feature}StartUTC",
                        dateTimeUtc, "TEST_ID", licenseDetails["TEST_ID"]);
                    sqlTableUtilsQat.RunInsertUpdateQuerySql(sessionDateUtcQuery, database);
                }

                sqlTableUtilsQat.RunInsertUpdateQuerySql(sqlServerQueries.UpdateQuerySql(table, "ExecutionLocalDate",
                    serverExecutionTime, "TEST_ID", licenseDetails["TEST_ID"]), database);

                var licenseServerLocalTime = sqlServerQueries.UpdateQuerySql(table, "ServerLocalTime",
                    licenseSessionEntry, "TEST_ID", licenseDetails["TEST_ID"]);
                sqlTableUtilsQat.RunInsertUpdateQuerySql(licenseServerLocalTime, database);

                var featureUpdateTimeUtcQuery = sqlServerQueries.UpdateQuerySql(table, "FeatureUpdateTimeUTC",
                    dateTimeUtc, "TEST_ID", licenseDetails["TEST_ID"]);
                sqlTableUtilsQat.RunInsertUpdateQuerySql(featureUpdateTimeUtcQuery, database);

                var userName = licenseDetails["username"];
                var userHost = licenseDetails["userhost"].ToLower();
                var licUse =
                    $"    {userName} {userHost} {userName} (v1.0) ({host}/{port} {200 + i}), start {sessionStartTime}\r\n";
                licUseFeature.Append(licUse);
            }

            var licData = "";
            if (!licUseFeature.ToString().Equals("", StringComparison.OrdinalIgnoreCase))
            {
                licData =
                    $"  \"{featureName}\" v1.0, vendor: {vendor}, expiry: permanent(no expiration date)\r\n  floating license\r\n{licUseFeature}";
            }

            featureUser =
                $"\r\nUsers of {featureName}:  (Total of 1 licenses issued;  Total of {userCount} licenses in use)\r\n  {licData}";

            var spaceCount = 32 - featureName.Length;
            var s = new string(' ', spaceCount);
            var featureDetailsPart = $"{featureName}";
            featureDetails =
                $"\r\n{featureDetailsPart}                        1.0          1         {vendor}        permanent(no expiration date)";

            featureText.Add("featureUser", featureUser);
            featureText.Add("featureDetails", featureDetails);

            return featureText;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating ArcGis Feature Data \n" + ex.StackTrace);
        }
    }

    public string GenerateArcGisLogData(Dictionary<string, string> licenseDetails)
    {
        _logging.LogInformation("ArcGisData - GenerateArcGisLogData");

        try
        {
            var userName = licenseDetails["username"].ToLower();
            var userHost = licenseDetails["userhost"].ToLower();

            var feature = "";

            if (!licenseDetails["Feature1"].Trim().IsNullOrEmpty())
            {
                feature = "Feature1";
            }
            else
            {
                if (!licenseDetails["Feature2"].Trim().IsNullOrEmpty())
                {
                    feature = "Feature2";
                }
                else
                {
                    feature = "Feature3";
                }
            }

            var featureName = licenseDetails["feature"];
            var denialTime = licenseDetails["DenialTime"];

            var data =
                $"{denialTime} (ARCGIS) DENIED: \"{featureName}\" {userName.ToLower()}@{userHost.ToLower()} (Licensed number of users already reached. (-4,342))";

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error In Generating CodeMeter Log Data. \n" + ex.StackTrace);
        }
    }
}
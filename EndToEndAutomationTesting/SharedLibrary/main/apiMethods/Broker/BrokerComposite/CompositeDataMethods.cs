using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.apiMethods.Broker.LicenseData;

namespace SharedLibrary.main.apiMethods.Broker.BrokerComposite;

public class CompositeDataMethods
{
    private readonly ILogging _logging;
    public CompositeDataMethods(ILogging logging) => _logging = logging;

    public string GenerateCompositeData(Dictionary<string, string> licenseDetails, string sessionType,
        string database, string table, string testId)
    {
        _logging.LogInformation("CompositeDataMethods - GenerateCompositeData");

        var arcGisData = new ArcGisData(new Logging());
        var dslsData = new DslsData(new Logging());
        var openLmGeneric = new OpenLmGeneric(new Logging());
        var rlmData = new RlmData(new Logging());

        try
        {
            var data = "";
            switch (licenseDetails["Type"].ToLower())
            {
                case "flexlm":
                    switch (licenseDetails["VendorName"].ToLower())
                    {
                        case "arcgis":
                            data = sessionType.ToLower() switch
                            {
                                "start" => arcGisData.GenerateArcGisData(licenseDetails, 1, false, database, table),
                                "mid" => arcGisData.GenerateArcGisData(licenseDetails, 1, true, database, table),
                                "close" => arcGisData.GenerateArcGisData(licenseDetails, 0, false, database, table),
                                _ => data
                            };
                            break;
                    }

                    break;
                case "dsls":
                    data = sessionType.ToLower() switch
                    {
                        "start" => dslsData.GenerateDslsData(licenseDetails, 1, false, database, table, testId),
                        "mid" => dslsData.GenerateDslsData(licenseDetails, 1, true, database, table, testId),
                        "close" => dslsData.GenerateDslsData(licenseDetails, 0, false, database, table, testId),
                        _ => data
                    };
                    break;
                case "openlm generic":
                    data = sessionType.ToLower() switch
                    {
                        "start" => openLmGeneric.GenerateGenericData(licenseDetails, 1, false, database, table, testId),
                        _ => data
                    };
                    break;
                case "rlm":
                    data = sessionType.ToLower() switch
                    {
                        "start" => rlmData.GenerateRlmData(licenseDetails, 1, false, database, table, testId),
                        _ => data
                    };
                    break;
            }

            return data;
        }
        catch (Exception ex)
        {
            throw new RuntimeException("Error While Generating Composite Data. \n" + ex.StackTrace);
        }
    }

    public string GenerateLogData(Dictionary<string, string> licenseDetails)
    {
        _logging.LogInformation("CompositeDataMethods - GenerateLogData");

        var arcGisData = new ArcGisData(new Logging());
        var dslsData = new DslsData(new Logging());
        var codeMeterData = new CodeMeterData(new Logging());

        try
        {
            var data = "";

            switch (licenseDetails["Type"].ToLower())
            {
                case "flexlm":
                    switch (licenseDetails["VendorName"].ToLower())
                    {
                        case "arcgis":
                            arcGisData.GenerateArcGisLogData(licenseDetails);
                            break;
                        case "adskflex":
                            break;
                    }

                    break;
                case "codemeter":
                    data = codeMeterData.GenerateCodeMeterLogData(licenseDetails);
                    break;
                case "dsls":
                    data = dslsData.GenerateDslsLogData(licenseDetails);
                    break;
            }

            return data;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error :: ", ex);
            throw new RuntimeException("Error While Generating Log Data. \n" + ex.StackTrace);
        }
    }
}
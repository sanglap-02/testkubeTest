using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.apiMethods.JsonMethods;

public class OpenLmServerGraphQlQuery
{
    private readonly ILogging _logging;
    public OpenLmServerGraphQlQuery(ILogging logging) => _logging = logging;

    public string ApproveLicenseServerQuery(Dictionary<string, string> licenseDetails)
    {
        _logging.LogInformation("OpenLMServerGraphQlQuery - ApproveLicenseServerJson");

        var lmValues = new HashSet<string> { "FLEXlm", "DSLS", "RLM" };

        try
        {
            return $@"
                mutation AddLicenseServer {{
                  addLicenseServer(request: {{
                      name: ""{licenseDetails["ServerName"].ToLower()}"",
                      timeZone: ""{licenseDetails["TimeZone"]}"",
                      enabled: {licenseDetails["Enabled"]},
                      sampleRate: {licenseDetails["data_inquiry_TimeInterval"]},
                      isTriad: false,
                      lmType: ""{licenseDetails["Type"]}"",
                      outputFormat : ""{(lmValues.Contains(licenseDetails["Type"]) ? licenseDetails["Type"] : "OpenLM Generic")}"",
                      source: {licenseDetails["SampleMode"]},
                      halClusterName: """",
                      readLicenseFile: false,
                      vendorName: ""{licenseDetails["VendorName"]}"",
                      properties: [],
                      hosts: [
                        {{
                          hostName: ""{licenseDetails["HostName"].ToLower()}"",
                          isHttps: true,
                          isPrimary: true,
                          password: ""password"",
                          port: {licenseDetails["Port"]},
                          username: ""username""
                        }}
                      ]
                  }})
                }}";
        }
        catch (Exception ex)
        {
            _logging.LogError("Error While Creating GraphQL query to Approve License server in OpenLM Server. \n", ex);
            throw new RuntimeException(ex.Message);
        }
    }

    public string PendingLicenseServerQuery(string apiUrl)
    {
        _logging.LogInformation("OpenLMServerGraphQlQuery - PendingLicenseServerQuery");

        try
        {
            if (apiUrl.Contains("dev-k8s-cloud"))
            {
                return @"
                    query candidateServersHeaders {
                        candidateServersHeaders(first: 25, where: {status: {eq: Pending}}) {
                            totalCount
                            pageInfo {
                                hasNextPage
                                hasPreviousPage
                                startCursor
                                endCursor
                            }
                            nodes {
                                id
                                isPrimary
                                name
                                lmType
                                hostName
                                port
                                source
                            }
                        }
                    }";
            } 
            
            return @"
                query candidateServersHeaders {
                    candidateServersHeaders(first: 250) {
                        totalCount
                        pageInfo {
                            hasNextPage
                            hasPreviousPage
                            startCursor
                            endCursor
                        }
                        nodes {
                            id
                            name
                            lmType
                            hostName
                            port
                            source
                        }
                    }
                }";
        }
        catch (Exception ex)
        {
            _logging.LogError("Error while creating GraphQL query to find pending License Server.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }

    public string FindLicenseServerByIdQuery(string id)
    {
        _logging.LogInformation("OpenLMServerGraphQlQuery - FindLicenseServerByIdQuery");

        try
        {
            var query = $@"
                    query licenseServerConfigById {{
                        licenseServerConfigById(licenseServerId: {id}) {{
                            id,
                            name,
                            lmType,
                            hostName,
                            port,
                            source,
                            allowFallbackToDeniedLicense,   
                            enabled,
                            readLicenseFile,
                            timeZone,
                            sampleRate,
                            sampleMode,
                            halClusterName,
                            vendorName,
                            isTriad,
                            hosts {{
                              id,
                              hostName,
                              username,
                              password,
                              isHttps,
                              port,
                              isPrimary
                            }}
                        }}
                    }}";

            return query;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error while creating GraphQL query to find License Server by Id.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }

    public string UpdateLicenseServerQuery(string enabled, Dictionary<string, string> licenseDetails)
    {
        _logging.LogInformation("OpenLMServerGraphQlQuery - UpdateLicenseServerQuery");

        try
        {
            var query = $@"
            mutation updateLicenseServer {{
              updateLicenseServer(request: {{
                id: {licenseDetails["licenseServerId"]},
                name: ""{licenseDetails["ServerName"].ToLower()}"",
                readLicenseFile: true,
                allowFallbackToDeniedLicense: false,
                enabled: {enabled},
                isTriad: false,
                properties: [],
                hosts: [{{
                  id: {licenseDetails["licenseServerId"]},
                  hostName: ""{licenseDetails["HostName"].ToLower()}"",
                  port: {licenseDetails["Port"]},
                  isHttps: false,
                  isPrimary: false
                }}]
              }})
            }}";

            return query;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error while creating GraphQL query to Update License Server.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }

    public string DeleteLicenseServerQuery(string licenseServerId)
    {
        _logging.LogInformation("OpenLMServerGraphQlQuery - DeleteLicenseServerQuery");

        try
        {
            var query = $@"
                mutation deleteLicenseServers {{
                    deleteLicenseServers(request: {{ 
                        ids: {licenseServerId}
                    }})
                }}";

            return query;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error while creating GraphQL query to Delete License Server.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }

    public string GetServerStatisticsQuery(string serverName)
    {
        _logging.LogInformation("OpenLMServerGraphQlQuery - GetServerStatisticsQuery");

        try
        {
            var query = $@"
            query serversStatistics {{
                serversStatistics(first: 25, where: {{name: {{in: [""{serverName}""]}}}}) {{
                    nodes {{
                        id
                        name
                        lmType
                        hostName
                        port
                        totalQuantity
                        totalConsumed
                        totalBorrowed
                        usagePercent
                        totalAllocated
                    }}
                }}
            }}";
            return query;
        }
        catch (Exception ex)
        {
            _logging.LogError("Error while creating GraphQL query to get License Server Statistics.", ex);
            throw new RuntimeException(ex.StackTrace);
        }
    }
}
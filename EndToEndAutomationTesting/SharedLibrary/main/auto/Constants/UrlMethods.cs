using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.Constants
{
    public class UrlMethods
    {
        private readonly ILogging _logging;
        public UrlMethods(ILogging logging) => _logging = logging;

        public string GetUrl(Dictionary<string, string> urlDetails, string product)
        {
            _logging.LogInformation("UrlMethods - GetUrl");

            try
            {
                var baseUrl = urlDetails["BASE_URL"];
                var port = "";
                var protocol = "";
                var url = "";

                switch (product)
                {
                    case "brokerhubapi":
                        port = urlDetails["BROKERHUBAPI_PORT"];
                        protocol = urlDetails["BROKERHUBAPI_PROTOCOL"];
                        try
                        {
                            var intPort = int.Parse(port);
                            url = protocol + "://" + baseUrl + ":" + intPort + "/";
                        }
                        catch (FormatException)
                        {
                            url = protocol + "://" + baseUrl + "/" + port + "/";
                        }

                        break;
                    case "identity":
                        port = urlDetails["IDENTITY_PORT"];
                        protocol = urlDetails["IDENTITY_PROTOCOL"];
                        try
                        {
                            var intPort = int.Parse(port);
                            url = protocol + "://" + baseUrl + ":" + intPort + "/";
                        }
                        catch (FormatException)
                        {
                            url = protocol + "://" + baseUrl + "/" + port + "/";
                        }

                        break;
                    case "easyadminApi":
                        port = urlDetails["EASYADMIN_API_PORT"];
                        protocol = urlDetails["EASYADMIN_API_PROTOCOL"];
                        try
                        {
                            var intPort = int.Parse(port);
                            url = protocol + "://" + baseUrl + ":" + intPort + "/";
                        }
                        catch (FormatException)
                        {
                            url = protocol + "://" + baseUrl + "/" + port + "/";
                        }

                        break;
                    case "operationalApi":
                        port = urlDetails["OPERATIONAL_PORT"];
                        protocol = urlDetails["OPERATIONAL_PROTOCOL"];
                        try
                        {
                            var intPort = int.Parse(port);
                            url = protocol + "://" + baseUrl + ":" + intPort + "/";
                        }
                        catch (FormatException)
                        {
                            url = protocol + "://" + baseUrl + "/" + port + "/";
                        }

                        break;
                    case "cloudbroker":
                        port = urlDetails["CLOUDBROKER_PORT"];
                        protocol = urlDetails["CLOUDBROKER_PROTOCOL"];
                        try
                        {
                            var intPort = int.Parse(port);
                            url = protocol + "://" + baseUrl + ":" + intPort + "/";
                        }
                        catch (FormatException)
                        {
                            url = protocol + "://" + baseUrl + "/" + port + "/";
                        }

                        break;
                    case "agentProcessCycle":
                        port = urlDetails["AGENT_PORT"];
                        protocol = urlDetails["AGENT_PROTOCOL"];
                        try
                        {
                            url = protocol + "://" + baseUrl + ":" + Int64.Parse(port) + "/";
                        }
                        catch (Exception e)
                        {
                            url = protocol + "://" + baseUrl + "/" + Int64.Parse(port) + "/";
                        }

                        break;
                    case "ugs":
                        port = urlDetails["USERS_GROUPS_PORT"];
                        protocol = urlDetails["USERS_GROUPS_PROTOCOL"];
                        try
                        {
                            var intPort = int.Parse(port);
                            url = protocol + "://" + baseUrl + ":" + intPort + "/";
                        }
                        catch (FormatException)
                        {
                            url = protocol + "://" + baseUrl + "/" + port + "/";
                        }

                        break;
                    default:
                        break;
                }

                return url;
            }
            catch (Exception e)
            {
                _logging.LogError("Error", e);
                throw new RuntimeException(e);
            }
        }
    }
}
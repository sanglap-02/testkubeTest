using System.Text.Json;
using NPOI.Util;
using Net.Extensions;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;
using SharedLibrary.main.auto.framework.apicalls;
using SharedLibrary.main.auto.framework.constants;
using SharedLibrary.main.auto.Constants;

namespace SharedLibrary.main.auto.framework.token
{
    public class AccessTokenMethods
    {
        private readonly ILogging _logging;
        public AccessTokenMethods(ILogging logging) => _logging = logging;

        private string ExtractTokenAccessValue(string accessValue)
        {
            _logging.LogInformation("AccessTokenMethods - ExtractTokenAccessValue");

            using (var doc = JsonDocument.Parse(accessValue))
            {
                return doc.RootElement.GetProperty("access_token").GetString() ?? "";
            }
        }
        
        public async Task<string> GetTokenFromBaseUrl(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("AccessTokenMethods - GetTokenFromBaseUrl");

            var urlMethods = new UrlMethods(new Logging());
            var tokenEnabled = securityDetails["AuthEnabled"];

            try
            {
                if (string.Equals(tokenEnabled, "No", StringComparison.OrdinalIgnoreCase)) return "Bearer " + "null";

                var securityBaseUrl = urlMethods.GetUrl(securityDetails, "identity");
                var tokenApiUrl = securityBaseUrl + UrlConstants.TokenApi;
                _logging.LogInformation($"API Token - Token API URL to be use :: {tokenApiUrl}");
                securityDetails.Add("ServerTokenURL", tokenApiUrl);
                var token = await GetAuthTokenFromUserPwd(securityDetails);
                
                if(token.Contains("{\"error\":\"invalid_client\"}")) 
                    return ExtractTokenAccessValue(token);
                
                return ExtractTokenAccessValue(token);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Setting Token. \n");
            }
        }
        
        private async Task<string> GetAuthTokenFromUserPwd(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("AccessTokenMethods - GetAuthTokenFromUserPwd");

            var restApiCall = new RestApiCalls(new Logging());
            
            try
            {
                return await restApiCall.GetAuthTokenFromUserPwd(securityDetails);
            }
            catch (Exception e)
            {
                _logging.LogError("Error :: ", e);
                throw new ApplicationException("Error While Getting Auth Token.");
            }
        }

        public async Task<string> GetBrokerHubTokenFromApi(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("AccessTokenMethods - GetBrokerHubTokenFromApi");

            var urlMethods = new UrlMethods(new Logging());
            var tokenEnabled = securityDetails["AuthEnabled"];

            try
            {
                if (string.Equals(tokenEnabled, "NO", StringComparison.OrdinalIgnoreCase)) return "Bearer " + "null";

                var securityBaseUrl = urlMethods.GetUrl(securityDetails, "identity");
                var tokenApiUrl = securityBaseUrl + UrlConstants.TokenApi;
                _logging.LogInformation($"API Token - Token API URL to be use :: {tokenApiUrl}");
                securityDetails.Add("BrokerHubURL", tokenApiUrl);
                var token = await GetBrokerHubApiAuthToken(securityDetails);
                
                if (token.Contains("invalid_client"))
                {
                    return token;
                }

                return ExtractTokenAccessValue(token);
            }
            catch (Exception e)
            {
                _logging.LogError("Error :: ", e);
                throw new RuntimeException("Error While Setting Token.");
            }
        }

        private async Task<string> GetBrokerHubApiAuthToken(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("AccessTokenMethods - GetBrokerHubApiAuthToken");
         
            var restApiCall = new RestApiCalls(new Logging());
            
            try
            {
                var securityDetailsBroker = new Dictionary<string, string>()
                {
                    { "grant_type", "client_credentials" },
                    { "scope", "openlm.brokerhub.scope" },
                    { "client_id", securityDetails["client_id_broker_hub"] },
                    { "client_secret", securityDetails["client_secret_broker_hub"] },
                    { "BrokerHubURL", securityDetails["BrokerHubURL"] },
                };
                
                return await restApiCall.GetApiAuthTokenFromClientCredentialsBrokerHub(securityDetailsBroker);
            }
            catch (Exception e)
            {
                _logging.LogError("Error :: ",e);
                throw new RuntimeException("Error While Getting Auth Token.");
            }
        }
        
        public async Task<string> GetAuthTokenFromOperationalApi(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("RestApiCalls - GetAuthTokenFromOperationalApi");

            var urlMethods = new UrlMethods(new Logging());
            var tokenEnabled = securityDetails["AuthEnabled"];
            
            try
            {
                if (string.Equals(tokenEnabled, "NO", StringComparison.OrdinalIgnoreCase)) return "Bearer " + "null";

                var securityBaseUrl = urlMethods.GetUrl(securityDetails, "identity");
                var tokenApiUrl = "";
                if (securityBaseUrl.Contains("k8s-cloud"))
                {
                    tokenApiUrl = securityBaseUrl + UrlConstants.TokenApiOperationalCloud + securityDetails["tenantId"];
                }
                else
                {
                    tokenApiUrl = securityBaseUrl + UrlConstants.TokenApi;
                }
                _logging.LogInformation($"API Token - Token API URL to be use :: {tokenApiUrl}");
                securityDetails.Add("OperationalApi", tokenApiUrl);
                var token = await GetOperationalApi(securityDetails);

                if (token.Contains("invalid_client"))
                {
                    return token;
                }
                
                return ExtractTokenAccessValue(token);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Getting Auth Token from Operational Api.");
            }
        }

        private async Task<string> GetOperationalApi(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("RestApiCalls - GetOperationalApi");

            try
            {
                var restApiCall = new RestApiCalls(new Logging());

                var securityDetailsOperationalApi = new Dictionary<string, string>()
                {
                    { "grant_type", securityDetails["grant_type_operational"]},
                    { "client_id", securityDetails["client_id_operational"] },
                    { "client_secret", securityDetails["client_secret_operational"] },
                    { "OperationalApi", securityDetails["OperationalApi"] },
                };
                
                return await restApiCall.GetAuthFromOperationalApi(securityDetailsOperationalApi);
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw new RuntimeException("Error While Getting Auth Token from Operational Api.");
            }
        }

        public async Task<string> GetAgentHubTokenFromApi(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("AccessTokenMethods - GetAgentHubTokenFromApi");

            var urlMethods = new UrlMethods(new Logging());
            var accessTokenMethods = new AccessTokenMethods(new Logging());
            var tokenEnabled = securityDetails["AuthEnabled"];

            try
            {
                if (string.Equals(tokenEnabled, "No", StringComparison.OrdinalIgnoreCase)) return "Bearer " + "null";

                var securityBaseUrl = urlMethods.GetUrl(securityDetails, "identity");
                var tokenApiUrl = securityBaseUrl + UrlConstants.TokenApi;
                _logging.LogInformation("API Token - Token API URL to be use :: {tokenApiUrl}");
                securityDetails.Add("AgentHubUrl", tokenApiUrl);
                var token = await accessTokenMethods.GetAgentHubApiAuthToken(securityDetails);

                if(token.Contains("{\"error\":\"invalid_client\"}")) 
                    return ExtractTokenAccessValue(token);
                
                return "Bearer " + token;
            }
            catch (Exception e)
            {
                _logging.LogError("Error While Setting Agent Token.", e);
                throw new RuntimeException(e.Message);
            }
        }

        private async Task<string> GetAgentHubApiAuthToken(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("AccessTokenMethods - GetAgentHubApiAuthToken");

            var restApiCall = new RestApiCalls(new Logging());
            
            try
            {
                if (securityDetails.IsEmpty()) return "";

                var securityDetailsTemp = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "scope", "openlm.agentshub.scope" },
                    { "client_id", securityDetails["client_id_agent_hub"] },
                    { "client_secret", securityDetails["client_secret_agent_hub"] },
                    { "AgentHubUrl", securityDetails["AgentHubUrl"] }
                };

                return await restApiCall.GetAuthTokenFromCredentialsAgentHub(securityDetailsTemp);
            }
            catch (Exception e)
            {
                _logging.LogError("Error While Getting Auth Token.", e);
                throw new RuntimeException(e.Message);
            }
        }
    }
}
using Newtonsoft.Json.Linq;
using NPOI.Util;
using System.Net.Http.Headers;
using System.Text;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.framework.apicalls
{
    public class RestApiCalls
    {
        private readonly ILogging _logging;
        private static readonly HttpClient Client = new HttpClient();

        public RestApiCalls(ILogging logging) => _logging = logging;

        public async Task<string> GetAuthTokenFromUserPwd(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("RestApiCalls - GetAuthTokenFromUserPwd");

            if (!securityDetails.ContainsKey("ServerTokenURL") ||
                string.IsNullOrWhiteSpace(securityDetails["ServerTokenURL"]))
            {
                throw new ArgumentException("URL must be provided in security details.");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", securityDetails["grant_type"]),
                        new KeyValuePair<string, string>("scope", securityDetails["scope"]),
                        new KeyValuePair<string, string>("client_id", securityDetails["client_id"]),
                        new KeyValuePair<string, string>("client_secret", securityDetails["client_secret"]),
                        new KeyValuePair<string, string>("username", securityDetails["username"]),
                        new KeyValuePair<string, string>("password", securityDetails["password"])
                    });

                    // Send a POST request to the specified URL
                    var response = await client.PostAsync(securityDetails["ServerTokenURL"], content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string and return it
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Response: {responseContent}");
                        return responseContent;
                    }
                    else
                    {
                        // Log and handle non-success status codes appropriately
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logging.LogInformation(
                            $"Failed to retrieve token. Status Code: {response.StatusCode}. Response: {errorContent}");

                        var jsonObject = JObject.Parse(errorContent);
                        var errorValue = jsonObject["error"].ToString();
                        return errorValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw;
            }
        }

        public async Task<string> GetApiAuthTokenFromClientCredentialsBrokerHub(
            Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("RestApiCalls - GetApiAuthTokenFromClientCredentialsBrokerHub");

            if (!securityDetails.ContainsKey("BrokerHubURL") ||
                string.IsNullOrWhiteSpace(securityDetails["BrokerHubURL"]))
            {
                throw new ArgumentException("URL must be provided in security details.");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    // Set up the request content with form parameters
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", securityDetails["grant_type"]),
                        new KeyValuePair<string, string>("scope", securityDetails["scope"]),
                        new KeyValuePair<string, string>("client_id", securityDetails["client_id"]),
                        new KeyValuePair<string, string>("client_secret", securityDetails["client_secret"]),
                    });

                    // Send a POST request to the specified URL
                    var response = await client.PostAsync(securityDetails["BrokerHubURL"], content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string and return it
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logging.LogInformation($"Response: {responseContent}");
                        return responseContent;
                    }
                    else
                    {
                        // Log and handle non-success status codes appropriately
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logging.LogInformation(
                            $"Failed to retrieve token. Status Code: {response.StatusCode}. Response: {errorContent}");

                        var jsonObject = JObject.Parse(errorContent);
                        var errorValue = jsonObject["error"].ToString();
                        return errorValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw;
            }
        }

        public async Task<string> GetAuthTokenFromCredentialsAgentHub(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("RestApiCalls - GetAuthTokenFromCredentialsAgentHub");

            if (!securityDetails.ContainsKey("AgentHubUrl") ||
                string.IsNullOrWhiteSpace(securityDetails["AgentHubUrl"]))
            {
                throw new ArgumentException("URL must be provided in security details.");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("grant_type", securityDetails["grant_type"]),
                            new KeyValuePair<string, string>("scope", securityDetails["scope"]),
                            new KeyValuePair<string, string>("client_id", securityDetails["client_id"]),
                            new KeyValuePair<string, string>("client_secret",
                                securityDetails["client_secret"]),
                        }
                    );

                    // Send a POST request to the specified URL
                    var response = await client.PostAsync(securityDetails["AgentHubUrl"], content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string and return it
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logging.LogInformation($"Response: {responseContent}");

                        var jsonResponse = JObject.Parse(responseContent);

                        return jsonResponse["access_token"]?.ToString();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logging.LogInformation(
                            $"Failed to retrieve token. Status Code: {response.StatusCode}. Response: {errorContent}");

                        var jsonObject = JObject.Parse(errorContent);
                        var errorValue = jsonObject["error"].ToString();
                        return errorValue;
                    }
                }
            }
            catch (Exception e)
            {
                _logging.LogError("Error :: ", e);
                throw new RuntimeException();
            }
        }

        public async Task<string> GetAuthFromOperationalApi(Dictionary<string, string> securityDetails)
        {
            _logging.LogInformation("RestApiCalls - GetAuthFromOperationalApi");

            if (!securityDetails.ContainsKey("OperationalApi") ||
                string.IsNullOrWhiteSpace(securityDetails["OperationalApi"]))
            {
                throw new ArgumentException("URL must be provided in security details.");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", securityDetails["grant_type"]),
                        new KeyValuePair<string, string>("client_id", securityDetails["client_id"]),
                        new KeyValuePair<string, string>("client_secret", securityDetails["client_secret"]),
                    });

                    var response = await client.PostAsync(securityDetails["OperationalApi"], content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logging.LogInformation($"Response: {responseContent}");
                        return responseContent;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logging.LogInformation(
                            $"Failed to retrieve token. Status Code: {response.StatusCode}. Response: {errorContent}");

                        var jsonObject = JObject.Parse(errorContent);
                        var errorValue = jsonObject["error"].ToString();
                        return errorValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logging.LogError("Error :: ", ex);
                throw;
            }
        }

        public HttpResponseMessage RestGetCall(string url, string authToken)
        {
            _logging.LogInformation("RestApiCall - RestGetCall");

            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(authToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                }

                response = client.GetAsync(url).Result;
            }

            _logging.LogInformation($"restGetCall: {response.Content.ReadAsStringAsync().Result}");

            return response;
        }

        public async Task<HttpResponseMessage> RestPostCall(string url, string payload, string authToken)
        {
            _logging.LogInformation("RestApiCalls - RestPostCall");
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrEmpty(authToken))
                {
                    requestMessage.Headers.Add("Authorization", authToken);
                }

                var response = await Client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error ::", ex);
                throw new Exception("Error while making API call.");
            }
        }

        public async Task<HttpResponseMessage> RestPatchCall(string url, string payload, string authToken)
        {
            _logging.LogInformation("RestApiCalls - RestPatchCall");
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Patch, url)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrEmpty(authToken))
                {
                    requestMessage.Headers.Add("Authorization", authToken);
                }

                var response = await Client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (Exception ex)
            {
                _logging.LogError("Error ::", ex);
                throw new Exception("Error while making PATCH API call.");
            }
        }
    }
}
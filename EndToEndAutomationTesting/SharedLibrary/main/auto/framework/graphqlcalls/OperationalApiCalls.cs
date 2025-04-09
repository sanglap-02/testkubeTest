using System.Text;
using Newtonsoft.Json.Linq;
using NPOI.Util;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.framework.graphqlcalls;

public class OperationalApiCalls
{
    private readonly ILogging _logging;
    public OperationalApiCalls(ILogging logging) => _logging = logging;
    
    private static readonly HttpClient Client = new HttpClient();
    
    public async Task<JObject> GQlPostCall(string url, string query, string authToken)
    {
        _logging.LogInformation("RestApiCalls - GQlPostCall");

        try
        {
            var requestBody = new
            {
                query = query
            };

            var content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            if (!string.IsNullOrEmpty(authToken))
            {
                Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }

            var response = await Client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            
            return JObject.Parse(responseBody);
        }
        catch (Exception e)
        {
            _logging.LogError("Not able to execute GraphQL query.", e);
            throw new RuntimeException(e.StackTrace);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiDesigner.Shared.Data
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(string baseUrl)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            // Add headers if necessary, like authentication headers
        }

        public async Task<Dictionary<string, object>> ExecuteProgramAsync(Guid programKey, Dictionary<Guid, object> inputValues, Guid apiKey, Guid? sessionId = null, bool isTest = false)
        {
            // Create request payload
            var requestContent = new StringContent(JsonConvert.SerializeObject(inputValues), Encoding.UTF8, "application/json");

            // Construct URL with query parameters
            var url = $"https://localhost:44313/api/Program/execute?programKey={programKey}&apiKey={apiKey}";

            if (sessionId.HasValue)
            {
                url += $"&sessionId={sessionId.Value}";
            }

            if (isTest)
            {
                url += $"&isTest=true";
            }

            // Make the API call
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();  // Throw an exception if the response is not successful.

            // Parse and return the response
            var responseContent = await response.Content.ReadAsStringAsync();
            var outputValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

            return outputValues;
        }
    }

}

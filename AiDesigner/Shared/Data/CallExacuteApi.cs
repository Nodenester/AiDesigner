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

        public async Task<List<object>> ExecuteProgramAsync(Guid programKey, Dictionary<Guid, object> inputValues, string apiKey, Guid? sessionId = null, bool isTest = false)
        {
            // Create request payload
            var requestContent = new StringContent(JsonConvert.SerializeObject(inputValues), Encoding.UTF8, "application/json");

            // Construct URL with query parameters
            var url = $"api/Program/execute?programKey={programKey}&apiKey={apiKey}";

            if (sessionId.HasValue)
            {
                url += $"&sessionId={sessionId.Value}";
            }

            if (isTest)
            {
                url += $"&isTest=true";
            }

            // Make the API call
            var response = new HttpResponseMessage();
            try
            {
                response = await _httpClient.PostAsync(url, requestContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Consider logging the exception for further analysis if needed.
            }

            response.EnsureSuccessStatusCode();  // Throw an exception if the response is not successful.

            // Parse and return the response
            var responseContent = await response.Content.ReadAsStringAsync();
            var outputValues = JsonConvert.DeserializeObject<List<object>>(responseContent);

            return outputValues;
        }
    }

}

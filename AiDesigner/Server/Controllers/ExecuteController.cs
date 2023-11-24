using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Newtonsoft.Json;

namespace AiDesigner.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ExecuteController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ExecuteController()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:44313/") };// You can configure the HttpClient instance here if needed.
        }

        [HttpPost("")]
        public async Task<ActionResult<string>> ExecuteProgramAsync(
            [FromQuery] Guid programKey,
            [FromBody] Dictionary<Guid, object> inputValues,
            [FromQuery] string apiKey,
            [FromQuery] Guid? sessionId = null,
            [FromQuery] bool isTest = false)
            {
            try
            {
                // Create request payload
                var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(inputValues), Encoding.UTF8, "application/json");
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
                    return Ok(response);
                }

                response.EnsureSuccessStatusCode(); 

                // Parse and return the response
                var responseContent = await response.Content.ReadAsStringAsync();
                var outputValues = JsonConvert.DeserializeObject<List<object>>(responseContent);

                return Content(responseContent, "application/json");
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

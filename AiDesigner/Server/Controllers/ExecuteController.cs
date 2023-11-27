using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Newtonsoft.Json;
using OtpNet;

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
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        [HttpPost("")]
        public async Task<ActionResult<string>> ExecuteProgramAsync(
            [FromQuery] Guid programKey,
            [FromBody] Dictionary<Guid, object> inputValues,
            [FromQuery] string apiKey,
            [FromQuery] Guid? sessionId = null,
            [FromQuery] bool isTest = false,            
            [FromQuery] bool isCustomBlock = false)
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

                if (isCustomBlock)
                {
                    url += $"&isCustomBlock=true";
                }

                url += $"&testToken={GenerateTOTP()}";

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
        public string GenerateTOTP()
        {
            //byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
            string secretKeyBase64 = "YOUR_TOTP_SECRET_HERE";
            byte[] secretKey = Convert.FromBase64String(secretKeyBase64);

            var totp = new Totp(secretKey);

            // Generate a TOTP token
            return totp.ComputeTotp();
        }
    }
}

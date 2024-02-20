using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Newtonsoft.Json;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.nodenestor.com/") };
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
            HttpResponseMessage response = new HttpResponseMessage(); // Declare outside of try block
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
                response = await _httpClient.PostAsync(url, requestContent);
                response.EnsureSuccessStatusCode();

                // Parse and return the response
                var responseContent = await response.Content.ReadAsStringAsync();
                var outputValues = JsonConvert.DeserializeObject<List<object>>(responseContent);

                return Content(responseContent, "application/json");
            }
            catch (Exception ex)
            {
                var errorDetails = new StringBuilder();
                errorDetails.AppendLine($"Message: {ex.Message}");

                if (ex.InnerException != null)
                {
                    errorDetails.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                }

                errorDetails.AppendLine($"StackTrace: {ex.StackTrace}");

                string partialData = null;
                if (response?.Content != null)
                {
                    try
                    {
                        partialData = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(partialData))
                        {
                            errorDetails.AppendLine($"Partial Data: {partialData}");
                        }
                    }
                    catch (Exception readEx)
                    {
                        errorDetails.AppendLine($"Failed to read partial data: {readEx.Message}");
                    }
                }

                var errorInfo = new
                {
                    Message = "Failed to execute program.",
                    Details = errorDetails.ToString()
                };

                return StatusCode(500, JsonConvert.SerializeObject(errorInfo));
            }
        }

        public string GenerateTOTP()
        {
            string secretKeyBase64 = "YOUR_TOTP_SECRET_HERE";
            byte[] secretKey = Convert.FromBase64String(secretKeyBase64);
            var totp = new Totp(secretKey);
            return totp.ComputeTotp();
        }
    }
}

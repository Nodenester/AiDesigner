using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AiDesigner.Server.Data;
using AiDesigner.Shared.Blocks;
using AiDesigner.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodeBaseApi.Version2;

namespace NodeBaseApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly DBConnection _dbConnection;

        public ChatController(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter> { new TupleConverter(), new BlockJsonConverter() },
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        // GET api/Chat/Workshop/{userId}
        [HttpGet("Workshop/{userId}")]
        public async Task<ActionResult<string>> GetUserWorkshopPrograms(string userId)
        {
            try
            {
                IEnumerable<CustomProgram> userPrograms = await _dbConnection.GetAllProgramsFromUserWorkshopArticlesAsync(userId);

                var serializedUserPrograms = JsonConvert.SerializeObject(userPrograms, Formatting.Indented, settings);

                return Ok(serializedUserPrograms);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine(ex);
                // Return a 500 status code
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Chat/Programs/{userId}
        [HttpGet("Programs/{userId}")]
        public async Task<ActionResult<string>> GetUserPrograms(Guid userId)
        {
            try
            {
                IEnumerable<CustomProgram> userPrograms = await _dbConnection.GetAllUserCustomProgramsAsync(userId);

                var serializedUserPrograms = JsonConvert.SerializeObject(userPrograms, Formatting.Indented, settings);

                return Ok(serializedUserPrograms);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine(ex);
                // Return a 500 status code
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Chat/News/Latest
        [HttpGet("News/Latest")]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetLatestNews()
        {
            try
            {
                IEnumerable<NewsArticle> latestArticles = await _dbConnection.GetLatestArticlesAsync();

                return Ok(latestArticles);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine(ex);
                // Return a 500 status code
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // POST api/Chat/News/Create
        [HttpPost("News/Create")]
        public async Task<ActionResult> CreateNews([FromBody] NewsArticle article)
        {
            try
            {
                await _dbConnection.CreateArticleAsync(article.Id, article.Title, article.Content, article.ImageData);

                return Ok("Article created successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine(ex);
                // Return a 500 status code
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        //Token stuff
        // POST api/Chat/GetCreateWallet
        [HttpPost("Wallet/GetCreateWallet")]
        public async Task<ActionResult> GetCreateWallet([FromBody] Guid userId)
        {
            try
            {
                var wallet = await _dbConnection.EnsureWalletAndRetrieveTokensAsync(userId);
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                // Log and handle the exception
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        //Call stuff
        // POST api/Chat/Call/GetCreateCall
        [HttpGet("Call/GetCalls/{Key}")]
        public async Task<ActionResult<IEnumerable<Call>>> GetCalls([FromBody] string Key)
        {
            try
            {
                var calls = await _dbConnection.GetApiCallsAsync(Key);
                return Ok(calls);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine(ex);
                // Return a 500 status code
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Chat/Call/GetAggregatedData/{userId}/{timeFrame}/{programId}
        [HttpGet("Call/GetAggregatedData/{userId}/{timeFrame}/{programId}")]
        public async Task<ActionResult<IEnumerable<AggregatedData>>> GetAggregatedData(string userId, string timeFrame, string programId)
        {
            try
            {
                var aggregatedData = await _dbConnection.GetAggregatedDataAsync(userId, timeFrame, programId);
                return Ok(aggregatedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        [HttpGet("Call/GetAggregatedDataForAdminAsync/{timeFrame}")]
        public async Task<ActionResult<IEnumerable<AggregatedData>>> GetAggregatedData(string timeFrame)
        {
            try
            {
                var aggregatedData = await _dbConnection.GetAggregatedDataForAdminAsync(timeFrame);
                return Ok(aggregatedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }


        // GET api/Chat/Call/GetByUserId/{userId}
        [HttpGet("Call/GetByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<Call>>> GetLatest100Calls(string userId)
        {
            try
            {
                var calls = await _dbConnection.GetLatest100CallsByUserIdAsync(userId);
                return Ok(calls);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Chat/Call/Getlatest/{numCalls}
        [HttpGet("Call/Getlatest/{numCalls}")]
        public async Task<ActionResult<IEnumerable<Call>>> GetLatestCalls(int numCalls)
        {
            try
            {
                var calls = await _dbConnection.GetLatestCallsAsync(numCalls);
                return Ok(calls);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        //ApiKeyStuff
        // POST api/Chat/ApiKey/Create
        [HttpPost("ApiKey/Create")]
        public async Task<ActionResult> Create([FromBody] ApiKey request)
        {
            try
            {
                var affectedRows = await _dbConnection.AddApiKeyAsync(request.apiKey, request.UserId, DateTime.UtcNow, request.Name);
                if (affectedRows > 0)
                    return Ok("API Key created successfully.");
                else
                    return BadRequest("Failed to create API key.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Chat/ApiKey/GetByUserId/{userId}
        [HttpGet("ApiKey/GetByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<ApiKey>>> GetByUserId(Guid userId)
        {
            try
            {
                var apiKeys = await _dbConnection.GetApiKeysByUserIdAsync(userId);
                return Ok(apiKeys);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // DELETE Chat/ApiKey/Delete/{id}
        [HttpDelete("ApiKey/Delete/{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var affectedRows = await _dbConnection.DeleteApiKeyAsync(id);
                if (affectedRows > 0)
                    return Ok("API Key deleted successfully.");
                else
                    return BadRequest("Failed to delete API key.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        //Session stuff
        // POST api/Session/Create
        [HttpPost("Session/Create")]
        public async Task<ActionResult> CreateSession([FromBody] Session request)
        {
            try
            {
                var sessionId = await _dbConnection.CreateSessionAsync(request);
                if (sessionId != Guid.Empty)
                    return Ok(new { message = "Session created successfully.", sessionId = sessionId });
                else
                    return BadRequest("Failed to create session.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Session/GetUserSessions
        [HttpGet("Session/GetUserSessions")]
        public async Task<ActionResult<IEnumerable<Session>>> GetUserSessions([FromQuery] Guid userId, [FromQuery] Guid? programId = null)
        {
            try
            {
                var sessions = await _dbConnection.GetSessionsAsync(userId, programId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // DELETE api/Session/Delete/{sessionId}
        [HttpDelete("Session/Delete/{sessionId}")]
        public async Task<ActionResult> DeleteSession(string sessionId)
        {
            try
            {
                var affectedRows = await _dbConnection.DeleteSessionAsync(sessionId);
                if (affectedRows > 0)
                    return Ok("Session deleted successfully.");
                else
                    return BadRequest("Failed to delete session.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }



    }
}

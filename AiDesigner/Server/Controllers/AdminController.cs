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

namespace AiDesigner.Server.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DBConnection _dbConnection;

        public AdminController(DBConnection dbConnection)
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
    }
}

using System;
using System.Collections.Generic;
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


    }
}

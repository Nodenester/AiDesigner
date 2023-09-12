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
    public class CustomBlocksController : ControllerBase
    {
        private readonly DBConnection _dbConnection;

        public CustomBlocksController(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // POST api/customblocks
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCustomBlock([FromBody] JsonElement body)
        {
            var settings = new JsonSerializerSettings
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

            var customBlock = JsonConvert.DeserializeObject<CustomBlockProgram>(body.GetRawText(), settings);

            if (customBlock == null)
            {
                return BadRequest();
            }

            if (customBlock.ProgramStructure == null)
            {
                customBlock.ProgramStructure = new ProgramStructure();
            }

            try
            {
                var customBlockId = await _dbConnection.SaveCustomBlockAsync(customBlock);
                return customBlockId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/customblocks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomBlock(Guid id, [FromBody] JsonElement body)
        {
            var settings = new JsonSerializerSettings
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

            var customBlock = JsonConvert.DeserializeObject<CustomBlockProgram>(body.GetRawText(), settings);

            if (id != customBlock.Id)
            {
                return BadRequest();
            }

            if (customBlock.ProgramStructure == null)
            {
                customBlock.ProgramStructure = new ProgramStructure();
            }

            await _dbConnection.UpdateCustomBlockAsync(customBlock);
            return NoContent();
        }

        // GET api/customblocks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomBlockProgram>> GetCustomBlock(Guid id)
        {
            CustomBlockProgram customBlock = await _dbConnection.LoadCustomBlockAsync(id);

            if (customBlock == null)
            {
                return NotFound();
            }

            return customBlock;
        }

        // DELETE api/customblocks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomBlock(Guid id)
        {
            await _dbConnection.DeleteCustomBlockAsync(id);
            return NoContent();
        }

        // GET api/customblocks/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<string>> GetUserCustomBlocks(Guid userId)
        {
            try
            {
                IEnumerable<CustomBlockProgram> userCustomBlocks = await _dbConnection.GetAllUserCustomBlocksAsync(userId);

                var settings = new JsonSerializerSettings
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

                var serializedUserCustomBlocks = JsonConvert.SerializeObject(userCustomBlocks, Formatting.Indented, settings);

                return Ok(serializedUserCustomBlocks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }
    }
}

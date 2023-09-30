using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AiDesigner.Server.Data;
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
    public class ProgramsController : ControllerBase
    {
        private readonly DBConnection _dbConnection;

        public ProgramsController(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // POST api/programs
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateProgram([FromBody] JsonElement body)
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

            // Deserialize the JsonElement body into a CustomProgram object
            CustomProgram? program = new CustomProgram();
            try
            {
                program = JsonConvert.DeserializeObject<CustomProgram>(body.GetRawText(), settings);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (program == null)
            {
                return BadRequest();
            }

            // Allow null ProgramStructure
            if (program.ProgramStructure == null)
            {
                program.ProgramStructure = new ProgramStructure();
            }

            try
            {
                var programId = await _dbConnection.SaveProgramAsync(program);
                return programId;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex);
                // Return a 500 status code
                return StatusCode(500, "Internal server error");
            }
        }


        // PUT api/programs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] JsonElement body)
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

            var program = JsonConvert.DeserializeObject<CustomProgram>(body.GetRawText(), settings);

            program.LastOpened = DateTime.Now;

            if (id != program.Id)
            {
                return BadRequest();
            }

            // Allow null ProgramStructure
            if (program.ProgramStructure == null)
            {
                program.ProgramStructure = new ProgramStructure();
            }

            await _dbConnection.UpdateProgramAsync(program);
            return NoContent();
        }

        // GET api/programs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> GetProgram(Guid id)
        {
            try
            {
                ProgramObject programObject = await _dbConnection.LoadProgramAsync(id);

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

                if (programObject == null)
                {
                    return NotFound();
                }

                var serializedUserPrograms = JsonConvert.SerializeObject(programObject, Formatting.Indented, settings);

                return Ok(serializedUserPrograms);
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/programs/{id}
        [HttpDelete("{userId}/{id}")]
        public async Task<IActionResult> DeleteProgram(Guid userId, Guid id)
        {
            await _dbConnection.DeleteProgramAsync(userId, id);
            return NoContent();
        }

        // GET api/programs/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<string>> GetUserPrograms(Guid userId)
        {
            try
            {
                IEnumerable<ProgramObject> userPrograms = await _dbConnection.GetAllUserProgramsAsync(userId);

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


        // GET api/programs/public
        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<ProgramObject>>> GetPublicPrograms()
        {
            IEnumerable<ProgramObject> publicPrograms = await _dbConnection.SearchPublicProgramsAsync("");
            return Ok(publicPrograms);
        }

        // GET api/programs/search/{searchTerm}
        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<ProgramObject>>> SearchPublicPrograms(string searchTerm)
        {
            IEnumerable<ProgramObject> searchResults = await _dbConnection.SearchPublicProgramsAsync(searchTerm);
            return Ok(searchResults);
        }

        // POST api/userprogram/connect/{userId}/{programId}
        [HttpPost("connect/{userId}/{programId}")]
        public async Task<IActionResult> ConnectUserToProgram(Guid userId, Guid programId)
        {
            await _dbConnection.ConnectUserToProgramAsync(userId, programId);
            return NoContent();
        }

        // POST api/userprogram/disconnect/{userId}/{programId}
        [HttpPost("disconnect/{userId}/{programId}")]
        public async Task<IActionResult> DisconnectUserFromProgram(Guid userId, Guid programId)
        {
            await _dbConnection.DisconnectUserFromProgramAsync(userId, programId);
            return NoContent();
        }
    }
}

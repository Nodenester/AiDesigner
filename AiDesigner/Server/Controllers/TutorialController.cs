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
    [Route("Tutorial")]
    [ApiController]
    public class TutorialController : ControllerBase
    {
        private readonly DBConnection _dbConnection;

        public TutorialController(DBConnection dbConnection)
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

        // POST api/Tutorial/Create
        [HttpPost("Create")]
        public async Task<ActionResult> CreateTutorial([FromBody] Tutorial tutorial)
        {
            try
            {
                var tutorialId = await _dbConnection.CreateTutorialAsync(tutorial);
                if (tutorialId > 0)
                    return Ok(new { message = "Tutorial created successfully.", tutorialId = tutorialId });
                else
                    return BadRequest("Failed to create tutorial.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Tutorial/Uncompleted/{userId}
        [HttpGet("Uncompleted/{userId}")]
        public async Task<ActionResult<IEnumerable<Tutorial>>> GetUncompletedTutorials(string userId)
        {
            try
            {
                var tutorials = await _dbConnection.GetUncompletedTutorialsForUserAsync(userId);
                return Ok(tutorials);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // PUT api/Tutorial/Complete/{input}
        [HttpPut("Complete/{input}")]
        public async Task<ActionResult> MarkTutorialAsCompleted(string input)
        {
            try
            {
                var parts = input.Split(':');
                if (parts.Length != 2)
                    return BadRequest("Invalid input format. Expected format: tutorialId:userId");

                int tutorialId = int.Parse(parts[0]);
                string userId = parts[1];

                var result = await _dbConnection.MarkTutorialAsCompletedForUserAsync(tutorialId, userId);
                if (result > 0)
                    return Ok("Tutorial marked as completed successfully.");
                else
                    return BadRequest("Failed to mark tutorial as completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }


        // GET api/Tutorial/Completed/{userId}
        [HttpGet("Completed/{userId}")]
        public async Task<ActionResult<IEnumerable<Tutorial>>> GetCompletedTutorials(string userId)
        {
            try
            {
                var tutorials = await _dbConnection.GetCompletedTutorialsForUserAsync(userId);
                return Ok(tutorials);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Tutorial/UserData/{userId}
        [HttpGet("UserData/{userId}")]
        public async Task<ActionResult<IEnumerable<Tutorial>>> GetTutorialsWithUserData(string userId)
        {
            try
            {
                var tutorials = await _dbConnection.GetTutorialsWithUserDataAsync(userId);
                return Ok(tutorials);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // GET api/Tutorial/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Tutorial>>> GetAllTutorials()
        {
            try
            {
                var tutorials = await _dbConnection.GetAllTutorialsAsync();
                if (tutorials != null && tutorials.Any())
                    return Ok(tutorials);
                else
                    return NotFound("No tutorials found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // PUT api/Tutorial/Update
        [HttpPut("Update")]
        public async Task<ActionResult> UpdateTutorial([FromBody] Tutorial tutorial)
        {
            try
            {
                var isSuccess = await _dbConnection.UpdateTutorialAsync(tutorial);
                if (isSuccess)
                    return Ok("Tutorial updated successfully.");
                else
                    return BadRequest("Failed to update tutorial.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

        // DELETE api/Tutorial/Delete/{tutorialId}
        [HttpDelete("Delete/{tutorialId}")]
        public async Task<ActionResult> DeleteTutorial(int tutorialId)
        {
            try
            {
                var isSuccess = await _dbConnection.DeleteTutorialAsync(tutorialId);
                if (isSuccess)
                    return Ok("Tutorial deleted successfully.");
                else
                    return BadRequest("Failed to delete tutorial.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message + " (from: " + ex.TargetSite);
            }
        }

    }

}

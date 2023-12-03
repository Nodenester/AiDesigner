using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AiDesigner.Server.Data;
using AiDesigner.Shared.Blocks;
using AiDesigner.Shared.Data;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodeBaseApi.Version2;
using OtpNet;

namespace AiDesigner.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WorkshopController : ControllerBase
    {
        private readonly DBConnection _dbConnection;

        public WorkshopController(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Count for Search Articles
        [HttpGet("search/count")]
        public async Task<IActionResult> GetArticleCountForSearch(string searchTerm = null, string searchClass = null, string type = null)
        {
            int totalCount = await _dbConnection.GetArticleCountForSearchAsync(searchTerm, searchClass, type);
            return Ok(new { totalCount });
        }

        // Search Articles
        [HttpGet("search")]
        public async Task<IActionResult> SearchArticles(string searchTerm = null, string searchClass = null, string type = null, int start = 0, int end = 10)
        {
            var articles = await _dbConnection.SearchArticlesAsync(start, end, searchTerm, searchClass, type);
            return Ok(articles);
        }

        [HttpGet("author-articles")]
        public async Task<IActionResult> GetArticleCountForSearch(string userId)
        {
            var articles = await _dbConnection.GetArticlesByAuthorIdAsync(userId);
            return Ok(articles);
        }

        // Count for most downloaded articles
        [HttpGet("most-downloaded/count")]
        public async Task<IActionResult> GetCountOfMostDownloadedArticles()
        {
            int totalCount = await _dbConnection.GetCountOfMostDownloadedArticlesAsync();
            return Ok(new { totalCount });
        }

        //Get most downloaded articles
        [HttpGet("most-downloaded")]
        public async Task<IActionResult> GetMostDownloadedArticles(int start = 0, int end = 10)
        {
            var articles = await _dbConnection.GetMostDownloadedArticlesAsync(start, end);
            return Ok(articles);
        }

        // Count for top-rated articles
        [HttpGet("top-rated/count")]
        public async Task<IActionResult> GetCountOfTopRatedArticles()
        {
            int totalCount = await _dbConnection.GetCountOfTopRatedArticlesAsync();
            return Ok(new { totalCount });
        }

        //Get top rated articles
        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedArticles(int start = 0, int end = 10)
        {
            var articles = await _dbConnection.GetTopRatedArticlesAsync(start, end);
            return Ok(articles);
        }

        //Article handling
        [HttpPost("save-article")]
        public async Task<IActionResult> SaveArticle([FromBody] WorkshopArticle article)
        {
            // Validate the input article object
            if (article == null || !ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid article data" });
            }

            try
            {
                string id = await _dbConnection.SaveArticleAsync(article);

                if (string.IsNullOrEmpty(id))
                {
                    return StatusCode(500, new { Message = "Failed to save the article" });
                }

                return CreatedAtAction(nameof(SaveArticle), new { Id = id });
            }
            catch (Exception ex)
            {
                // Log the exception
                // For example: _logger.LogError(ex, "An error occurred while saving the article.");
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("article/{id}")]
        public async Task<IActionResult> GetArticleById(Guid id)
        {
            var article = await _dbConnection.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return Ok(article);
        }

        [HttpGet("article/program/{id}")]
        public async Task<IActionResult> GetArticleByProgramId(Guid id)
        {
            var article = await _dbConnection.GetArticleByProgramIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return Ok(article);
        }

        [HttpGet("author/{authorId}/articles")]
        public async Task<IActionResult> GetArticlesByAuthor(string authorId)
        {
            var articles = await _dbConnection.GetArticlesByAuthorAsync(authorId);
            return Ok(articles);
        }

        [HttpPut("update-article")]
        public async Task<IActionResult> UpdateArticle(WorkshopArticle article)
        {
            await _dbConnection.UpdateArticleAsync(article);
            return Ok();
        }

        [HttpDelete("article-images/{articleId}")]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            await _dbConnection.DeleteArticleAsync(id);
            return Ok();
        }

        [HttpPatch("article/{id}/status")]
        public async Task<IActionResult> UpdateArticleStatus(Guid id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest(new { Message = "Invalid status value" });
            }

            try
            {
                await _dbConnection.UpdateArticleStatusAsync(id.ToString(), status);
                return Ok(new { Message = "Article status updated successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception
                // For example: _logger.LogError(ex, "An error occurred while updating the article status.");
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("articles/pending")]
        public async Task<IActionResult> GetPendingArticles()
        {
            try
            {
                var pendingArticles = await _dbConnection.GetPendingArticlesAsync();
                return Ok(pendingArticles);
            }
            catch (Exception ex)
            {
                // Log the exception
                // For example: _logger.LogError(ex, "An error occurred while fetching pending articles.");
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }

        [HttpPost("connect-article-to-user")]
        public async Task<IActionResult> ConnectArticleToUser([FromBody] UserArticle userArticle)
        {
            await _dbConnection.ConnectArticleToUserAsync(userArticle);
            return Ok();
        }

        [HttpGet("get-articles-connected-to-user/{userId}")]
        public async Task<IActionResult> GetArticlesConnectedToUser([FromRoute] string userId)
        {
            try
            {
                var articles = await _dbConnection.GetArticlesConnectedToUserAsync(userId);
                if (articles == null || !articles.Any())
                {
                    return NotFound("No articles found for this user.");
                }
                return Ok(articles);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., _logger.LogError(ex, "An error occurred while getting articles."));
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("update-user-article")]
        public async Task<IActionResult> UpdateUserArticle([FromBody] UserArticle userArticle)
        {
            try
            {
                await _dbConnection.UpdateUserArticleAsync(userArticle);
                return Ok("Successfully updated.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., _logger.LogError(ex, "An error occurred while updating user-article."));
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("update-user-article2/{programId}")]
        public async Task<IActionResult> UpdateUserArticle(string programId, [FromBody] UserArticle userArticle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // This will return the validation errors.
            }

            try
            {
                await _dbConnection.UpdateUserArticleAsync2(userArticle, programId);
                return Ok("Successfully updated.");
            }
            catch (Exception ex)
            {
                // Log the exception details here to get more information
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



        [HttpDelete("remove-user-article/{userId}/{articleId}")]
        public async Task<IActionResult> RemoveUserArticle([FromRoute] string userId, [FromRoute] string articleId)
        {
            try
            {
                var result = await _dbConnection.RemoveUserArticleAsync(userId, articleId);
                if (result == "Successfully Removed")
                {
                    return Ok("Successfully removed.");
                }
                else if (result == "Removal Failed")
                {
                    return NotFound("User-article relationship not found.");
                }
                return BadRequest("An unknown error occurred.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., _logger.LogError(ex, "An error occurred while removing user-article."));
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        //article ai generation
        [HttpPost("generate-article-image")]
        public async Task<IActionResult> GenerateArticleImage([FromBody] WorkshopArticle article)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_HF_TOKEN_HERE");

                var program = await _dbConnection.LoadProgramAsync(Guid.Parse(article.ProgramId)); // Ensure this is awaited

                string prompt = $"Create a logo for the program named '{program.Name}' described as: {program.Description}";

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { inputs = prompt }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://api-inference.huggingface.co/models/openskyml/dalle-3-xl", content);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Failed to generate image");
                }

                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                // Convert the byte array to a Base64 string
                string base64Image = Convert.ToBase64String(imageBytes);

                return Ok(base64Image);
            }
            catch (Exception ex)
            {
                // Log the exception
                // For example: _logger.LogError(ex, "An error occurred while generating the article image.");
                return StatusCode(500, new { Message = "An error occurred while processing your request" });
            }
        }


        [HttpPost("generate-article-description")]
        public async Task<IActionResult> GenerateArticleDescription([FromBody] WorkshopArticle article)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_HF_TOKEN_HERE");

                ProgramObject program = _dbConnection.LoadProgramAsync(Guid.Parse(article.ProgramId)).Result;

                StringBuilder promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("Generate a concise and informative description of the following program:");
                promptBuilder.AppendLine();

                // Program Information
                promptBuilder.AppendLine($"Program Name: {program.Name}");
                promptBuilder.AppendLine($"Program Description: {program.Description}");
                promptBuilder.AppendLine();

                // Program Inputs
                promptBuilder.AppendLine("Program Inputs:");
                foreach (var input in program.ProgramStructure.ProgramStart)
                {
                    promptBuilder.AppendLine($"- {input.Name}: {input.Description}");
                }
                promptBuilder.AppendLine();

                // Program Outputs
                promptBuilder.AppendLine("Program Outputs:");
                foreach (var output in program.ProgramStructure.ProgramEnd) // Assuming ProgramEnd contains outputs
                {
                    promptBuilder.AppendLine($"- {output.Name}: {output.Description}");
                }

                // Adding a tag to indicate where the generated description should start
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("<!-- Generated Description Start -->");

                string prompt = promptBuilder.ToString();

                var parameters = new
                {
                    max_new_tokens = 1024,
                    top_p = 0.8,
                    temperature = 0.6,
                    return_full_text = false,
                    stop = new string[] { "<!-- Generated Description End -->" }
                };

                var payload = new
                {
                    inputs = prompt,
                    parameters
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://api-inference.huggingface.co/models/meta-llama/Llama-2-70b-chat-hf", content);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Failed to generate description");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // Process the response to extract the generated description
                // Assuming the response is JSON and contains a key for the generated text

                return Ok(responseContent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Home page
        [HttpGet("most-popular")]
        public async Task<IActionResult> GetMostPopularArticles()
        {
            var articles = await _dbConnection.GetMostPopularArticlesAsync();
            return Ok(articles);
        }
        [HttpGet("most-downloaded/top")]
        public async Task<IActionResult> GetTopDownloadedArticles()
        {
            var articles = await _dbConnection.GetTopDownloadedArticlesAsync();
            return Ok(articles);
        }
        [HttpGet("newest")]
        public async Task<IActionResult> GetNewestArticles()
        {
            var articles = await _dbConnection.GetNewestArticlesAsync();
            return Ok(articles);
        }
        [HttpGet("author")]
        public async Task<IActionResult> Get4ArticlesByAuthor()
        {
            var articles = await _dbConnection.Get5ArticlesByAuthorAsync("14fa5960-cfeb-4cb5-8718-0df2fe41b071");
            return Ok(articles);
        }

    }
}

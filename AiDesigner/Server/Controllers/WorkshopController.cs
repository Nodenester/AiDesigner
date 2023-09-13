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

        [HttpDelete("article/{id}")]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            await _dbConnection.DeleteArticleAsync(id);
            return Ok();
        }

        //Article image handling
        [HttpPost("add-article-image")]
        public async Task<IActionResult> InsertArticleImage([FromBody] ArticleImages articleImage)
        {
            // Log to check if articleImage is correctly populated
            Console.WriteLine($"Received articleImage: {JsonConvert.SerializeObject(articleImage)}");

            if (articleImage == null)
            {
                return BadRequest(new { Message = "articleImage is null" });
            }

            try
            {
                var result = await _dbConnection.InsertArticleImageAsync(articleImage);

                // Log to check if database operation was successful
                Console.WriteLine($"Database operation result: {result}");

                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex}");

                return StatusCode(500, new { Message = "Internal Server Error" });
            }
        }

        [HttpGet("article-images/{articleId}")]
        public async Task<IActionResult> GetArticleImages(string articleId)
        {
            var articleImages = await _dbConnection.GetArticleImagesAsync(articleId);
            return Ok(articleImages);
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

    }
}

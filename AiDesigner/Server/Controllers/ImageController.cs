using Microsoft.AspNetCore.Mvc;

namespace AiDesigner.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        [HttpPost]
        public IActionResult UploadImage([FromBody] byte[] imageData)
        {
            var fileName = $"images/{Guid.NewGuid()}.png";
            var filePath = Path.Combine("wwwroot", fileName);
            System.IO.File.WriteAllBytes(filePath, imageData);
            return Ok(fileName);
        }
    }

}

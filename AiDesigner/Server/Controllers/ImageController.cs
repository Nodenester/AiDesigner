using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AiDesigner.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        [HttpPost]
        public IActionResult UploadImage([FromBody] byte[] imageData)
        {
            var imageHash = GetSha256Hash(imageData);
            var existingFile = Directory.GetFiles("wwwroot/images")
                                        .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == imageHash);

            if (existingFile != null)
            {
                return Ok($"images/{Path.GetFileName(existingFile)}");
            }

            var fileName = $"images/{imageHash}.png";
            var filePath = Path.Combine("wwwroot", fileName);
            System.IO.File.WriteAllBytes(filePath, imageData);
            return Ok(fileName);
        }

        private string GetSha256Hash(byte[] input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(input);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}

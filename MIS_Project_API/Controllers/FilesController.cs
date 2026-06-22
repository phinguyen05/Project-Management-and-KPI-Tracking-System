using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace MIS_Project_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".zip", ".txt" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FilesController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Không có file được tải lên.");

            if (file.Length > MaxFileSize)
                return BadRequest("File vượt quá dung lượng cho phép (tối đa 10MB).");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest($"Định dạng file '{extension}' không được hỗ trợ.");

            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var safeFileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{uniqueFileName}";
            return Ok(new { FileUrl = fileUrl, FileName = safeFileName });
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            var filePath = Path.Combine(uploadsFolder, safeFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Không tìm thấy file.");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(safeFileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, contentType, safeFileName);
        }
    }
}

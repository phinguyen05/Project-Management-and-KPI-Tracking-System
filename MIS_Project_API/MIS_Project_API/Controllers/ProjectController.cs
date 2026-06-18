using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.Models;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ổ khóa: Bắt buộc phải có JWT Token hợp lệ mới được vào
    public class ProjectController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public ProjectController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách toàn bộ dự án
        [HttpGet]
        public IActionResult GetAllProjects()
        {
            var projects = _context.Projects.ToList();
            return Ok(projects);
        }

        // 2. Tạo dự án mới
        [HttpPost]
        public IActionResult CreateProject([FromBody] Project project)
        {
            // Nhận object Project từ dưới lên và lưu vào DB
            _context.Projects.Add(project);
            _context.SaveChanges();

            return Ok(new { message = "Tạo dự án thành công!", project });
        }
    }
}
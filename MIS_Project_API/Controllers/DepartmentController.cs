using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Models;
using System.Linq;

namespace MIS_Project_API.Controllers
{
    [Route("api/department")] // Gõ cứng đường dẫn, không dùng [controller] nữa
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public DepartmentController(MisProjectManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetAllDepartments()
        {
            // Trả về danh sách phòng ban, ánh xạ đúng tên biến cho Frontend dễ đọc
            var departments = await _context.Departments
                .Select(d => new {
                    id = d.DepartmentId, // Hoặc d.Id tùy theo cấu trúc model của bạn
                    name = d.Name
                })
                .ToListAsync();

            return Ok(departments);
        }
    }
}
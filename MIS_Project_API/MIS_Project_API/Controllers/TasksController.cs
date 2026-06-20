using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.DTOs.Task;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models; // ĐÃ THÊM: Để gọi được DbContext và Models

namespace MIS_Project_API.Controllers
{
    // Dinh nghia lop model ngay trong controller de khong lam rac thu muc DTO
    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = null!;
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly MisProjectManagementContext _context; // [ĐÃ FIX]: Khai báo thêm biến _context

        // [ĐÃ FIX]: Tiêm (Inject) MisProjectManagementContext vào Constructor
        public TasksController(ITaskService taskService, MisProjectManagementContext context)
        {
            _taskService = taskService;
            _context = context;
        }

        [HttpGet("project/{projectId}/wbs")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetWbs(int projectId)
        {
            var wbs = await _taskService.GetProjectTasksWbsAsync(projectId);
            return Ok(wbs);
        }

        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto createDto)
        {
            var task = await _taskService.CreateTaskAsync(createDto);
            return Ok(task);
        }

        [HttpPut("{id}/dates")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> UpdateDates(int id, [FromBody] UpdateTaskDateDto updateDto)
        {
            var success = await _taskService.UpdateTaskDatesAndShiftChildrenAsync(id, updateDto);
            if (!success) return NotFound("Không tìm thấy Task hoặc Task chưa có ngày bắt đầu.");
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest updateDto)
        {
            // Lay thong tin user dang call API
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int currentUserId);
            bool isManager = User.IsInRole("Manager") || User.IsInRole("Admin");

            var result = await _taskService.UpdateTaskStatusAsync(id, updateDto.Status, currentUserId, isManager);

            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy") || result.Message.Contains("quyền")) return Forbid(result.Message);
                return BadRequest(result.Message);
            }
            return Ok(new { message = result.Message });
        }

        // PATCH: api/tasks/{id}/risk
        [HttpPatch("{id}/risk")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Manager/Admin mới có quyền duyệt
        public async Task<IActionResult> ApproveRisk(int id, [FromBody] bool isRiskApproved)
        {
            // Lúc này biến _context đã có sẵn để gọi!
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound("Không tìm thấy Task.");

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            task.RiskFlag = isRiskApproved;
            task.UpdatedBy = userId; // Ghi vết người duyệt để lưu vào Audit Log

            await _context.SaveChangesAsync();

            var msg = isRiskApproved ? "Đã duyệt cờ Rủi ro (miễn phạt trễ hạn)." : "Đã gỡ cờ Rủi ro.";
            return Ok(new { message = msg });
        }
    }
}
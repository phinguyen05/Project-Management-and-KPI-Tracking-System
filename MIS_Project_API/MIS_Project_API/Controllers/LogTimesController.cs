using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.DTOs.LogTime;
using MIS_Project_API.Models;
using System.Security.Claims;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogTimesController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public LogTimesController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // Nhân viên nộp báo cáo giờ làm
        [HttpPost]
        [Authorize]
        public async System.Threading.Tasks.Task<IActionResult> CreateLogTime([FromBody] CreateLogTimeDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            // Dùng AsNoTracking() để EF không dính líu đến việc update ngầm entity Task
            var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.TaskId == dto.TaskId);
            if (task == null) return NotFound("Task không tồn tại.");

            if (task.AssigneeId != userId)
                return StatusCode(403, "Bạn chỉ được phép báo cáo giờ làm cho công việc của chính mình.");

            // Fallback an toàn: Nếu chuỗi JSON bị lỗi parse ra MinValue, tự lấy ngày hiện tại
            var validDate = dto.LogDate == DateTime.MinValue ? DateTime.Now : dto.LogDate;

            var logTime = new LogTime
            {
                TaskId = dto.TaskId,
                UserId = userId,
                ActualHours = dto.ActualHours,
                Description = dto.Description,
                LogDate = DateOnly.FromDateTime(validDate),
                AttachedFile = dto.AttachedFile,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.LogTimes.Add(logTime);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Nộp báo cáo giờ làm thành công, chờ duyệt.", logId = logTime.LogId });
        }

        // Quản lý duyệt/từ chối Log-time
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        public async System.Threading.Tasks.Task<IActionResult> ApproveLogTime(int id, [FromBody] ApproveLogTimeDto dto)
        {
            var logTime = await _context.LogTimes.FindAsync(id);
            if (logTime == null) return NotFound("Không tìm thấy Log-time.");

            if (dto.Status != "Approved" && dto.Status != "Rejected")
                return BadRequest("Trạng thái chỉ chấp nhận 'Approved' hoặc 'Rejected'.");

            logTime.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Đã {dto.Status} báo cáo giờ làm." });
        }
    }
}

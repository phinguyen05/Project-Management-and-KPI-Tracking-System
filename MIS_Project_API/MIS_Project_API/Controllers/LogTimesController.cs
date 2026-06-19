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
        public async Task<IActionResult> SubmitLogTime([FromBody] CreateLogTimeDto dto)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var logTime = new LogTime
            {
                UserId = currentUserId,
                TaskId = dto.TaskId,
                ActualHours = dto.ActualHours,
                Description = dto.Description,
                LogDate = DateOnly.FromDateTime(dto.LogDate),
                Status = "Pending", // Mặc định chờ duyệt
                CreatedAt = DateTime.Now
            };

            _context.LogTimes.Add(logTime);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Nộp Log-time thành công, chờ Quản lý duyệt.", LogId = logTime.LogId });
        }

        // Quản lý duyệt/từ chối Log-time
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveLogTime(int id, [FromBody] ApproveLogTimeDto dto)
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
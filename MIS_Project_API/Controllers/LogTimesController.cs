using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim)) return null;
            if (!int.TryParse(userIdClaim, out var userId)) return null;
            return userId;
        }


        public LogTimesController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // Nhân viên nộp báo cáo giờ làm (có đính kèm file bằng chứng)
        [HttpPost]
        [Authorize]
        public async System.Threading.Tasks.Task<IActionResult> CreateLogTime([FromForm] CreateLogTimeDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.TaskId == dto.TaskId);
            if (task == null) return NotFound("Task không tồn tại.");

            if (task.AssigneeId != userId)
                return StatusCode(403, "Bạn chỉ được phép báo cáo giờ làm cho công việc của chính mình.");

            var validDate = dto.LogDate == DateTime.MinValue ? DateTime.Now : dto.LogDate;

            string? attachedFilePath = null;
            if (dto.AttachedFile != null)
            {
                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                var extension = Path.GetExtension(dto.AttachedFile.FileName);
                var fileName = $"logtime_{userId}_{dto.TaskId}_{DateTime.UtcNow.Ticks}{extension}";
                var fullPath = Path.Combine(uploadsRoot, fileName);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.AttachedFile.CopyToAsync(stream);
                }

                attachedFilePath = $"/uploads/{fileName}";
            }

            var logTime = new LogTime
            {
                TaskId = dto.TaskId,
                UserId = userId,
                ActualHours = dto.ActualHours,
                Description = dto.Description,
                LogDate = DateOnly.FromDateTime(validDate),
                AttachedFile = attachedFilePath,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.LogTimes.Add(logTime);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Nộp báo cáo giờ làm thành công, chờ duyệt.", logId = logTime.LogId });
        }

        // Lấy danh sách Log-time đang chờ duyệt
        [HttpGet("pending")]
        [Authorize(Roles = "Manager,Admin")]
        public async System.Threading.Tasks.Task<IActionResult> GetPendingLogTimes()
        {
            var pendingLogs = await _context.LogTimes
                .Where(l => l.Status == "Pending")
                .Select(l => new {
                    logId = l.LogId,
                    actualHours = l.ActualHours,
                    description = l.Description,
                    logDate = l.LogDate,
                    attachedFile = l.AttachedFile,
                    taskName = _context.Tasks.Where(t => t.TaskId == l.TaskId).Select(t => t.Name).FirstOrDefault(),
                    userName = _context.Users.Where(u => u.UserId == l.UserId).Select(u => u.FullName).FirstOrDefault()
                }).ToListAsync();

            return Ok(pendingLogs);
        }

        // GET /api/logtimes/me
        [HttpGet("me")]
        public async System.Threading.Tasks.Task<IActionResult> GetMyLogTimes()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var logs = await _context.LogTimes
                .Where(l => l.UserId == userId.Value)
                .Select(l => new {
                    logId = l.LogId,
                    taskName = _context.Tasks.Where(t => t.TaskId == l.TaskId).Select(t => t.Name).FirstOrDefault(),
                    actualHours = l.ActualHours,
                    logDate = l.LogDate,
                    status = l.Status,
                    description = l.Description,
                    attachedFile = l.AttachedFile
                })
                .ToListAsync();

            return Ok(logs);
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

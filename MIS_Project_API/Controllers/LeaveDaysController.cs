using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.DTOs.LeaveDay;
using MIS_Project_API.Models;
using System.Security.Claims;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveDaysController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public LeaveDaysController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // GET /api/leavedays
        [HttpGet]
        public async Task<IActionResult> GetAllLeaveDays()
        {
            var leaves = await _context.LeaveDays
                .AsNoTracking()
                .Include(ld => ld.User)
                .OrderByDescending(ld => ld.CreatedAt)
                .Select(ld => new
                {
                    leaveId = ld.LeaveId,
                    userId = ld.UserId,
                    userName = ld.User != null ? ld.User.FullName : null,
                    leaveDate = ld.LeaveDate,
                    reason = ld.Reason,
                    status = ld.Status,
                    createdAt = ld.CreatedAt
                })
                .ToListAsync();

            return Ok(leaves);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitLeaveRequest([FromBody] CreateLeaveDayDto dto)

        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var leaveDay = new LeaveDay
            {
                UserId = currentUserId,
                LeaveDate = DateOnly.FromDateTime(dto.LeaveDate),
                Reason = dto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.LeaveDays.Add(leaveDay);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã gửi đơn xin nghỉ phép.", LeaveId = leaveDay.LeaveId });
        }

        // PUT /api/leavedays/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateLeaveDayStatus(int id, [FromBody] ApproveLeaveDayDto dto)

        {
            var leaveDay = await _context.LeaveDays.FindAsync(id);
            if (leaveDay == null) return NotFound("Không tìm thấy đơn xin nghỉ.");

            if (dto.Status != "Approved" && dto.Status != "Rejected")
                return BadRequest("Trạng thái không hợp lệ.");

            leaveDay.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Đã {dto.Status} đơn xin nghỉ phép." });
        }

    }
}
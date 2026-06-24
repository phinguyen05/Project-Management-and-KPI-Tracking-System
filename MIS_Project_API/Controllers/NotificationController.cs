using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Models;

namespace MIS_Project_API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public NotificationController(MisProjectManagementContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // GET /api/notifications - lấy toàn bộ thông báo của user hiện tại (mới nhất trước)
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetMyNotifications()
        {
            var userId = GetCurrentUserId();

            var items = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    id = n.NotifId,
                    message = n.Content,
                    isRead = n.IsRead ?? false,
                    createdAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // PUT /api/notifications/{id}/read - đánh dấu đã đọc
        [HttpPut("{id}/read")]
        public async System.Threading.Tasks.Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotifId == id && n.UserId == userId);

            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}


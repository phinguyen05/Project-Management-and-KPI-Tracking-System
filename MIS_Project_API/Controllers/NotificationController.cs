using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Models;
using Hangfire;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public NotificationController(MisProjectManagementContext context) { _context = context; }

        // ĐÃ FIX: Chỉ định rõ System.Threading.Tasks.Task
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetMyNotifications()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync());
        }
        // ĐÃ FIX 1: Bỏ chữ static, bỏ biến db truyền vào, dùng luôn _context
        public async System.Threading.Tasks.Task CheckTaskDeadlinesJob()
        {
            var today = DateTime.Now.Date;
            var tasks = await _context.Tasks.Include(t => t.Assignee)
                                      .Where(t => t.Status != "Done" && t.Deadline != null)
                                      .ToListAsync();

            foreach (var task in tasks)
            {
                if (task.AssigneeId == null) continue;

                var daysLeft = (task.Deadline.Value.Date - today).Days;
                string content = "";

                if (daysLeft == 1) content = $"[Sắp đến hạn] Task '{task.Name}' sẽ hết hạn vào ngày mai.";
                else if (daysLeft < 0) content = $"[Trễ hạn] Task '{task.Name}' đã trễ hạn! Vui lòng cập nhật ngay.";

                if (!string.IsNullOrEmpty(content))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = task.AssigneeId.Value,
                        Content = content,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        // ĐÃ FIX 2: API test thủ công trên trình duyệt
        [HttpGet("run-job")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> RunJobManual()
        {
            await CheckTaskDeadlinesJob();
            return Ok("Đã quét và tạo thông báo xong!");
        }
    }
}
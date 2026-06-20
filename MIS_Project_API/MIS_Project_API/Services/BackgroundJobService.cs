using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;
namespace MIS_Project_API.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly MisProjectManagementContext _context;
        public BackgroundJobService(MisProjectManagementContext context)
        {
            _context = context;
        }
        public async System.Threading.Tasks.Task CheckAndAlertOverdueTasksAsync()
        {
            var today = DateTime.Now.Date;
            var overdueTasks = await _context.Tasks
                .Where(t => t.Deadline.HasValue && t.Deadline.Value.Date < today && t.Status != "Done")
                .ToListAsync();
            foreach (var task in overdueTasks)
            {
                if (task.AssigneeId.HasValue)
                {
                    var notif = new Notification
                    {
                        UserId = task.AssigneeId.Value,
                        Content = $"[Trễ hạn] Task '{task.Name}' đã trễ hạn! Vui lòng cập nhật tiến độ ngay.",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notif);
                }
            }
            var tomorrow = today.AddDays(1);
            var dueSoonTasks = await _context.Tasks
                .Where(t => t.Deadline.HasValue && t.Deadline.Value.Date == tomorrow && t.Status != "Done")
                .ToListAsync();
            foreach (var task in dueSoonTasks)
            {
                if (task.AssigneeId.HasValue)
                {
                    var notif = new Notification
                    {
                        UserId = task.AssigneeId.Value,
                        Content = $"[Sắp đến hạn] Task '{task.Name}' sẽ hết hạn vào ngày mai.",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notif);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task AutoApprovePendingLogTimesAsync()
        {
            var today = DateTime.Now.Date;
            var lastDayOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            var warningDay = lastDayOfMonth.AddDays(-1);

            var pendingLogs = await _context.LogTimes
                .Where(lt => lt.Status == "Pending")
                .ToListAsync();

            if (!pendingLogs.Any()) return;

            if (today == warningDay)
            {
                var managerIds = await _context.Users
                    .Where(u => u.Role == "Manager" || u.Role == "Admin")
                    .Select(u => u.UserId)
                    .ToListAsync();

                foreach (var managerId in managerIds)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = managerId,
                        Content = $"[Tối hậu thư] Còn {pendingLogs.Count} báo cáo giờ làm chưa duyệt. Sau 24h hệ thống sẽ tự động phê duyệt để tránh treo lương nhân sự.",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();
            }

            if (today == lastDayOfMonth)
            {
                foreach (var log in pendingLogs)
                {
                    log.Status = "Approved";
                    log.ApprovedAt = DateTime.Now;

                    _context.Notifications.Add(new Notification
                    {
                        UserId = log.UserId,
                        Content = $"Báo cáo giờ làm ngày {log.LogDate:dd/MM/yyyy} đã được hệ thống tự động phê duyệt do quá hạn xử lý.",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}

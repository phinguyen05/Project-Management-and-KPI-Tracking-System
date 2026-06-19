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

            // 1. Quét Task trễ hạn (Deadline < Hôm nay và chưa Done)
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

            // 2. Quét Task sắp đến hạn (Deadline = Ngày mai)
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
            // Sẽ triển khai chi tiết luồng chống tắc nghẽn sau
            await System.Threading.Tasks.Task.CompletedTask;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;

namespace MIS_Project_API.Services
{
    public class LogTimeBackgroundService : ILogTimeBackgroundService
    {
        private readonly MisProjectManagementContext _context;

        public LogTimeBackgroundService(MisProjectManagementContext context)
        {
            _context = context;
        }

        // ĐÃ FIX: Thêm System.Threading.Tasks.Task
        public async System.Threading.Tasks.Task AutoApprovePendingLogTimesAsync()
        {
            // Căn mốc 24h trước
            var thresholdDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

            // Quét toàn bộ Log_Time trạng thái Pending và có log_date <= 24h trước
            var pendingLogs = await _context.LogTimes
                .Where(l => l.Status == "Pending" && l.LogDate <= thresholdDate)
                .ToListAsync();

            if (pendingLogs.Any())
            {
                foreach (var log in pendingLogs)
                {
                    log.Status = "Approved";
                    log.ApprovedAt = DateTime.Now;
                    // Để trống ApprovedBy để nhận diện là Hệ Thống tự động duyệt
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[Hangfire] Đã tự động duyệt {pendingLogs.Count} báo cáo giờ làm quá hạn 24h.");
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;

namespace MIS_Project_API.Services
{
    public class KpiService : IKpiService
    {
        private readonly MisProjectManagementContext _context;

        public KpiService(MisProjectManagementContext context)
        {
            _context = context;
        }

        public async Task<double> CalculateKpiTimelinessAsync(int userId, int month, int year)
        {
            // 1. Áp dụng Rule "Cut-off Date": Chỉ lấy Task Done trong tháng đang xét
            var tasksInMonth = await _context.Tasks
                .Where(t => t.AssigneeId == userId
                         && t.Status == "Done"
                         && t.CompletedAt.HasValue
                         && t.CompletedAt.Value.Month == month
                         && t.CompletedAt.Value.Year == year)
                .ToListAsync();

            // 2. Áp dụng Rule "Division by zero": Tránh lỗi Crash hệ thống
            double totalEstTime = tasksInMonth.Sum(t => t.EstimatedTime ?? 0);
            if (totalEstTime == 0) return 0;

            double earnedEstTime = 0;

            foreach (var task in tasksInMonth)
            {
                double est = task.EstimatedTime ?? 0;
                if (!task.Deadline.HasValue)
                {
                    earnedEstTime += est;
                    continue;
                }

                int delayDays = (task.CompletedAt.Value.Date - task.Deadline.Value.Date).Days;

                if (delayDays <= 0)
                {
                    // Hoàn thành đúng hạn hoặc sớm hơn
                    earnedEstTime += est;
                }
                else
                {
                    // Trễ hạn - Áp dụng Penalty Factor
                    double penalty = 0.0;
                    if (delayDays >= 1 && delayDays <= 2) penalty = 0.8;
                    else if (delayDays >= 3 && delayDays <= 5) penalty = 0.5;
                    else penalty = 0.0;

                    earnedEstTime += (est * penalty);
                }
            }

            // 3. Tính toán KPI cuối cùng
            double kpiTimeliness = (earnedEstTime / totalEstTime) * 100.0;
            return Math.Round(kpiTimeliness, 2);
        }
    }
}

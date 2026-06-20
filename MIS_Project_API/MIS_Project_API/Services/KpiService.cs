using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;
using System.Text.Json;

namespace MIS_Project_API.Services
{
    public class KpiService : IKpiService
    {
        private readonly MisProjectManagementContext _context;

        public KpiService(MisProjectManagementContext context)
        {
            _context = context;
        }

        // --- 1. KPI TIẾN ĐỘ (40%) ---
        public async Task<double> CalculateKpiTimelinessAsync(int userId, int month, int year)
        {
            string monthYear = $"{month:D2}/{year}";
            var cycle = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.MonthYear == monthYear);

            // Đọc hệ số phạt từ JSON trong DB (Mặc định nếu chưa có cấu hình)
            Dictionary<string, double> penaltyFactors = new Dictionary<string, double>
            {
                { "1-2", 0.8 }, // Trễ 1-2 ngày tính 80%
                { "3-5", 0.5 }, // Trễ 3-5 ngày tính 50%
                { "over5", 0.0 } // Trễ quá 5 ngày tính 0%
            };

            if (cycle != null && !string.IsNullOrEmpty(cycle.PenaltyFactor))
            {
                try
                {
                    penaltyFactors = JsonSerializer.Deserialize<Dictionary<string, double>>(cycle.PenaltyFactor) ?? penaltyFactors;
                }
                catch { /* Giữ nguyên mặc định nếu JSON lỗi */ }
            }

            var tasksInMonth = await _context.Tasks
                .Where(t => t.AssigneeId == userId && t.Status == "Done" && t.CompletedAt.HasValue
                         && t.CompletedAt.Value.Month == month && t.CompletedAt.Value.Year == year)
                .ToListAsync();

            double totalEstTime = tasksInMonth.Sum(t => t.EstimatedTime ?? 0);
            if (totalEstTime == 0) return 0;

            double earnedEstTime = 0;

            foreach (var task in tasksInMonth)
            {
                double est = task.EstimatedTime ?? 0;

                // Nếu task không có deadline hoặc được cắm cờ Rủi ro (đã duyệt) -> Miễn phạt
                if (!task.Deadline.HasValue || ((task.RiskFlag ?? false) && (task.RiskApprovedByManager ?? false)))
                {
                    earnedEstTime += est;
                    continue;
                }

                int delayDays = (task.CompletedAt.Value.Date - task.Deadline.Value.Date).Days;
                if (delayDays <= 0)
                {
                    earnedEstTime += est;
                }
                else
                {
                    double penalty = 0.0;
                    if (delayDays >= 1 && delayDays <= 2) penalty = penaltyFactors.ContainsKey("1-2") ? penaltyFactors["1-2"] : 0.8;
                    else if (delayDays >= 3 && delayDays <= 5) penalty = penaltyFactors.ContainsKey("3-5") ? penaltyFactors["3-5"] : 0.5;
                    else penalty = penaltyFactors.ContainsKey("over5") ? penaltyFactors["over5"] : 0.0;

                    earnedEstTime += (est * penalty);
                }
            }

            double kpiTimeliness = (earnedEstTime / totalEstTime) * 100.0;
            return Math.Round(kpiTimeliness, 2);
        }

        // --- 2. KPI HIỆU SUẤT (30%) ---
        public async Task<double> CalculateKpiEfficiencyAsync(int userId, int month, int year)
        {
            var tasksInMonth = await _context.Tasks
                .Where(t => t.AssigneeId == userId && t.Status == "Done" && t.CompletedAt.HasValue
                         && t.CompletedAt.Value.Month == month && t.CompletedAt.Value.Year == year)
                .ToListAsync();

            if (!tasksInMonth.Any()) return 0;

            var taskIds = tasksInMonth.Select(t => t.TaskId).ToList();
            double totalEstTime = tasksInMonth.Sum(t => t.EstimatedTime ?? 0);

            // FIX: Chỉ tính giờ đã được duyệt (Approved)
            double totalActualTime = await _context.LogTimes
                .Where(lt => taskIds.Contains(lt.TaskId) && lt.Status == "Approved")
                .SumAsync(lt => lt.ActualHours);

            if (totalActualTime == 0) return 0;

            double kpiEfficiency = (totalEstTime / totalActualTime) * 100.0;
            if (kpiEfficiency > 150.0) kpiEfficiency = 150.0; // Trần 150%

            return Math.Round(kpiEfficiency, 2);
        }

        // --- 3. KPI KHỐI LƯỢNG (20%) ---
        public async Task<double> CalculateKpiCapacityAsync(int userId, int month, int year)
        {
            string monthYear = $"{month:D2}/{year}";
            var cycle = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.MonthYear == monthYear);
            int standardHours = cycle?.StandardWorkingHours ?? 160;

            var leaveDaysCount = await _context.LeaveDays
                .CountAsync(l => l.UserId == userId && l.LeaveDate.Month == month && l.LeaveDate.Year == year && l.Status == "Approved");

            int adjustedStandardHours = standardHours - (leaveDaysCount * 8);
            if (adjustedStandardHours <= 0) adjustedStandardHours = 8;

            // FIX: Chỉ tính giờ đã được duyệt (Approved)
            var totalActualTime = await _context.LogTimes
                .Where(lt => lt.UserId == userId && lt.LogDate.Month == month && lt.LogDate.Year == year && lt.Status == "Approved")
                .SumAsync(lt => lt.ActualHours);

            double kpiCapacity = (totalActualTime / adjustedStandardHours) * 100.0;
            return Math.Round(kpiCapacity, 2);
        }

        // --- 4. CHỐT SỔ ĐIỂM (TỔNG HỢP) ---
        public async Task<(bool Success, string Message, KpiReview? Data)> FinalizeKpiReviewAsync(int userId, int month, int year, double managerScore, string note)
        {
            string monthYear = $"{month:D2}/{year}";
            var cycle = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.MonthYear == monthYear);

            if (cycle == null)
                return (false, "Chưa thiết lập Kỳ đánh giá (SystemConfig) cho tháng này. Không thể chốt KPI.", null);

            double timeliness = await CalculateKpiTimelinessAsync(userId, month, year);
            double efficiency = await CalculateKpiEfficiencyAsync(userId, month, year);
            double capacity = await CalculateKpiCapacityAsync(userId, month, year);

            double baseScore = (timeliness * 0.4) + (efficiency * 0.3) + (capacity * 0.2) + (managerScore * 0.1);

            bool isOverloaded = capacity > 110.0;
            double finalScore = baseScore;

            if (isOverloaded && timeliness >= 90.0)
            {
                finalScore = baseScore * 1.1; // Thưởng 10% gánh vác
            }

            var existingReview = await _context.KpiReviews
                .FirstOrDefaultAsync(k => k.UserId == userId && k.CycleId == cycle.CycleId);

            if (existingReview != null)
            {
                existingReview.KpiTimeliness = timeliness;
                existingReview.KpiEfficiency = efficiency;
                existingReview.KpiCapacity = capacity;
                existingReview.KpiManagerEvaluation = managerScore;
                existingReview.FinalKpiScore = Math.Round(finalScore, 2);
                existingReview.IsOverloaded = isOverloaded;
                existingReview.Note = note;
            }
            else
            {
                var newReview = new KpiReview
                {
                    UserId = userId,
                    CycleId = cycle.CycleId,
                    KpiTimeliness = timeliness,
                    KpiEfficiency = efficiency,
                    KpiCapacity = capacity,
                    KpiManagerEvaluation = managerScore,
                    FinalKpiScore = Math.Round(finalScore, 2),
                    IsOverloaded = isOverloaded,
                    Note = note
                };
                await _context.KpiReviews.AddAsync(newReview);
                existingReview = newReview;
            }

            await _context.SaveChangesAsync();
            return (true, "Chốt KPI thành công.", existingReview);
        }
    }
}


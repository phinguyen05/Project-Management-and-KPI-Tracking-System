using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIS_Project_API.Controllers
{
    [Route("api/dashboard")] // Đã fix cứng route chuẩn để không bị 404
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public DashboardController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // GET /api/dashboard/gantt/{projectId}
        [HttpGet("gantt/{projectId}")]
        public async Task<IActionResult> GetGantt(int projectId)
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.Assignee)
                .ToListAsync();

            var taskIds = tasks.Select(t => t.TaskId).ToList();
            var deps = await _context.TaskDependencies
                .AsNoTracking()
                .Where(d => taskIds.Contains(d.PredecessorTaskId) && taskIds.Contains(d.SuccessorTaskId))
                .ToListAsync();

            var taskNameById = tasks.ToDictionary(t => t.TaskId, t => t.Name);

            var data = tasks.Select(t =>
            {
                var dependencyNames = deps
                    .Where(d => d.SuccessorTaskId == t.TaskId)
                    .Where(d => d.DependencyType == "Finish_to_Start")
                    .Select(d => taskNameById.ContainsKey(d.PredecessorTaskId) ? taskNameById[d.PredecessorTaskId] : d.PredecessorTaskId.ToString())
                    .Distinct()
                    .ToList();

                int progress = t.Status == "Done" ? 100 : (t.Status == "Doing" ? 50 : 0);
                string start = t.StartDate.HasValue ? t.StartDate.Value.ToString("yyyy-MM-dd") : "";
                
                // ĐÃ FIX: Sử dụng đúng thuộc tính Deadline của CSDL
                DateTime? due = t.Deadline; 
                string end = due.HasValue ? due.Value.ToString("yyyy-MM-dd") : "";

                return new
                {
                    id = t.TaskId,
                    name = t.Name,
                    start = start,
                    end = end,
                    status = t.Status,
                    progress = progress,
                    dependency = dependencyNames.Count > 0 ? string.Join(", ", dependencyNames) : "None"
                };
            }).ToList();

            return Ok(data);
        }

        // GET /api/dashboard/heatmap
        [HttpGet("heatmap")]
        public async Task<IActionResult> GetHeatmap()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "Employee" || u.Role == "Manager")
                .Select(u => new { u.UserId, u.FullName })
                .ToListAsync();

            var now = DateTime.Now;
            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = now.AddDays(-diff).Date;

            var dates = new List<DateTime>
            {
                monday,
                monday.AddDays(1),
                monday.AddDays(2),
                monday.AddDays(3),
                monday.AddDays(4)
            };

            var startDate = DateOnly.FromDateTime(dates.First());
            var endDate = DateOnly.FromDateTime(dates.Last());

            var logtimes = await _context.LogTimes
                .AsNoTracking()
                .Where(l => l.Status == "Approved" && l.LogDate >= startDate && l.LogDate <= endDate)
                .Select(l => new { l.UserId, l.LogDate, l.ActualHours })
                .ToListAsync();

            var result = users.Select(u =>
            {
                var dayTotals = dates.Select(d =>
                {
                    return logtimes
                        .Where(lt => lt.UserId == u.UserId && lt.LogDate == DateOnly.FromDateTime(d))
                        .Sum(lt => lt.ActualHours);
                }).ToList();

                var percents = dayTotals.Select(h => (h / 8.0) * 100.0).ToList();
                var max = percents.Count > 0 ? percents.Max() : 0;
                var status = max > 110 ? "Overload" : "Normal";

                return new
                {
                    name = u.FullName,
                    mon = Math.Round(percents[0], 0),
                    tue = Math.Round(percents[1], 0),
                    wed = Math.Round(percents[2], 0),
                    thu = Math.Round(percents[3], 0),
                    fri = Math.Round(percents[4], 0),
                    status
                };
            }).ToList();

            return Ok(result);
        }
    }
}
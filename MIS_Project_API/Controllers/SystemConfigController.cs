using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Models;
using MIS_Project_API.DTOs; // THÊM DÒNG NÀY ĐỂ FIX LỖI CS0246

namespace MIS_Project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class SystemConfigController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public SystemConfigController(MisProjectManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetLatestConfig()
        {
            // Lấy config mới nhất theo MonthYear (chuỗi mm/yyyy)
            var latest = await _context.SystemConfigs
                .AsNoTracking()
                .OrderByDescending(c => c.MonthYear)
                .FirstOrDefaultAsync();

            if (latest == null)
            {
                return Ok(new
                {
                    month_year = "06/2026",
                    standard_working_hours = 176,
                    penalty_factor = "{\"penalty_1_2_days\":0.8,\"penalty_3_5_days\":0.5,\"penalty_over_5_days\":0.0}",
                    // Holidays dạng int trong DB, FE hiện đã được vô hiệu hóa UI Holidays nên không cần map
                    holidays = latest?.Holidays
                });
            }

            return Ok(new
            {
                month_year = latest.MonthYear,
                standard_working_hours = latest.StandardWorkingHours,
                penalty_factor = latest.PenaltyFactor,
                holidays = latest.Holidays
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveConfig([FromBody] SystemConfigDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MonthYear)) return BadRequest("Thiếu MonthYear.");

            var existingConfig = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.MonthYear == dto.MonthYear);

            if (existingConfig != null)
            {
                existingConfig.StandardWorkingHours = dto.StandardWorkingHours;
                existingConfig.Holidays = dto.Holidays;
                existingConfig.PenaltyFactor = dto.PenaltyFactor;
                _context.SystemConfigs.Update(existingConfig);
            }
            else
            {
                var newConfig = new SystemConfig
                {
                    MonthYear = dto.MonthYear,
                    StandardWorkingHours = dto.StandardWorkingHours,
                    Holidays = dto.Holidays,
                    PenaltyFactor = dto.PenaltyFactor
                };
                _context.SystemConfigs.Add(newConfig);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Thiết lập kỳ đánh giá {dto.MonthYear} thành công!" });
        }
    }
}
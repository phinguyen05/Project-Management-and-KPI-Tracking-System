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
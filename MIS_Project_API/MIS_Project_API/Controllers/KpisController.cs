using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.Interfaces;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KpisController : ControllerBase
    {
        private readonly IKpiService _kpiService;

        public KpisController(IKpiService kpiService)
        {
            _kpiService = kpiService;
        }

        [HttpGet("timeliness/{userId}")]
        public async Task<ActionResult> GetKpiTimeliness(int userId, [FromQuery] int month, [FromQuery] int year)
        {
            var kpi = await _kpiService.CalculateKpiTimelinessAsync(userId, month, year);
            return Ok(new { 
                UserId = userId, 
                Month = month, 
                Year = year, 
                KpiTimeliness = kpi,
                Message = "Đã áp dụng rule Cut-off Date và Division by zero."
            });
        }
    }
}

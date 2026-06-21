using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.Interfaces;
using System.Security.Claims; // Cần thiết để lấy thông tin từ Token

namespace MIS_Project_API.Controllers
{
    public class FinalizeKpiRequest
    {
        public double ManagerScore { get; set; }
        public string Note { get; set; } = string.Empty;
    }

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

        // Hàm hỗ trợ check quyền
        private bool IsAuthorizedToView(int targetUserId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                if (currentUserId == targetUserId) return true; // Chính chủ
            }

            // Hoặc là Manager/Admin
            return User.IsInRole("Manager") || User.IsInRole("Admin");
        }

        [HttpGet("timeliness/{userId}")]
        public async Task<ActionResult> GetKpiTimeliness(int userId, [FromQuery] int month, [FromQuery] int year)
        {
            if (!IsAuthorizedToView(userId)) return Forbid("Bạn không có quyền xem KPI của người khác.");

            var kpi = await _kpiService.CalculateKpiTimelinessAsync(userId, month, year);
            return Ok(new { UserId = userId, Month = month, Year = year, KpiTimeliness = kpi });
        }

        [HttpGet("efficiency/{userId}")]
        public async Task<ActionResult> GetKpiEfficiency(int userId, [FromQuery] int month, [FromQuery] int year)
        {
            if (!IsAuthorizedToView(userId)) return Forbid("Bạn không có quyền xem KPI của người khác.");

            var kpi = await _kpiService.CalculateKpiEfficiencyAsync(userId, month, year);
            return Ok(new { UserId = userId, Month = month, Year = year, KpiEfficiency = kpi });
        }

        [HttpGet("capacity/{userId}")]
        public async Task<ActionResult> GetKpiCapacity(int userId, [FromQuery] int month, [FromQuery] int year)
        {
            if (!IsAuthorizedToView(userId)) return Forbid("Bạn không có quyền xem KPI của người khác.");

            var kpi = await _kpiService.CalculateKpiCapacityAsync(userId, month, year);
            return Ok(new { UserId = userId, Month = month, Year = year, KpiCapacity = kpi });
        }

        [HttpPost("finalize/{userId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> FinalizeKpi(int userId, [FromQuery] int month, [FromQuery] int year, [FromBody] FinalizeKpiRequest request)
        {
            var result = await _kpiService.FinalizeKpiReviewAsync(userId, month, year, request.ManagerScore, request.Note);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(new
            {
                Message = result.Message,
                ReviewId = result.Data?.ReviewId,
                Timeliness = result.Data?.KpiTimeliness,
                Efficiency = result.Data?.KpiEfficiency,
                Capacity = result.Data?.KpiCapacity,
                ManagerScore = result.Data?.KpiManagerEvaluation,
                FinalScore = result.Data?.FinalKpiScore,
                IsOverloaded = result.Data?.IsOverloaded
            });
        }
    }
}
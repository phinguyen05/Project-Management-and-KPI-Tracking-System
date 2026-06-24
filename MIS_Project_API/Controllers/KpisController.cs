using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;
using System.Security.Claims; // Cần thiết để lấy thông tin từ Token
using Microsoft.EntityFrameworkCore;

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
        private readonly MisProjectManagementContext _context;

        public KpisController(IKpiService kpiService, MisProjectManagementContext context)
        {
            _kpiService = kpiService;
            _context = context;
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

        [HttpGet("summary")] // GET /api/kpis/summary?month=6&year=2026
        public async Task<ActionResult> GetKpiSummary([FromQuery] int month, [FromQuery] int year)
        {
            // Quy đổi tạm thời month/year => cycle_id
            // Theo yêu cầu: month=6&year=2026 => cycle_id = 1
            int cycleId = 1;

            // Bước 1: Chỉ khai báo câu Query (không gọi ToListAsync ở đây)
            var query = from u in _context.Users
                        where u.Role == "Employee"
                        join k in _context.KpiReviews.Where(kr => kr.CycleId == cycleId)
                            on u.UserId equals k.UserId into kpiGroup
                        from k in kpiGroup.DefaultIfEmpty() // Left Join chuẩn
                        select new
                        {
                            userId = u.UserId,
                            userName = u.FullName,
                            // Nếu chưa chốt thì giả lập điểm tiến độ, nếu chốt rồi thì lấy từ DB
                            timeliness = k != null ? k.KpiTimeliness : 90,
                            efficiency = k != null ? k.KpiEfficiency : 80,
                            capacity = k != null ? k.KpiCapacity : 100,
                            managerScore = k != null ? k.KpiManagerEvaluation : 0,
                            status = k != null ? "Closed" : "Draft"
                        };

            // Bước 2: Thực thi Query một cách độc lập
            var result = await query.ToListAsync();

            return Ok(result);
        }

        // GET /api/kpis/me?month=6&year=2026
        [HttpGet("me")]
        public async Task<ActionResult> GetMyKpis([FromQuery] int month, [FromQuery] int year)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            // Theo logic hiện tại của KpiService, KPI của các thành phần đều có thể tính trực tiếp từ DB.
            // Manager score lấy từ KpiReview nếu đã finalize; nếu chưa thì trả về 0.
            double timeliness = await _kpiService.CalculateKpiTimelinessAsync(currentUserId, month, year);
            double efficiency = await _kpiService.CalculateKpiEfficiencyAsync(currentUserId, month, year);
            double capacity = await _kpiService.CalculateKpiCapacityAsync(currentUserId, month, year);

            // Quy đổi month/year => cycle_id hiện đang được map tạm trong controller summary/finalize (cycleId=1)
            // Giữ nhất quán theo hệ thống hiện tại.
            int cycleId = 1;
            var review = await _context.KpiReviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == currentUserId && r.CycleId == cycleId);

            double managerEvaluation = review?.KpiManagerEvaluation ?? 0;

            return Ok(new
            {
                userId = currentUserId,
                month,
                year,
                kpi_timeliness = timeliness,
                kpi_efficiency = efficiency,
                kpi_capacity = capacity,
                kpi_manager_evaluation = managerEvaluation
            });
        }


        [HttpPost("finalize/{userId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> FinalizeKpi(int userId, [FromQuery] int month, [FromQuery] int year, [FromBody] FinalizeKpiRequest request)
        {
            // Quy đổi tạm thời month/year => cycle_id
            // Theo yêu cầu: month=6&year=2026 => cycle_id = 1
            int cycleId = 1;

            // UPSERT để tránh rác dữ liệu (trùng userId + cycleId)
            var existing = await _context.KpiReviews
                .FirstOrDefaultAsync(kr => kr.UserId == userId && kr.CycleId == cycleId);

            if (existing != null)
            {
                existing.KpiManagerEvaluation = request.ManagerScore;
                // Nếu model có cột Note/ManagerNote/...
                // Ta cập nhật nếu tồn tại property; nếu không compile thì bạn sẽ cần map lại theo đúng property trong project.
                // Vì hiện tại chỉ chắc chắn về KpiManagerEvaluation theo mapping yêu cầu.

                // Cố gắng cập nhật trường Note nếu entity có
                var noteProp = existing.GetType().GetProperty("Note");
                if (noteProp != null && noteProp.CanWrite)
                {
                    noteProp.SetValue(existing, request.Note);
                }

                _context.KpiReviews.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Đã cập nhật KPI." ,
                    ReviewId = existing.ReviewId,
                    Timeliness = existing.KpiTimeliness,
                    Efficiency = existing.KpiEfficiency,
                    Capacity = existing.KpiCapacity,
                    ManagerScore = existing.KpiManagerEvaluation,
                    // FinalScore/IsOverloaded có thể được tính trong service; ở đây trả về null nếu entity không có
                    FinalScore = existing.GetType().GetProperty("FinalKpiScore")?.GetValue(existing),
                    IsOverloaded = existing.GetType().GetProperty("IsOverloaded")?.GetValue(existing)
                });
            }
            else
            {
                // Nếu chưa có thì tạo mới theo entity của bạn.
                var entity = new KpiReview
                {
                    UserId = userId,
                    CycleId = cycleId,
                    KpiManagerEvaluation = request.ManagerScore,
                    // KpiTimeliness/KpiEfficiency/KpiCapacity nếu chưa có thì để mặc định (0) hoặc framework sẽ tính.
                    // Nếu bạn muốn đảm bảo có đủ số liệu, cần bổ sung bước tính từ DB/service.
                };

                var noteProp = entity.GetType().GetProperty("Note");
                if (noteProp != null && noteProp.CanWrite)
                {
                    noteProp.SetValue(entity, request.Note);
                }

                _context.KpiReviews.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Đã chốt KPI mới." ,
                    ReviewId = entity.ReviewId,
                    Timeliness = entity.KpiTimeliness,
                    Efficiency = entity.KpiEfficiency,
                    Capacity = entity.KpiCapacity,
                    ManagerScore = entity.KpiManagerEvaluation,
                    FinalScore = entity.GetType().GetProperty("FinalKpiScore")?.GetValue(entity),
                    IsOverloaded = entity.GetType().GetProperty("IsOverloaded")?.GetValue(entity)
                });
            }
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Models;

namespace MIS_Project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class AuditLogsController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;

        public AuditLogsController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // GET /api/auditlogs
        [HttpGet]
        public async Task<IActionResult> GetAllAuditLogs()
        {
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    logId = l.LogId,
                    createdAt = l.CreatedAt,
                    userName = l.User != null ? l.User.FullName : null,
                    action = l.Action,
                    targetTable = l.TargetTable,
                    details = l.Details
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}


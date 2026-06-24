using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.DTOs.Task;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;
using Microsoft.EntityFrameworkCore;

namespace MIS_Project_API.Controllers
{
    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = null!;
    }

    public class RiskApprovalRequest
    {
        public bool Approved { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly MisProjectManagementContext _context;


        public TasksController(ITaskService taskService, MisProjectManagementContext context)
        {
            _taskService = taskService;
            _context = context;
        }

        private int? GetCurrentUserId()
        {

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim)) return null;
            if (!int.TryParse(userIdClaim, out var userId)) return null;
            return userId;
        }

        private string? GetCurrentRoleClaim()
        {
            // Hệ thống hiện tại dùng role string để quyết định policy trong TaskService
            return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        }


        // NOTE: Controller không cần constructor đơn tham số nữa. Dùng DI constructor có MisProjectManagementContext.


        [HttpGet("project/{projectId}/wbs")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetWbs(int projectId)
        {
            var userId = GetCurrentUserId();
            var roleClaim = GetCurrentRoleClaim();

            if (userId != null && roleClaim != null)
            {
                var wbs = await _taskService.GetProjectTasksWbsForUserAsync(projectId, userId.Value, roleClaim);
                return Ok(wbs);
            }

            var defaultWbs = await _taskService.GetProjectTasksWbsAsync(projectId);
            return Ok(defaultWbs);
        }

        // GET /api/tasks/me
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
        {
            var userId = GetCurrentUserId();
            var roleClaim = GetCurrentRoleClaim();

            if (userId == null || string.IsNullOrWhiteSpace(roleClaim)) return Unauthorized();

            // Trả về tasks được gán cho chính user này.
            // Hiện tại front-end đang dùng format phẳng và lọc theo status.
            // Truy vấn trực tiếp DB để tránh ràng buộc thiếu method trong ITaskService.
            // Trả về danh sách TaskDto dạng phẳng (không lồng tree WBS) để FE render ổn định.
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Where(t => t.AssigneeId == userId.Value)
                .Include(t => t.Assignee)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    ProjectId = t.ProjectId,
                    ParentTaskId = t.ParentTaskId,
                    Name = t.Name,
                    Description = t.Description,
                    AssigneeId = t.AssigneeId ?? 0,
                    AssigneeName = t.Assignee != null ? t.Assignee.FullName : "Chưa giao việc",
                    Status = t.Status,
                    EstimatedTime = t.EstimatedTime ?? 0,
                    StartDate = t.StartDate.HasValue ? t.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    DueDate = t.Deadline,
                    RiskFlag = t.RiskFlag ?? false,
                    SubTasks = new List<TaskDto>()
                })
                .ToListAsync();

            return Ok(tasks);


        }


        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto createDto)
        {
            var task = await _taskService.CreateTaskAsync(createDto);
            return Ok(task);
        }

        [HttpPut("{id}/dates")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> UpdateDates(int id, [FromBody] UpdateTaskDateDto updateDto)
        {
            var success = await _taskService.UpdateTaskDatesAndShiftChildrenAsync(id, updateDto);
            if (!success) return NotFound("Không tìm thấy Task hoặc Task chưa có ngày bắt đầu.");
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest updateDto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int currentUserId);
            bool isManager = User.IsInRole("Manager") || User.IsInRole("Admin");

            var result = await _taskService.UpdateTaskStatusAsync(id, updateDto.Status, currentUserId, isManager);

            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy") || result.Message.Contains("quyền")) return Forbid(result.Message);
                return BadRequest(result.Message);
            }
            return Ok(new { message = result.Message });
        }

        [HttpPatch("{id}/risk-approval")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> SetRiskApproval(int id, [FromBody] RiskApprovalRequest request)
        {
            var result = await _taskService.SetRiskApprovalAsync(id, request.Approved);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(new { message = result.Message });
        }

        // PUT /api/tasks/{id} - update task (tên, assignee, status, deadline/dueDate...)
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequestDto dto)
        {
            var taskEntity = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == id);
            if (taskEntity == null) return NotFound();

            // Update các trường quan trọng theo yêu cầu
            taskEntity.Name = dto.Name;
            taskEntity.AssigneeId = dto.AssigneeId;
            taskEntity.Status = dto.Status;
            taskEntity.Description = dto.Description;
            if (dto.EstimatedTime.HasValue) taskEntity.EstimatedTime = dto.EstimatedTime.Value;
            if (dto.StartDate.HasValue) taskEntity.StartDate = DateOnly.FromDateTime(dto.StartDate.Value);
            if (dto.dueDate.HasValue) taskEntity.Deadline = dto.dueDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteTaskCascadeAsync(id);
            if (!result.Success) return NotFound(result.Message);
            return NoContent();
        }
    }
}

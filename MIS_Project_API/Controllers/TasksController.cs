using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.DTOs.Task;
using MIS_Project_API.Interfaces;

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

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("project/{projectId}/wbs")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetWbs(int projectId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userIdClaim != null && roleClaim != null)
            {
                int userId = int.Parse(userIdClaim);
                var wbs = await _taskService.GetProjectTasksWbsForUserAsync(projectId, userId, roleClaim);
                return Ok(wbs);
            }

            var defaultWbs = await _taskService.GetProjectTasksWbsAsync(projectId);
            return Ok(defaultWbs);
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

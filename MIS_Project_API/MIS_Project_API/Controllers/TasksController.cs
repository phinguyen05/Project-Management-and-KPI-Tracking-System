using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.DTOs.Task;
using MIS_Project_API.Interfaces;

namespace MIS_Project_API.Controllers
{
    // Dinh nghia lop model ngay trong controller de khong lam rac thu muc DTO
    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = null!;
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
            var wbs = await _taskService.GetProjectTasksWbsAsync(projectId);
            return Ok(wbs);
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
            var result = await _taskService.UpdateTaskStatusAsync(id, updateDto.Status);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy")) return NotFound(result.Message);
                return BadRequest(result.Message); 
            }
            return Ok(new { message = result.Message });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS_Project_API.DTOs.Project;
using MIS_Project_API.Interfaces;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu có JWT Token mới được gọi
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userIdClaim != null && roleClaim != null)
            {
                int userId = int.Parse(userIdClaim);
                var projects = await _projectService.GetProjectsForUserAsync(userId, roleClaim);
                return Ok(projects);
            }

            var allProjects = await _projectService.GetAllProjectsAsync();
            return Ok(allProjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetById(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound("Không tìm thấy dự án.");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userIdClaim != null && roleClaim == "Employee")
            {
                int userId = int.Parse(userIdClaim);
                var userProjects = await _projectService.GetProjectsForUserAsync(userId, roleClaim);
                if (!userProjects.Any(p => p.ProjectId == id))
                {
                    return StatusCode(403, "Bạn không có quyền truy cập thông tin dự án này.");
                }
            }

            return Ok(project);
        }

        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto createDto)
        {
            var createdProject = await _projectService.CreateProjectAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdProject.ProjectId }, createdProject);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] UpdateProjectDto updateDto)
        {
            var updatedProject = await _projectService.UpdateProjectAsync(id, updateDto);
            if (updatedProject == null) return NotFound("Không tìm thấy dự án để cập nhật.");
            return Ok(updatedProject);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var isDeleted = await _projectService.DeleteProjectAsync(id);
            if (!isDeleted) return NotFound("Không tìm thấy dự án để xóa.");
            return NoContent();
        }
    }
}

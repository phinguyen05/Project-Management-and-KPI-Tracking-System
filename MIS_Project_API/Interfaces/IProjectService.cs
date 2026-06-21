using MIS_Project_API.DTOs.Project;

namespace MIS_Project_API.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<IEnumerable<ProjectDto>> GetProjectsForUserAsync(int userId, string role);
        Task<ProjectDto?> GetProjectByIdAsync(int id);
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto);
        Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto updateDto);
        Task<bool> DeleteProjectAsync(int id);
    }
}
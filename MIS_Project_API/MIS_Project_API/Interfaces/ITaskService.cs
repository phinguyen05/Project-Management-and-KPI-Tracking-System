using MIS_Project_API.DTOs.Task;

namespace MIS_Project_API.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetProjectTasksWbsAsync(int projectId);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto);
        Task<bool> UpdateTaskDatesAndShiftChildrenAsync(int taskId, UpdateTaskDateDto updateDto);
        Task<(bool Success, string Message)> UpdateTaskStatusAsync(int taskId, string newStatus, int currentUserId, bool isManager);
        Task<(bool Success, string Message)> SetRiskApprovalAsync(int taskId, bool approved);
    }
}

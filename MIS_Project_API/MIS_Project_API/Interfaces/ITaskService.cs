using MIS_Project_API.DTOs.Task;

namespace MIS_Project_API.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetProjectTasksWbsAsync(int projectId);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto);

        // Bản hợp đồng công việc mới: Cập nhật ngày và kích hoạt Domino đệ quy
        Task<bool> UpdateTaskDatesAndShiftChildrenAsync(int taskId, UpdateTaskDateDto updateDto);
    }
}
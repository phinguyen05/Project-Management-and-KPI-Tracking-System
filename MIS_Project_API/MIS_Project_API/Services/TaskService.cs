using Microsoft.EntityFrameworkCore;
using MIS_Project_API.DTOs.Task;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;
using DbTask = MIS_Project_API.Models.Task;

namespace MIS_Project_API.Services
{
    public class TaskService : ITaskService
    {
        private readonly MisProjectManagementContext _context;

        public TaskService(MisProjectManagementContext context)
        {
            _context = context;
        }

        // 1. THUẬT TOÁN WBS
        public async Task<IEnumerable<TaskDto>> GetProjectTasksWbsAsync(int projectId)
        {
            var allTasks = await _context.Tasks
                .Include(t => t.Assignee)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            var allTaskDtos = allTasks.Select(t => new TaskDto
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
            }).ToList();

            var taskDictionary = allTaskDtos.ToDictionary(t => t.TaskId);
            var rootTasks = new List<TaskDto>();

            foreach (var task in allTaskDtos)
            {
                if (task.ParentTaskId.HasValue && taskDictionary.ContainsKey(task.ParentTaskId.Value))
                {
                    taskDictionary[task.ParentTaskId.Value].SubTasks.Add(task);
                }
                else
                {
                    rootTasks.Add(task);
                }
            }

            return rootTasks;
        }

        // 2. TẠO TASK MỚI
        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto)
        {
            var taskModel = new DbTask
            {
                ProjectId = createDto.ProjectId,
                ParentTaskId = createDto.ParentTaskId,
                Name = createDto.Name,
                Description = createDto.Description,
                AssigneeId = createDto.AssigneeId,
                EstimatedTime = createDto.EstimatedTime,
                StartDate = createDto.StartDate.HasValue ? DateOnly.FromDateTime(createDto.StartDate.Value) : null,
                Deadline = createDto.DueDate,
                Status = "To_Do",
                RiskFlag = false
            };

            await _context.Tasks.AddAsync(taskModel);
            await _context.SaveChangesAsync();

            var assignee = await _context.Users.FindAsync(taskModel.AssigneeId);

            return new TaskDto
            {
                TaskId = taskModel.TaskId,
                ProjectId = taskModel.ProjectId,
                ParentTaskId = taskModel.ParentTaskId,
                Name = taskModel.Name,
                Description = taskModel.Description,
                AssigneeId = taskModel.AssigneeId ?? 0,
                AssigneeName = assignee != null ? assignee.FullName : "Chưa giao việc",
                Status = taskModel.Status,
                EstimatedTime = taskModel.EstimatedTime ?? 0,
                StartDate = taskModel.StartDate.HasValue ? taskModel.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                DueDate = taskModel.Deadline,
                RiskFlag = taskModel.RiskFlag ?? false
            };
        }

        // 3. CẬP NHẬT NGÀY & KÍCH HOẠT HIỆU ỨNG DOMINO
        public async Task<bool> UpdateTaskDatesAndShiftChildrenAsync(int taskId, UpdateTaskDateDto updateDto)
        {
            var rootTask = await _context.Tasks.FindAsync(taskId);
            if (rootTask == null || !rootTask.StartDate.HasValue) return false;

            DateTime oldStartDateTime = rootTask.StartDate.Value.ToDateTime(TimeOnly.MinValue);
            int offsetDays = (int)(updateDto.NewStartDate.Date - oldStartDateTime.Date).TotalDays;

            rootTask.StartDate = DateOnly.FromDateTime(updateDto.NewStartDate);
            rootTask.Deadline = updateDto.NewDueDate;

            if (offsetDays != 0)
            {
                var allProjectTasks = await _context.Tasks.Where(t => t.ProjectId == rootTask.ProjectId).ToListAsync();
                var taskIds = allProjectTasks.Select(t => t.TaskId).ToList();
                var allDependencies = await _context.TaskDependencies.Where(d => taskIds.Contains(d.PredecessorTaskId)).ToListAsync();

                ShiftSuccessorTasksRecursively(allProjectTasks, allDependencies, taskId, offsetDays, new HashSet<int>());
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private void ShiftSuccessorTasksRecursively(List<DbTask> allTasks, List<TaskDependency> allDependencies, int currentTaskId, int offsetDays, HashSet<int> visited)
        {
            if (!visited.Add(currentTaskId)) return;

            var successorIds = allDependencies
                .Where(d => d.PredecessorTaskId == currentTaskId && d.DependencyType == "Finish_to_Start")
                .Select(d => d.SuccessorTaskId);

            foreach (var successorId in successorIds)
            {
                var successor = allTasks.FirstOrDefault(t => t.TaskId == successorId);
                if (successor == null) continue;

                if (successor.StartDate.HasValue)
                    successor.StartDate = DateOnly.FromDateTime(successor.StartDate.Value.ToDateTime(TimeOnly.MinValue).AddDays(offsetDays));

                if (successor.Deadline.HasValue)
                    successor.Deadline = successor.Deadline.Value.AddDays(offsetDays);

                ShiftSuccessorTasksRecursively(allTasks, allDependencies, successorId, offsetDays, visited);
            }
        }

        // 4. CẬP NHẬT TRẠNG THÁI TASK & KIỂM TRA LOG-TIME
        public async Task<(bool Success, string Message)> UpdateTaskStatusAsync(int taskId, string newStatus)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return (false, "Không tìm thấy Task.");

            var validStatuses = new[] { "To_Do", "Doing", "Done", "Risk" };
            if (!validStatuses.Contains(newStatus))
                return (false, "Trạng thái không hợp lệ.");

            if (newStatus == "Done")
            {
                bool hasLogTime = await _context.LogTimes.AnyAsync(lt => lt.TaskId == taskId);
                if (!hasLogTime)
                {
                    return (false, "Lỗi: Bắt buộc phải báo cáo giờ làm thực tế (Log-time) trước khi đánh dấu Hoàn thành (Done).");
                }
                task.CompletedAt = DateTime.Now;
            }
            else
            {
                task.CompletedAt = null;
            }

            task.Status = newStatus;
            await _context.SaveChangesAsync();
            return (true, "Cập nhật trạng thái thành công.");
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.DTOs.Project;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;

namespace MIS_Project_API.Services
{
    public class ProjectService : IProjectService
    {
        private readonly MisProjectManagementContext _context;
        private readonly IMapper _mapper;

        public ProjectService(MisProjectManagementContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            // Include Manager để lấy được tên người quản lý khi map ra DTO
            var projects = await _context.Projects.Include(p => p.Manager).ToListAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsForUserAsync(int userId, string role)
        {
            IQueryable<Project> query = _context.Projects.Include(p => p.Manager);

            if (role == "Employee")
            {
                // Chỉ lấy ra những Project mà nhân sự đó có ít nhất một Task được giao
                query = query.Where(p => p.Tasks.Any(t => t.AssigneeId == userId));
            }

            var projects = await query.ToListAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects.Include(p => p.Manager).FirstOrDefaultAsync(p => p.ProjectId == id);
            if (project == null) return null;
            return _mapper.Map<ProjectDto>(project);
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto)
        {
            // --- CÁCH LY AUTOMAPPER: CODE CHAY ĐỂ TEST LÕI DATABASE ---
            var projectModel = new Project
            {
                Name = createDto.Name,
                Description = createDto.Description,
                ManagerId = createDto.ManagerId,

                // 1. Ép kiểu từ DateTime? (DTO) sang DateOnly? (Model)
                StartDate = createDto.StartDate.HasValue
                            ? DateOnly.FromDateTime(createDto.StartDate.Value)
                            : null,

                // 2. Dùng đúng tên cột Deadline của Database
                Deadline = createDto.EndDate.HasValue
                           ? DateOnly.FromDateTime(createDto.EndDate.Value)
                           : null,

                Status = "On_Time" // Mặc định dự án mới là On_Time
            };

            // Lưu vào DB
            await _context.Projects.AddAsync(projectModel);
            await _context.SaveChangesAsync();

            // Tìm thông tin Manager để lấy cái Tên trả ra cho đẹp
            var manager = await _context.Users.FindAsync(projectModel.ManagerId);

            // Trả về DTO (Map tay từ Model ra DTO)
            return new ProjectDto
            {
                ProjectId = projectModel.ProjectId,
                Name = projectModel.Name,
                Description = projectModel.Description,

                // 3. Đảm bảo ManagerId không bị null (dùng ?? 0)
                ManagerId = projectModel.ManagerId ?? 0,
                ManagerName = manager != null ? manager.FullName : "N/A",

                // 4. Ép ngược lại từ DateOnly? (Model) ra DateTime? (DTO) cho Frontend
                StartDate = projectModel.StartDate.HasValue
                            ? projectModel.StartDate.Value.ToDateTime(TimeOnly.MinValue)
                            : null,

                EndDate = projectModel.Deadline.HasValue
                          ? projectModel.Deadline.Value.ToDateTime(TimeOnly.MinValue)
                          : null,

                Status = projectModel.Status
            };
        }

        public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto updateDto)
        {
            var projectModel = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
            if (projectModel == null) return null;

            _mapper.Map(updateDto, projectModel);
            await _context.SaveChangesAsync();

            return await GetProjectByIdAsync(id) ?? _mapper.Map<ProjectDto>(projectModel);
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var projectModel = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
            if (projectModel == null) return false;

            // Xóa toàn bộ Task thuộc Project trước để tránh ràng buộc khóa ngoại.
            var taskIds = await _context.Tasks
                .Where(t => t.ProjectId == id)
                .Select(t => t.TaskId)
                .ToListAsync();

            // Duyệt theo từng task root để cascade đúng quan hệ cây.
            // (DeleteTaskCascadeAsync sẽ tự lấy toàn bộ sub-task.)
            foreach (var taskId in taskIds)
            {
                // Nếu đã xóa do cascade từ task khác thì bỏ qua.
                var exists = await _context.Tasks.AnyAsync(t => t.TaskId == taskId);
                if (!exists) continue;

                // Cascade delete trực tiếp theo cây và dữ liệu liên quan.
                // Lưu ý: sử dụng DbContext để tránh phụ thuộc ITaskService.

                var idsToDelete = new List<int>();
                var queue = new Queue<int>();
                queue.Enqueue(taskId);

                while (queue.Count > 0)
                {
                    var currentId = queue.Dequeue();
                    if (idsToDelete.Contains(currentId)) continue;
                    idsToDelete.Add(currentId);

                    var childrenIds = await _context.Tasks
                        .Where(t => t.ParentTaskId == currentId)
                        .Select(t => t.TaskId)
                        .ToListAsync();

                    foreach (var childId in childrenIds)
                    {
                        if (!idsToDelete.Contains(childId)) queue.Enqueue(childId);
                    }
                }

                var depsToDelete = await _context.TaskDependencies
                    .Where(d => idsToDelete.Contains(d.PredecessorTaskId) || idsToDelete.Contains(d.SuccessorTaskId))
                    .ToListAsync();
                if (depsToDelete.Count > 0) _context.TaskDependencies.RemoveRange(depsToDelete);

                var logTimesToDelete = await _context.LogTimes
                    .Where(lt => idsToDelete.Contains(lt.TaskId))
                    .ToListAsync();
                if (logTimesToDelete.Count > 0) _context.LogTimes.RemoveRange(logTimesToDelete);

                var commentsToDelete = await _context.Comments
                    .Where(c => idsToDelete.Contains(c.TaskId))
                    .ToListAsync();
                if (commentsToDelete.Count > 0) _context.Comments.RemoveRange(commentsToDelete);

                var extensionRequestsToDelete = await _context.ExtensionRequests
                    .Where(er => idsToDelete.Contains(er.TaskId))
                    .ToListAsync();
                if (extensionRequestsToDelete.Count > 0) _context.ExtensionRequests.RemoveRange(extensionRequestsToDelete);

                var tasksToDelete = await _context.Tasks
                    .Where(t => idsToDelete.Contains(t.TaskId))
                    .ToListAsync();
                if (tasksToDelete.Count > 0) _context.Tasks.RemoveRange(tasksToDelete);

                await _context.SaveChangesAsync();
            }

            // Xóa tài liệu dự án
            var projectDocs = await _context.ProjectDocuments.Where(d => d.ProjectId == id).ToListAsync();
            if (projectDocs.Count > 0) _context.ProjectDocuments.RemoveRange(projectDocs);

            // Xóa bảng trung gian Project_Client (many-to-many) để tránh xung đột khóa ngoại
            // Entity này không có DbSet trong code, nên dùng FromSql để tác động trực tiếp.
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM Project_Client WHERE project_id = {0}",
                id
            );

            _context.Projects.Remove(projectModel);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
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

            _context.Projects.Remove(projectModel);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
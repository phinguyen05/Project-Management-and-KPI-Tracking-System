using System.ComponentModel.DataAnnotations;

namespace MIS_Project_API.DTOs.Project
{
    public class ProjectDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = null!; // Lấy thêm tên Quản lý cho Frontend dễ hiển thị
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace MIS_Project_API.DTOs.Project
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Tên dự án không được để trống")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Phải chỉ định người quản lý dự án")]
        public int ManagerId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
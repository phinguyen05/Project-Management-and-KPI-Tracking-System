using System.ComponentModel.DataAnnotations;

namespace MIS_Project_API.DTOs.Project
{
    public class UpdateProjectDto
    {
        [Required(ErrorMessage = "Tên dự án không được để trống")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
    }
}
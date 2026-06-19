using System.ComponentModel.DataAnnotations;

namespace MIS_Project_API.DTOs.Task
{
    public class CreateTaskDto
    {
        [Required]
        public int ProjectId { get; set; }

        public int? ParentTaskId { get; set; } // Truyền null nếu là việc lớn, truyền ID nếu là việc con

        [Required(ErrorMessage = "Tên công việc không được để trống")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Phải chỉ định người thực hiện")]
        public int AssigneeId { get; set; }

        public double EstimatedTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
namespace MIS_Project_API.DTOs.Task
{
    public class TaskDto
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }

        // Cột sống của WBS: Lưu ID của Task cha
        public int? ParentTaskId { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int AssigneeId { get; set; }
        public string AssigneeName { get; set; } = null!; // Tên người thực hiện
        public string? Status { get; set; }
        public double EstimatedTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool RiskFlag { get; set; }

        // DANH SÁCH TASK CON: Tự động lồng ghép thành dạng cây đệ quy
        public List<TaskDto> SubTasks { get; set; } = new List<TaskDto>();
    }
}
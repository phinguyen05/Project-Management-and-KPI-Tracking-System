namespace MIS_Project_API.DTOs.LogTime
{
    public class CreateLogTimeDto
    {
        public int TaskId { get; set; }
        public double ActualHours { get; set; }
        public string? Description { get; set; }
        public DateTime LogDate { get; set; }
    }

    public class ApproveLogTimeDto
    {
        public string Status { get; set; } = null!; // "Approved" hoặc "Rejected"
    }
}
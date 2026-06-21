namespace MIS_Project_API.DTOs.LeaveDay
{
    public class CreateLeaveDayDto
    {
        public DateTime LeaveDate { get; set; }
        public string Reason { get; set; } = null!;
    }

    public class ApproveLeaveDayDto
    {
        public string Status { get; set; } = null!; // "Approved" hoặc "Rejected"
    }
}
namespace MIS_Project_API.DTOs
{
    public class SystemConfigDto
    {
        public required string MonthYear { get; set; }
        public int StandardWorkingHours { get; set; }
        public int Holidays { get; set; }
        public required string PenaltyFactor { get; set; }
    }
}
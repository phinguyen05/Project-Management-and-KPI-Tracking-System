namespace MIS_Project_API.Models
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty; // Hoặc Email tùy thiết kế của bạn
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
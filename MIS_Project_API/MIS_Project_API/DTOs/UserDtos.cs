public class CreateUserDto
{
    public required string FullName { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; } // Admin, Manager, Employee, C_Level, Client
    public int? DepartmentId { get; set; }
}

public class UpdateUserDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public int? DepartmentId { get; set; }
    public required string Status { get; set; } // Active, Locked
}

public class UserResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
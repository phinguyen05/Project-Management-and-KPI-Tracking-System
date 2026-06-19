using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MIS_Project_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MisProjectManagementContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(MisProjectManagementContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. API Đăng ký (CHỈ dùng để tạo tài khoản Admin đầu tiên, tự khóa sau khi đã có user)
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            bool hasAnyUser = _context.Users.Any();
            if (hasAnyUser)
            {
                return StatusCode(403, "Hệ thống đã có Admin. Vui lòng dùng chức năng 'Thêm nhân sự' trong trang Quản trị.");
            }

            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest("Tài khoản đã tồn tại!");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Role = "Admin",
                Status = "Active"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "Tạo tài khoản Admin thành công!" });
        }

        // 2. API Đăng nhập kết nối Database
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "Employee"),
                new Claim("FullName", user.FullName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

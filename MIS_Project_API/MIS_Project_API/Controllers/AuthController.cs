using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MIS_Project_API.Models; // Gọi đến thư mục Models chứa User, LoginRequest, RegisterRequest
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

        // Tiêm DbContext (để gọi DB) và IConfiguration (để lấy Key JWT)
        public AuthController(MisProjectManagementContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. API Đăng ký (Dùng để tạo tài khoản Admin đầu tiên)
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra xem username đã tồn tại chưa
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest("Tài khoản đã tồn tại!");
            }

            // Băm mật khẩu bằng BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo đối tượng User mới
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,     // Lưu mật khẩu đã bị băm
                FullName = request.FullName,
                Role = "Admin",              // Gắn cứng quyền Admin cho tài khoản đầu tiên
                Status = "Active"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges(); // Lưu xuống SQL Server

            return Ok(new { message = "Tạo tài khoản Admin thành công!" });
        }

        // 2. API Đăng nhập kết nối Database
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Tìm user trong DB theo Username
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            // Nếu không tìm thấy user, HOẶC mật khẩu băm không khớp -> Từ chối
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
            }

            // Nếu đúng thông tin, tiến hành sinh JWT Token
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
        }

        // Hàm hỗ trợ sinh Token
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Nhét thông tin cá nhân (Claims) vào token để các API sau phân quyền
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // ID của user
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "Employee"),          // Quyền của user
                new Claim("FullName", user.FullName ?? "")                    // Tên hiển thị
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Cho phép Token sống 8 tiếng (1 ngày làm việc)
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
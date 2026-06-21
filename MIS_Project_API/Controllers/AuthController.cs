using Microsoft.AspNetCore.Authorization;
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

            // BỔ SUNG ĐOẠN NÀY
            if (user.Status == "Locked") // Hoặc IsActive == false tùy cách bạn thiết kế DB
            {
                return StatusCode(403, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
        }

        // 3. API Đổi mật khẩu
        [HttpPost("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound("Không tìm thấy người dùng.");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                return BadRequest("Mật khẩu cũ không chính xác.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.SaveChanges();

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        // 4. API Quên mật khẩu
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                // Để bảo mật không nên để lộ email tồn tại hay không, nhưng ở đây dễ test
                return NotFound("Email không tồn tại trong hệ thống.");
            }

            // Tạo mã OTP ngẫu nhiên 6 chữ số
            var otp = new Random().Next(100000, 999999).ToString();
            Console.WriteLine($"[FORGOT PASSWORD] Email: {request.Email} - OTP: {otp}");

            return Ok(new { 
                message = "Hệ thống đã gửi OTP qua email (In ra console log của server).", 
                otp = otp // Trả về luôn để dễ test/automation
            });
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

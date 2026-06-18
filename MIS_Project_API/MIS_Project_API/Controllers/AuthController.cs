using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MIS_Project_API.DTOs;

namespace MIS_Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // TODO: Kết nối với Database qua Entity Framework để kiểm tra Username/Password thực tế.
            // Tạm thời dùng Mock Data để test luồng sinh Token:
            if (request.Username == "admin_user" && request.Password == "123456")
            {
                var token = GenerateJwtToken(request.Username, "Admin"); // Chỗ này sau sẽ lấy Role thực từ DB
                return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
            }

            return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");
        }

        private string GenerateJwtToken(string username, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Cấu hình các "Claim" (thông tin đi kèm Token như Tên, Quyền)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2), // Token sống trong 2 giờ
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
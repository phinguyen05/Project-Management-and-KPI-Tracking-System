using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_Project_API.Models;

namespace MIS_Project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        // ĐÃ SỬA: Dùng đúng tên Context thật của project
        private readonly MisProjectManagementContext _context;

        public UserController(MisProjectManagementContext context)
        {
            _context = context;
        }

        // 1. GET: api/user (Lấy danh sách toàn bộ user)
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            // ĐÃ SỬA: Dùng _context.Users có "s" giống hệt AuthController
            var users = await _context.Users
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    DepartmentId = u.DepartmentId,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt ?? DateTime.Now
                }).ToListAsync();

            return Ok(users);
        }

        // 2. POST: api/user (Admin tạo tài khoản mới cho nhân sự)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Tên đăng nhập đã tồn tại.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email đã được sử dụng.");

            var validRoles = new[] { "Admin", "Manager", "Employee", "C_Level", "Client" };
            if (!validRoles.Contains(dto.Role))
                return BadRequest("Vai trò (Role) không hợp lệ.");

            var newUser = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                DepartmentId = dto.DepartmentId,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo tài khoản nhân sự thành công!", userId = newUser.UserId });
        }

        // 3. PUT: api/user/{id} (Cập nhật thông tin & trạng thái khóa/mở)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy nhân sự.");

            var validStatuses = new[] { "Active", "Locked" };
            if (!validStatuses.Contains(dto.Status)) return BadRequest("Trạng thái không hợp lệ.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.DepartmentId = dto.DepartmentId;
            user.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật tài khoản thành công!" });
        }

        // 4. DELETE: api/user/{id} (Xóa cứng hoặc chuyển sang trạng thái Locked)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy nhân sự.");

            user.Status = "Locked";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã khóa tài khoản nhân sự an toàn." });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;
using Project.Utils;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly FastRailDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(FastRailDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tạo tài khoản người dùng mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            // 1. Kiểm tra role có tồn tại không
            var roleExists = await _context.Role.AnyAsync(r => r.RoleId == request.RoleId);
            if (!roleExists)
            {
                return BadRequest(new { message = "Role không tồn tại" });
            }

            // 2. Kiểm tra username/email đã tồn tại chưa
            var userExists = await _context.User.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (userExists)
            {
                return BadRequest(new { message = "Username hoặc email đã tồn tại" });
            }

            // 3. Tạo user
            var newUser = UserUtils.CreateUser(
                username: request.Username,
                email: request.Email,
                fullName: request.FullName,
                password: request.Password,
                roleId: request.RoleId,
                phone: request.Phone,
                gender: request.Gender,
                dateOfBirth: request.DateOfBirth,
                address: request.Address
            );

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo tài khoản thành công", userId = newUser.UserId });
        }

        /// <summary>
        /// Trả về mật khẩu đã được hash (chỉ dùng để test)
        /// </summary>
        [HttpPost("hash")]
        public IActionResult HashPassword([FromBody] HashPasswordRequest request)
        {
            var hashed = UserUtils.HashPassword(request.Password);
            return Ok(new { hashedPassword = hashed });
        }

        /// <summary>
        /// Kiểm tra mật khẩu có đúng với hash không (chỉ dùng để test)
        /// </summary>
        [HttpPost("verify")]
        public IActionResult VerifyPassword([FromBody] VerifyPasswordRequest request)
        {
            var isMatch = UserUtils.VerifyPassword(request.Password, request.HashedPassword);
            return Ok(new { isMatch });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Project.DTO;
using Project.DTOs;
using Project.Services;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("staff/login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> StaffLogin([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Email and password are required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var result = await _authService.LoginStaffAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("Staff login successful for email: {Email}", request.Email);
                    return Ok(new ApiResponse<LoginResponse>
                    {
                        Success = true,
                        Data = result,
                        Message = "Login successful",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }
                else
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        RequestId = HttpContext.TraceIdentifier
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff login for email: {Email}", request.Email);
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An error occurred during login",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("staff/logout")]
        public async Task<ActionResult<ApiResponse<object>>> StaffLogout([FromHeader(Name = "Authorization")] string? authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Authorization header is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var token = authorization.Substring("Bearer ".Length);
                var result = await _authService.LogoutStaffAsync(token);

                if (result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Logout successful",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff logout");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during logout",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("staff/validate")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> ValidateStaffToken([FromHeader(Name = "Authorization")] string? authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return Unauthorized(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "Authorization header is required",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var token = authorization.Substring("Bearer ".Length);
                var isValid = await _authService.ValidateStaffTokenAsync(token);

                if (!isValid)
                {
                    return Unauthorized(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "Invalid or expired token",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                var user = await _authService.GetStaffUserAsync(token);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found",
                        RequestId = HttpContext.TraceIdentifier
                    });
                }

                return Ok(new ApiResponse<UserResponse>
                {
                    Success = true,
                    Data = user,
                    Message = "Token is valid",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating staff token");
                return StatusCode(500, new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "An error occurred during token validation",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }
    }
} 
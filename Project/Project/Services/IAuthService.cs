using Project.DTOs;

namespace Project.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginStaffAsync(LoginRequest request);
        Task<bool> ValidateStaffTokenAsync(string token);
        Task<UserResponse?> GetStaffUserAsync(string token);
        Task<bool> LogoutStaffAsync(string token);
    }
} 
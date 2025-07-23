using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using Project.DTOs;
using Project.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly FastRailDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly Dictionary<string, string> _activeTokens = new(); // Simple in-memory token storage

        public AuthService(FastRailDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginStaffAsync(LoginRequest request)
        {
            try
            {

                // Trim email and password to avoid issues with extra spaces
                var inputEmail = request.Email.Trim();
                var inputPassword = request.Password.Trim();

                // Find user by email (case-insensitive)
                var user = await _context.User
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == inputEmail.ToLower() && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: user not found or inactive. Email: {Email}", inputEmail);

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Check if user has STAFF role
                var hasStaffRole = user.UserRoles.Any(ur => ur.Role.RoleName.ToLower() == "staff");
                if (!hasStaffRole)
                {
                    _logger.LogWarning("Login failed: user does not have STAFF role. Email: {Email}", inputEmail);

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Access denied. STAFF role required."
                    };
                }


                // Verify password (trim both input and db value)
                if (!VerifyPassword(inputPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: password mismatch. Email: {Email}, InputPassword: '{InputPassword}', DbPassword: '{DbPassword}'", inputEmail, inputPassword, user.PasswordHash);

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                // Store token
                _activeTokens[token] = user.Email;

                // Update last login
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Staff user {Email} logged in successfully", user.Email);

                return new LoginResponse
                {
                    Success = true,
                    User = new UserResponse
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        FullName = user.FullName,
                        Phone = user.Phone,
                        DateOfBirth = user.DateOfBirth,
                        Address = user.Address,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.UpdatedAt,
                        IsActive = user.IsActive
                    },
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(8), // 8 hours token validity
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff login for email {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<bool> ValidateStaffTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return false;


                // Validate JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSecretKeyHere12345678901234567890");
                var issuer = _configuration["Jwt:Issuer"] ?? "FastRailSystem";
                var audience = _configuration["Jwt:Audience"] ?? "FastRailStaff";

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,

                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserResponse?> GetStaffUserAsync(string token)
        {
            try
            {
                // Extract email from JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSecretKeyHere12345678901234567890");
                var issuer = _configuration["Jwt:Issuer"] ?? "FastRailSystem";
                var audience = _configuration["Jwt:Audience"] ?? "FastRailStaff";
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return null;

                var user = await _context.User
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

                if (user == null || !user.UserRoles.Any(ur => ur.Role.RoleName.ToLower() == "staff"))
                    return null;

                return new UserResponse
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.UpdatedAt,
                    IsActive = user.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff user from token");
                return null;
            }
        }

        public  async Task<bool> LogoutStaffAsync(string token)
        {
            try
            {
                if (_activeTokens.ContainsKey(token))
                {
                    _activeTokens.Remove(token);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff logout");
                return false;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSecretKeyHere12345678901234567890");
            var issuer = _configuration["Jwt:Issuer"] ?? "FastRailSystem";
            var audience = _configuration["Jwt:Audience"] ?? "FastRailStaff";


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("role", "staff")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            // Compare trimmed values to avoid issues with extra spaces
            return password.Trim() == passwordHash.Trim();

        }

        private string GenerateAdminJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSecretKeyHere12345678901234567890");
            var issuer = _configuration["Jwt:Issuer"] ?? "FastRailSystem";
            var audience = _configuration["Jwt:Audience"] ?? "FastRailAdmin";


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, "admin")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<LoginResponse> LoginAdminAsync(LoginRequest request)
        {
            var user = await _context.User
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                
                return new LoginResponse { Success = false, Message = "Invalid username or password" };
            }

            var isAdmin = user.UserRoles.Any(ur => ur.Role.RoleName == "Admin");
            if (!isAdmin)
            {
                return new LoginResponse { Success = false, Message = "You are not authorized as an admin." };
            }

            var token = GenerateAdminJwtToken(user);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                
            };
        }
    }
} 
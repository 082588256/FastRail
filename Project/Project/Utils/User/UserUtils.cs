using Project.Models;
using System.Security.Cryptography;

namespace Project.Utils
{
    public static class UserUtils
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            byte[] storedPasswordHash = new byte[32];
            Array.Copy(hashBytes, 16, storedPasswordHash, 0, 32);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] inputPasswordHash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (storedPasswordHash[i] != inputPasswordHash[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tạo User mới và gán roleId vào danh sách UserRoles
        /// </summary>
        public static User CreateUser(string username, string email, string fullName, string password,
                                      int roleId,
                                      string? phone = null, string? gender = null, DateTime? dateOfBirth = null, string? address = null)
        {
            var user = new User
            {
                Username = username.Trim(),
                Email = email.Trim(),
                FullName = fullName.Trim(),
                Phone = phone?.Trim(),
                Gender = gender?.Trim(),
                DateOfBirth = dateOfBirth,
                Address = address?.Trim(),
                PasswordHash = HashPassword(password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        RoleId = roleId,
                        AssignedAt = DateTime.UtcNow
                    }
                }
            };

            return user;
        }
    }
}

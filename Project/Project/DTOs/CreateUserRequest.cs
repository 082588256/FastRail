namespace Project.DTOs
{
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }

        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }

    public class HashPasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyPasswordRequest
    {
        public string Password { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
    }
}

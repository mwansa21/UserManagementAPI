using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Admin|User|Moderator)$", ErrorMessage = "Role must be Admin, User, or Moderator.")]
        public string Role { get; set; } = "User";

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO used for creating or updating a user (excludes server-set fields).
    /// </summary>
    public class UserDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Admin|User|Moderator)$", ErrorMessage = "Role must be Admin, User, or Moderator.")]
        public string Role { get; set; } = "User";

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Represents login credentials for generating a JWT token.
    /// </summary>
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

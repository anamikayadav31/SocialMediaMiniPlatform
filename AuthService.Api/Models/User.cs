using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email),    IsUnique = true)]
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required][MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required][MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required][EmailAddress][MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Bio { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsPrivate { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [MaxLength(20)]
    public string Role { get; set; } = "User";

    public int FollowerCount  { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;
    public int PostCount      { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
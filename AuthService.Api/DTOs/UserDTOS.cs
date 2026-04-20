using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required][MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required][MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required][EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required][MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required][EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    [MaxLength(100)]
    public string? FullName  { get; set; }

    [MaxLength(500)]
    public string? Bio       { get; set; }

    public string? AvatarUrl { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required][MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

public class TokenRequestDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token    { get; set; } = string.Empty;
    public int    UserId   { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role     { get; set; } = string.Empty;
}

public class UserProfileDto
{
    public int      UserId         { get; set; }
    public string   UserName       { get; set; } = string.Empty;
    public string   FullName       { get; set; } = string.Empty;
    public string   Bio            { get; set; } = string.Empty;
    public string?  AvatarUrl      { get; set; }
    public bool     IsPrivate      { get; set; }
    public bool     IsActive       { get; set; }
    public string   Role           { get; set; } = string.Empty;
    public int      FollowerCount  { get; set; }
    public int      FollowingCount { get; set; }
    public int      PostCount      { get; set; }
    public DateTime CreatedAt      { get; set; }
}
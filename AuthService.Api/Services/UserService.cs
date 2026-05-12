using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

// Business logic layer — all rules live here
public class UserService : IUserService
{
    private readonly IUserRepository      _repo;
    private readonly IConfiguration       _config;
    private readonly PasswordHasher<User> _hasher = new();

    public UserService(IUserRepository repo, IConfiguration config)
    {
        _repo   = repo;
        _config = config;
    }

    // ── REGISTER ─────────────────────────────────────────────
    public async Task<AuthResponseDto?> Register(RegisterDto dto)
    {
        if (await _repo.ExistsByUserName(dto.UserName)) return null;
        if (await _repo.ExistsByEmail(dto.Email))       return null;

        var user = new User
        {
            UserName  = dto.UserName.ToLower().Trim(),
            FullName  = dto.FullName.Trim(),
            Email     = dto.Email.ToLower().Trim(),
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        await _repo.AddUser(user);
        await _repo.SaveChanges();

        return new AuthResponseDto
        {
            Token    = GenerateJwtToken(user),
            UserId   = user.UserId,
            UserName = user.UserName,
            FullName = user.FullName,
            Role     = user.Role
        };
    }

    // ── LOGIN ─────────────────────────────────────────────────
    public async Task<AuthResponseDto?> Login(LoginDto dto)
    {
        var user = await _repo.FindByEmail(dto.Email.ToLower().Trim());
        if (user == null || !user.IsActive) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        return new AuthResponseDto
        {
            Token    = GenerateJwtToken(user),
            UserId   = user.UserId,
            UserName = user.UserName,
            FullName = user.FullName,
            Role     = user.Role
        };
    }

    // ── LOGOUT ───────────────────────────────────────────────
    public Task Logout(int userId) => Task.CompletedTask;

    // ── VALIDATE TOKEN ────────────────────────────────────────
    public Task<bool> ValidateToken(string token)
    {
        try
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = key,
                ValidateIssuer           = true,
                ValidIssuer              = _config["Jwt:Issuer"],
                ValidateAudience         = true,
                ValidAudience            = _config["Jwt:Audience"],
                ClockSkew                = TimeSpan.Zero
            }, out _);
            return Task.FromResult(true);
        }
        catch { return Task.FromResult(false); }
    }

    // ── REFRESH TOKEN ─────────────────────────────────────────
    public async Task<string?> RefreshToken(string token)
    {
        try
        {
            var handler     = new JwtSecurityTokenHandler();
            var jwt         = handler.ReadJwtToken(token);
            var userIdClaim = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null) return null;

            // FIX: use FindById to match interface alias
            var user = await _repo.FindById(int.Parse(userIdClaim));
            if (user == null || !user.IsActive) return null;

            return GenerateJwtToken(user);
        }
        catch { return null; }
    }

    // ── GET USER BY ID ────────────────────────────────────────
    // FIX: use FindById (alias) so Moq setup on FindById works
    public async Task<UserProfileDto?> GetUserById(int userId)
    {
        var user = await _repo.FindById(userId);
        return user == null ? null : ToDto(user);
    }

    // ── GET USER BY USERNAME ──────────────────────────────────
    public async Task<UserProfileDto?> GetUserByUserName(string userName)
    {
        var user = await _repo.FindByUserName(userName.ToLower().Trim());
        return user == null ? null : ToDto(user);
    }

    // ── UPDATE PROFILE ────────────────────────────────────────
    // FIX: use FindById
    public async Task<bool> UpdateProfile(int userId, UpdateProfileDto dto)
    {
        var user = await _repo.FindById(userId);
        if (user == null) return false;

        if (dto.FullName  != null) user.FullName  = dto.FullName.Trim();
        if (dto.Bio       != null) user.Bio       = dto.Bio.Trim();
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl.Trim();

        _repo.UpdateUser(user);
        await _repo.SaveChanges();
        return true;
    }

    // ── CHANGE PASSWORD ───────────────────────────────────────
    public async Task<bool> ChangePassword(int userId, ChangePasswordDto dto)
    {
        var user = await _repo.FindById(userId);
        if (user == null) return false;

        var check = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (check == PasswordVerificationResult.Failed) return false;

        user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
        _repo.UpdateUser(user);
        await _repo.SaveChanges();
        return true;
    }

    // ── SEARCH USERS ──────────────────────────────────────────
    // FIX: use SearchByQuery (alias) so Moq setup on SearchByQuery works
    public async Task<List<UserProfileDto>> SearchUsers(string query)
        => (await _repo.SearchByQuery(query)).Select(ToDto).ToList();

    // ── TOGGLE PRIVACY ────────────────────────────────────────
    // FIX: use FindById
    public async Task<bool> TogglePrivacy(int userId)
    {
        var user = await _repo.FindById(userId);
        if (user == null) return false;

        user.IsPrivate = !user.IsPrivate;
        _repo.UpdateUser(user);
        await _repo.SaveChanges();
        return true;
    }

    // ── DEACTIVATE ACCOUNT ────────────────────────────────────
    // FIX: use FindById
    public async Task<bool> DeactivateAccount(int userId)
    {
        var user = await _repo.FindById(userId);
        if (user == null) return false;

        user.IsActive = false;
        _repo.UpdateUser(user);
        await _repo.SaveChanges();
        return true;
    }

    // ── SUGGESTED USERS ───────────────────────────────────────
    public async Task<List<UserProfileDto>> GetSuggestedUsers(int userId)
        => (await _repo.FindAllActive())
            .Where(u => u.UserId != userId)
            .Take(10)
            .Select(ToDto)
            .ToList();

    // ── ADMIN — GET ALL USERS ─────────────────────────────────
    public async Task<List<UserProfileDto>> GetAllUsers()
        => (await _repo.FindAllActive()).Select(ToDto).ToList();

    // ── ADMIN — SUSPEND USER ──────────────────────────────────
    public async Task<bool> SuspendUser(int userId)
    {
        var user = await _repo.FindById(userId);
        if (user == null) return false;

        user.IsActive = false;
        _repo.UpdateUser(user);
        await _repo.SaveChanges();
        return true;
    }

    // ── ADMIN — ACTIVATE USER ─────────────────────────────────
    public async Task<bool> ActivateUser(int userId)
    {
        var user = await _repo.FindById(userId);
        if (user == null) return false;

        user.IsActive = true;
        _repo.UpdateUser(user);
        await _repo.SaveChanges();
        return true;
    }

    // ── ADMIN — PERMANENT DELETE ──────────────────────────────
    public async Task<bool> AdminDeleteUser(int userId)
    {
        var user = await _repo.FindById(userId);
        if (user == null) return false;

        _repo.RemoveUser(user);
        await _repo.SaveChanges();
        return true;
    }

    // ── UPDATE COUNTERS ───────────────────────────────────────
    public async Task UpdateCounters(int userId, string field, int delta)
        => await _repo.UpdateCounters(userId, field, delta);

    // ── IS PRIVATE ────────────────────────────────────────────
    public async Task<bool> IsPrivate(int userId)
    {
        var user = await _repo.FindById(userId);
        return user?.IsPrivate ?? false;
    }

    // ── UPDATE COUNTERS (follow overload) ─────────────────────
    public async Task UpdateCounters(int followerId, int followeeId, bool increment)
    {
        int delta = increment ? 1 : -1;
        await _repo.UpdateCounters(followerId, "FollowingCount", delta);
        await _repo.UpdateCounters(followeeId, "FollowerCount",  delta);
    }

    // ── JWT ───────────────────────────────────────────────────
    private string GenerateJwtToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name,           user.UserName),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role)
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── GOOGLE LOGIN ─────────────────────────────────────────
    public async Task<AuthResponseDto?> GoogleLogin(string email, string? name, string? picture)
    {
        var user = await _repo.FindByEmail(email.ToLower().Trim());

        if (user == null)
        {
            var baseUsername = email.Split('@')[0].ToLower().Replace(".", "_").Replace("+", "_");
            var userName = baseUsername;
            int suffix = 1;
            while (await _repo.ExistsByUserName(userName))
                userName = $"{baseUsername}_{suffix++}";

            user = new User
            {
                UserName     = userName,
                FullName     = name ?? userName,
                Email        = email.ToLower().Trim(),
                PasswordHash = "",
                AvatarUrl    = null,
                IsActive     = true,
                Role         = "User",
                CreatedAt    = DateTime.UtcNow
            };
            await _repo.AddUser(user);
            await _repo.SaveChanges();
        }

        if (!user.IsActive) return null;

        return new AuthResponseDto
        {
            Token    = GenerateJwtToken(user),
            UserId   = user.UserId,
            UserName = user.UserName,
            FullName = user.FullName,
            Role     = user.Role
        };
    }

    // ── MAPPER ───────────────────────────────────────────────
    private static UserProfileDto ToDto(User u) => new()
    {
        UserId         = u.UserId,
        UserName       = u.UserName,
        FullName       = u.FullName,
        Bio            = u.Bio,
        AvatarUrl      = u.AvatarUrl,
        IsPrivate      = u.IsPrivate,
        IsActive       = u.IsActive,
        Role           = u.Role,
        FollowerCount  = u.FollowerCount,
        FollowingCount = u.FollowingCount,
        PostCount      = u.PostCount,
        CreatedAt      = u.CreatedAt
    };
}
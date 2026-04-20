using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _svc;

    public UserController(IUserService svc) { _svc = svc; }

    // POST /api/users/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _svc.Register(dto);
        if (result == null)
            return BadRequest(new { message = "Username or Email already taken." });
        return Ok(result);
    }

    // POST /api/users/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _svc.Login(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });
        return Ok(result);
    }

    // POST /api/users/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _svc.Logout(userId);
        return Ok(new { message = "Logged out successfully." });
    }

    // POST /api/users/validate-token
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] TokenRequestDto dto)
    {
        var isValid = await _svc.ValidateToken(dto.Token);
        return Ok(new { isValid });
    }

    // POST /api/users/refresh-token
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto dto)
    {
        var newToken = await _svc.RefreshToken(dto.Token);
        if (newToken == null)
            return Unauthorized(new { message = "Token invalid or user not found." });
        return Ok(new { token = newToken });
    }

    // GET /api/users/{id}
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(int id)
    {
        var profile = await _svc.GetUserById(id);
        if (profile == null) return NotFound(new { message = "User not found." });
        return Ok(profile);
    }

    // GET /api/users/by-username/{userName}  — public
    [HttpGet("by-username/{userName}")]
    public async Task<IActionResult> GetUserByUserName(string userName)
    {
        var profile = await _svc.GetUserByUserName(userName);
        if (profile == null) return NotFound(new { message = $"User '{userName}' not found." });
        return Ok(profile);
    }

    // PUT /api/users/{id}/profile
    [HttpPut("{id:int}/profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDto dto)
    {
        var success = await _svc.UpdateProfile(id, dto);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "Profile updated." });
    }

    // PUT /api/users/{id}/change-password
    [HttpPut("{id:int}/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        var success = await _svc.ChangePassword(id, dto);
        if (!success) return BadRequest(new { message = "Current password is incorrect." });
        return Ok(new { message = "Password changed." });
    }

    // PUT /api/users/{id}/toggle-privacy
    [HttpPut("{id:int}/toggle-privacy")]
    [Authorize]
    public async Task<IActionResult> TogglePrivacy(int id)
    {
        var success = await _svc.TogglePrivacy(id);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "Privacy setting updated." });
    }

    // DELETE /api/users/{id}/deactivate
    [HttpDelete("{id:int}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateAccount(int id)
    {
        var success = await _svc.DeactivateAccount(id);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "Account deactivated." });
    }

    // GET /api/users/search?q=john  — public
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search query cannot be empty." });
        return Ok(await _svc.SearchUsers(q));
    }

    // GET /api/users/{id}/suggested
    [HttpGet("{id:int}/suggested")]
    [Authorize]
    public async Task<IActionResult> GetSuggestedUsers(int id)
        => Ok(await _svc.GetSuggestedUsers(id));

    // PUT /api/users/{id}/update-counters — called by other services
    [HttpPut("{id:int}/update-counters")]
    public async Task<IActionResult> UpdateCounters(
        int id, [FromQuery] string field, [FromQuery] int delta)
    {
        await _svc.UpdateCounters(id, field, delta);
        return Ok(new { message = "Counter updated." });
    }

    // ── ADMIN ─────────────────────────────────────────────────

    // GET /api/users/admin/all
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
        => Ok(await _svc.GetAllUsers());

    // PUT /api/users/admin/suspend/{id}
    [HttpPut("admin/suspend/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SuspendUser(int id)
    {
        var success = await _svc.SuspendUser(id);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "User suspended." });
    }

    // DELETE /api/users/admin/delete/{id}
    [HttpDelete("admin/delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDeleteUser(int id)
    {
        var success = await _svc.AdminDeleteUser(id);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "User permanently deleted." });
    }
}
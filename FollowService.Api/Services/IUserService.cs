using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Contract for calling AuthService to check if a user is private
/// and to update follower/following counters.
/// </summary>
public interface IUserService
{
    Task<bool> IsPrivate(int userId);
    Task       UpdateCounters(int followerId, int followeeId, bool increment);
}

/// <summary>
/// Real implementation — calls AuthService via HTTP.
/// </summary>
public class UserService : IUserService
{
    private readonly HttpClient _http;
    private readonly ILogger<UserService> _logger;
    private readonly string _baseUrl;

    public UserService(HttpClient http, IConfiguration config, ILogger<UserService> logger)
    {
        _http = http;
        _logger = logger;
        _baseUrl = config["ServiceUrls:AuthService"] ?? "http://localhost:5050";
    }

    public async Task<bool> IsPrivate(int userId)
    {
        try
        {
            // GET /api/users/{id}
            // Note: Since this endpoint is [Authorize] in AuthService, 
            // but we call it internally, it might fail unless we forward the token.
            // For now, we assume public check is possible or fallback to public (false).
            var resp = await _http.GetAsync($"{_baseUrl}/api/users/{userId}");
            if (!resp.IsSuccessStatusCode) return false;

            var user = await resp.Content.ReadFromJsonAsync<JsonElement>();
            return user.TryGetProperty("isPrivate", out var p) && p.GetBoolean();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user {UserId} is private", userId);
            return false; 
        }
    }

    public async Task UpdateCounters(int followerId, int followeeId, bool increment)
    {
        int delta = increment ? 1 : -1;
        try
        {
            // 1. Update FollowerCount for the person being followed (followee)
            var resp1 = await _http.PutAsync($"{_baseUrl}/api/users/{followeeId}/update-counters?field=FollowerCount&delta={delta}", null);
            
            // 2. Update FollowingCount for the person doing the following (follower)
            var resp2 = await _http.PutAsync($"{_baseUrl}/api/users/{followerId}/update-counters?field=FollowingCount&delta={delta}", null);

            if (resp1.IsSuccessStatusCode && resp2.IsSuccessStatusCode)
            {
                _logger.LogInformation("[UserService] Successfully updated counters for follower={f} and followee={e} (delta={d})", followerId, followeeId, delta);
            }
            else
            {
                _logger.LogWarning("[UserService] Failed to update some counters. F={s1}, E={s2}", resp2.StatusCode, resp1.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update counters for follower {FollowerId} / followee {FolloweeId}", followerId, followeeId);
        }
    }
}
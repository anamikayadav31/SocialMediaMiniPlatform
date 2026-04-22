/// <summary>
/// Contract for calling AuthService to check if a user is private
/// and to update follower/following counters.
/// Replace stubs with real HTTP calls.
/// </summary>
public interface IUserService
{
    Task<bool> IsPrivate(int userId);
    Task       UpdateCounters(int followerId, int followeeId, bool increment);
}

/// <summary>
/// Stub — logs only. Replace with real IHttpClientFactory typed HTTP client calls.
/// </summary>
public class UserServiceStub : IUserService
{
    private readonly ILogger<UserServiceStub> _logger;

    public UserServiceStub(ILogger<UserServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsPrivate(int userId)
    {
        _logger.LogInformation(
            "[UserServiceStub] IsPrivate check — userId={UserId} → returning false (public)",
            userId);

        // TODO: Replace with e.g.:
        // var user = await _httpClient.GetFromJsonAsync<UserDto>($"http://auth-service/api/users/{userId}");
        // return user?.IsPrivate ?? false;

        return Task.FromResult(false); // default: public
    }

    public Task UpdateCounters(int followerId, int followeeId, bool increment)
    {
        _logger.LogInformation(
            "[UserServiceStub] UpdateCounters — followerId={FollowerId}, followeeId={FolloweeId}, increment={Increment}",
            followerId, followeeId, increment);

        // TODO: Replace with e.g.:
        // await _httpClient.PutAsJsonAsync("http://auth-service/api/users/updateCounters",
        //     new { followerId, followeeId, increment });

        return Task.CompletedTask;
    }
}
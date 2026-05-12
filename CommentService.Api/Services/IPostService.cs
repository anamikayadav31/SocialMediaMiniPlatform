/// <summary>
/// Contract for calling PostService to update comment count.
/// Replace stub with real HTTP call.
/// </summary>
public interface IPostService
{
    Task IncrementCommentCount(int postId);
}

/// <summary>
/// Stub — logs only. Replace with real HTTP call to PostService.
/// </summary>
public class PostServiceStub : IPostService
{
    private readonly ILogger<PostServiceStub> _logger;

    public PostServiceStub(ILogger<PostServiceStub> logger)
    {
        _logger = logger;
    }

    public Task IncrementCommentCount(int postId)
    {
        _logger.LogInformation(
            "[PostServiceStub] IncrementCommentCount — postId={PostId}", postId);

        // TODO: Replace with e.g.:
        // await _httpClient.PutAsync($"http://post-service/api/posts/{postId}/incrementComment", null);

        return Task.CompletedTask;
    }
}
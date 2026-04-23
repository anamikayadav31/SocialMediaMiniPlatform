using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/feed")]
public class FeedController : ControllerBase
{
    private readonly IFeedService _svc;

    public FeedController(IFeedService svc) { _svc = svc; }

    // GET /api/feed/{userId}?page=1&pageSize=10
    [HttpGet("{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetFeed(
        int userId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await _svc.GetFeedForUser(userId, page, pageSize));

    // GET /api/feed/explore/{userId}?page=1&pageSize=10
    [HttpGet("explore/{userId:int}")]
    public async Task<IActionResult> GetExploreFeed(
        int userId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await _svc.GetExploreFeed(userId, page, pageSize));

    // POST /api/feed/fanout
    [HttpPost("fanout")]
    public async Task<IActionResult> Fanout([FromBody] FanoutDto dto)
    {
        await _svc.AddPostToFollowerFeeds(dto.PostId, dto.AuthorId, dto.FollowerIds);
        return Ok(new { message = $"Fanned out to {dto.FollowerIds.Count} followers." });
    }

    // GET /api/feed/trending?topN=10
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingHashtags([FromQuery] int topN = 10)
        => Ok(await _svc.GetTrendingHashtags(topN));

    // GET /api/feed/suggested/{userId}
    [HttpGet("suggested/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetSuggestedUsers(int userId)
        => Ok(await _svc.GetSuggestedUsers(userId));
}
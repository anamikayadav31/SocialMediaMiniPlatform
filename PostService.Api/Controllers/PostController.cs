using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly IPostService _svc;
    private readonly IHttpClientFactory _httpFactory;
    private const string FOLLOW_BASE = "http://localhost:5400";
    private const string FEED_BASE   = "http://localhost:5600";

    public PostController(IPostService svc, IHttpClientFactory httpFactory)
    {
        _svc = svc;
        _httpFactory = httpFactory;
    }

    // POST /api/posts
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
    {
        var result = await _svc.CreatePost(dto);
        if (result != null)
        {
            // Background mein fanout karo — response block mat karo
            _ = Task.Run(() => FanoutToFollowers(result.PostId, dto.UserId));
        }
        return Ok(result);
    }

    private async Task FanoutToFollowers(int postId, int authorId)
    {
        try
        {
            var http = _httpFactory.CreateClient();

            // 1. Get followers list (People following the author)
            var followersResp = await http.GetAsync($"{FOLLOW_BASE}/api/follows/followers/{authorId}");
            var followersJson = followersResp.IsSuccessStatusCode ? await followersResp.Content.ReadAsStringAsync() : "[]";

            // 2. Get following list (People the author is following)
            var followingResp = await http.GetAsync($"{FOLLOW_BASE}/api/follows/following/{authorId}");
            var followingJson = followingResp.IsSuccessStatusCode ? await followingResp.Content.ReadAsStringAsync() : "[]";

            var targetUserIds = new HashSet<int>();

            // Add Followers
            using (var doc = JsonDocument.Parse(followersJson)) {
                foreach (var e in doc.RootElement.EnumerateArray()) {
                    if (e.TryGetProperty("followerId", out var p)) targetUserIds.Add(p.GetInt32());
                }
            }

            // Add Following
            using (var doc = JsonDocument.Parse(followingJson)) {
                foreach (var e in doc.RootElement.EnumerateArray()) {
                    if (e.TryGetProperty("followeeId", out var p)) targetUserIds.Add(p.GetInt32());
                }
            }

            // Also ensure author sees their own post in feed (optional, but often desired)
            targetUserIds.Add(authorId);

            var finalIds = targetUserIds.Where(id => id > 0).ToList();
            if (finalIds.Count == 0) return;

            // 3. Fanout to FeedService
            var fanoutPayload = JsonSerializer.Serialize(new { postId, authorId, followerIds = finalIds });
            await http.PostAsync($"{FEED_BASE}/api/feed/fanout",
                new StringContent(fanoutPayload, Encoding.UTF8, "application/json"));
        }
        catch { /* Fanout failure should not affect post creation */ }
    }

    // GET /api/posts/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPostById(int id)
    {
        var post = await _svc.GetPostById(id);
        if (post == null) return NotFound(new { message = "Post not found." });
        return Ok(post);
    }

    // GET /api/posts/user/{userId}
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetPostsByUser(int userId)
        => Ok(await _svc.GetPostsByUser(userId));

    // GET /api/posts/public?page=1&pageSize=10
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicPosts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _svc.GetPublicPosts(page, pageSize));

    // PUT /api/posts/{id}
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto dto)
    {
        var result = await _svc.UpdatePost(id, dto);
        if (result == null) return NotFound(new { message = "Post not found." });
        return Ok(result);
    }

    // DELETE /api/posts/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var success = await _svc.DeletePost(id);
        if (!success) return NotFound(new { message = "Post not found." });
        return Ok(new { message = "Post deleted." });
    }

    // GET /api/posts/hashtag/{tag}?page=1&pageSize=10
    [HttpGet("hashtag/{tag}")]
    public async Task<IActionResult> GetByHashtag(
        string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _svc.GetByHashtag(tag, page, pageSize));

    // GET /api/posts/search?q=hello&page=1&pageSize=10
    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchPosts(
        [FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search query cannot be empty." });
        return Ok(await _svc.SearchPosts(q, page, pageSize));
    }

    // GET /api/posts/trending?hoursBack=24&take=20
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingPosts(
        [FromQuery] int hoursBack = 24, [FromQuery] int take = 20)
        => Ok(await _svc.GetTrendingPosts(hoursBack, take));

    // GET /api/posts/feed?followingIds=1&followingIds=2&page=1&pageSize=10
    // FIX: [FromBody] on a GET is not supported — changed to [FromQuery]
    [HttpGet("feed")]
    [Authorize]
    public async Task<IActionResult> GetFeed(
        [FromQuery] List<int> followingIds,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _svc.GetFeedForUser(followingIds, page, pageSize));

    // GET /api/posts/explore?excludeUserIds=1&excludeUserIds=2&page=1&pageSize=10
    // FIX: [FromBody] on a GET is not supported — changed to [FromQuery]
    [HttpGet("explore")]
    [Authorize]
    public async Task<IActionResult> GetExploreFeed(
        [FromQuery] List<int> excludeUserIds,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _svc.GetExploreFeed(excludeUserIds, page, pageSize));

    // GET /api/posts/timeline/{userId}?page=1&pageSize=10
    [HttpGet("timeline/{userId:int}")]
    public async Task<IActionResult> GetTimeline(
        int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _svc.GetTimeline(userId, page, pageSize));

    // PUT /api/posts/{id}/increment-count
    [HttpPut("{id:int}/increment-count")]
    public async Task<IActionResult> IncrementCount(int id, [FromBody] IncrementCountDto dto)
    {
        await _svc.IncrementCount(id, dto.Field, dto.Delta);
        return Ok(new { message = "Count updated." });
    }
}
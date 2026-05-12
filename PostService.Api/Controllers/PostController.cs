using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly IPostService _svc;
    public PostController(IPostService svc) { _svc = svc; }

    // POST /api/posts
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
    {
        var result = await _svc.CreatePost(dto);
        return Ok(result);
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
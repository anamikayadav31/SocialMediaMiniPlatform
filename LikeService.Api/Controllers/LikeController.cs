using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/likes")]
public class LikeController : ControllerBase
{
    private readonly ILikeService _svc;
    public LikeController(ILikeService svc) { _svc = svc; }

    // POST /api/likes/toggle
    // Body: { userId, targetId, targetType }
    [HttpPost("toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeDto dto)
    {
        var result = await _svc.ToggleLike(dto.UserId, dto.TargetId, dto.TargetType, dto.OwnerId);
        return Ok(result);
    }

    // GET /api/likes/byTarget/{targetId}?targetType=POST
    [HttpGet("byTarget/{targetId:int}")]
    public async Task<IActionResult> GetByTarget(
        int targetId, [FromQuery] TargetType targetType)
        => Ok(await _svc.GetLikesByTarget(targetId, targetType));

    // GET /api/likes/byUser/{userId}
    [HttpGet("byUser/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(await _svc.GetLikesByUser(userId));

    // GET /api/likes/count?targetId=5&targetType=POST
    [HttpGet("count")]
    public async Task<IActionResult> GetLikeCount(
        [FromQuery] int targetId, [FromQuery] TargetType targetType)
    {
        int count = await _svc.GetLikeCount(targetId, targetType);
        return Ok(new { targetId, targetType, count });
    }

    // GET /api/likes/hasLiked?userId=1&targetId=5&targetType=POST
    [HttpGet("hasLiked")]
    [Authorize]
    public async Task<IActionResult> HasLiked(
        [FromQuery] int userId,
        [FromQuery] int targetId,
        [FromQuery] TargetType targetType)
    {
        bool liked = await _svc.HasUserLiked(userId, targetId, targetType);
        return Ok(new { userId, targetId, targetType, liked });
    }

    // GET /api/likes/likers/{postId}
    [HttpGet("likers/{postId:int}")]
    public async Task<IActionResult> GetLikersForPost(int postId)
        => Ok(await _svc.GetLikersForPost(postId));

    // GET /api/likes/likedPosts/{userId}
    [HttpGet("likedPosts/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetLikedPostsByUser(int userId)
        => Ok(await _svc.GetLikedPostsByUser(userId));
}
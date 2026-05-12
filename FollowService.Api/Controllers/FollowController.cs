using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/follows")]
public class FollowController : ControllerBase
{
    private readonly IFollowService _svc;

    public FollowController(IFollowService svc) { _svc = svc; }

    // POST /api/follows
    // Body: { followerId, followeeId }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Follow([FromBody] FollowRequestDto dto)
    {
        var result = await _svc.FollowUser(dto.FollowerId, dto.FolloweeId);
        return Ok(result);
    }

    // DELETE /api/follows/unfollow
    // Body: { followerId, followeeId }
    [HttpDelete("unfollow")]
    [Authorize]
    public async Task<IActionResult> Unfollow([FromBody] FollowRequestDto dto)
    {
        await _svc.UnfollowUser(dto.FollowerId, dto.FolloweeId);
        return NoContent();
    }

    // PUT /api/follows/accept/{followId}
    [HttpPut("accept/{followId:int}")]
    [Authorize]
    public async Task<IActionResult> Accept(int followId)
    {
        var result = await _svc.AcceptFollowRequest(followId);
        return result == null ? NotFound() : Ok(result);
    }

    // PUT /api/follows/reject/{followId}
    [HttpPut("reject/{followId:int}")]
    [Authorize]
    public async Task<IActionResult> Reject(int followId)
    {
        var result = await _svc.RejectFollowRequest(followId);
        return result == null ? NotFound() : Ok(result);
    }

    // GET /api/follows/followers/{userId}
    [HttpGet("followers/{userId:int}")]
    public async Task<IActionResult> GetFollowers(int userId)
        => Ok(await _svc.GetFollowers(userId));

    // GET /api/follows/following/{userId}
    [HttpGet("following/{userId:int}")]
    public async Task<IActionResult> GetFollowing(int userId)
        => Ok(await _svc.GetFollowing(userId));

    // GET /api/follows/pending/{userId}
    [HttpGet("pending/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetPending(int userId)
        => Ok(await _svc.GetPendingRequests(userId));

    // GET /api/follows/isFollowing?followerId=1&followeeId=2
    [HttpGet("isFollowing")]
    public async Task<IActionResult> IsFollowing(
        [FromQuery] int followerId,
        [FromQuery] int followeeId)
    {
        bool result = await _svc.IsFollowing(followerId, followeeId);
        return Ok(new { followerId, followeeId, isFollowing = result });
    }

    // GET /api/follows/followerCount/{userId}
    [HttpGet("followerCount/{userId:int}")]
    public async Task<IActionResult> GetFollowerCount(int userId)
    {
        int count = await _svc.GetFollowerCount(userId);
        return Ok(new { userId, followerCount = count });
    }

    // GET /api/follows/followingCount/{userId}
    [HttpGet("followingCount/{userId:int}")]
    public async Task<IActionResult> GetFollowingCount(int userId)
    {
        int count = await _svc.GetFollowingCount(userId);
        return Ok(new { userId, followingCount = count });
    }

    // GET /api/follows/followingIds/{userId}
    [HttpGet("followingIds/{userId:int}")]
    public async Task<IActionResult> GetFollowingIds(int userId)
        => Ok(await _svc.GetFollowingIds(userId));

    // GET /api/follows/mutual/{userId}
    [HttpGet("mutual/{userId:int}")]
    public async Task<IActionResult> GetMutualFollowers(int userId)
        => Ok(await _svc.GetMutualFollowers(userId));
}
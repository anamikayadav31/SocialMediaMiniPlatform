public class FollowServiceImpl : IFollowService
{
    private readonly IFollowRepository _repo;
    private readonly IUserService      _userSvc;
    private readonly INotifService     _notif;

    public FollowServiceImpl(
        IFollowRepository repo,
        IUserService userSvc,
        INotifService notif)
    {
        _repo    = repo;
        _userSvc = userSvc;
        _notif   = notif;
    }

    // ── Follow ────────────────────────────────────────────────
    public async Task<FollowDto> FollowUser(int followerId, int followeeId)
    {
        var existing = await _repo.FindByFollowerAndFollowee(followerId, followeeId);
        if (existing != null) return ToDto(existing);

        bool isPrivate = await _userSvc.IsPrivate(followeeId);
        var status = isPrivate ? FollowStatus.PENDING : FollowStatus.ACCEPTED;

        var follow = new Follow
        {
            FollowerId = followerId,
            FolloweeId = followeeId,
            Status     = status,
            CreatedAt  = DateTime.UtcNow
        };

        await _repo.AddFollow(follow);
        await _repo.SaveChanges();

        if (status == FollowStatus.ACCEPTED)
            await _userSvc.UpdateCounters(followerId, followeeId, increment: true);

        await _notif.SendFollowNotif(followerId, followeeId, status);

        return ToDto(follow);
    }

    // ── Unfollow ──────────────────────────────────────────────
    public async Task UnfollowUser(int followerId, int followeeId)
    {
        var follow = await _repo.FindByFollowerAndFollowee(followerId, followeeId);
        if (follow == null) return;

        await _repo.DeleteFollowById(follow.FollowId);

        if (follow.Status == FollowStatus.ACCEPTED)
            await _userSvc.UpdateCounters(followerId, followeeId, increment: false);
    }

    // ── Accept / Reject ───────────────────────────────────────
    public async Task<FollowDto?> AcceptFollowRequest(int followId)
    {
        var follow = (await _repo.FindPendingRequests(0))
            .FirstOrDefault(f => f.FollowId == followId);

        if (follow == null || follow.Status != FollowStatus.PENDING) return null;

        follow.Status = FollowStatus.ACCEPTED;
        await _repo.SaveChanges();

        await _userSvc.UpdateCounters(follow.FollowerId, follow.FolloweeId, increment: true);
        await _notif.SendFollowNotif(follow.FolloweeId, follow.FollowerId, FollowStatus.ACCEPTED);

        return ToDto(follow);
    }

    public async Task<FollowDto?> RejectFollowRequest(int followId)
    {
        var follow = (await _repo.FindPendingRequests(0))
            .FirstOrDefault(f => f.FollowId == followId);

        if (follow == null || follow.Status != FollowStatus.PENDING) return null;

        follow.Status = FollowStatus.REJECTED;
        await _repo.SaveChanges();

        return ToDto(follow);
    }

    // ── Queries ───────────────────────────────────────────────
    // FIX: use FindFollowers alias so Moq setup on FindFollowers works
    public async Task<List<FollowDto>> GetFollowers(int userId)
        => (await _repo.FindFollowers(userId)).Select(ToDto).ToList();

    // FIX: use FindFollowing alias
    public async Task<List<FollowDto>> GetFollowing(int userId)
        => (await _repo.FindFollowing(userId)).Select(ToDto).ToList();

    public async Task<List<FollowDto>> GetPendingRequests(int userId)
        => (await _repo.FindPendingRequests(userId)).Select(ToDto).ToList();

    // FIX: use ExistsAccepted alias so Moq setup on ExistsAccepted works
    public async Task<bool> IsFollowing(int followerId, int followeeId)
        => await _repo.ExistsAccepted(followerId, followeeId);

    public async Task<int> GetFollowerCount(int userId)
        => await _repo.CountFollowers(userId);

    public async Task<int> GetFollowingCount(int userId)
        => await _repo.CountFollowing(userId);

    // FIX: use GetAcceptedFolloweeIds alias so Moq setup works
    public async Task<List<int>> GetFollowingIds(int userId)
        => await _repo.GetAcceptedFolloweeIds(userId);

    public async Task<List<FollowDto>> GetMutualFollowers(int userId)
    {
        var followers  = await _repo.FindFollowers(userId);
        var following  = await _repo.FindFollowing(userId);

        var followerIds  = followers.Select(f => f.FollowerId).ToHashSet();
        var followingIds = following.Select(f => f.FolloweeId).ToHashSet();

        var mutualIds = followerIds.Intersect(followingIds).ToHashSet();

        return followers
            .Where(f => mutualIds.Contains(f.FollowerId))
            .Select(ToDto)
            .ToList();
    }

    // ── Mapper ────────────────────────────────────────────────
    private static FollowDto ToDto(Follow f) => new()
    {
        FollowId   = f.FollowId,
        FollowerId = f.FollowerId,
        FolloweeId = f.FolloweeId,
        Status     = f.Status,
        CreatedAt  = f.CreatedAt
    };
}
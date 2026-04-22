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
        // Check if already following
        var existing = await _repo.FindByFollowerAndFollowee(followerId, followeeId);
        if (existing != null) return ToDto(existing);

        // Public = ACCEPTED immediately, Private = PENDING
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

        // Update counters only if accepted immediately
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
        var follow = await _repo.FindByFollowerAndFollowee(0, 0); // placeholder
        // Use direct DB lookup via repo
        follow = (await _repo.FindPendingRequests(0))
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

    public async Task<List<FollowDto>> GetFollowers(int userId)
        => (await _repo.FindFollowersByUserId(userId)).Select(ToDto).ToList();

    public async Task<List<FollowDto>> GetFollowing(int userId)
        => (await _repo.FindFollowingByUserId(userId)).Select(ToDto).ToList();

    public async Task<List<FollowDto>> GetPendingRequests(int userId)
        => (await _repo.FindPendingRequests(userId)).Select(ToDto).ToList();

    public async Task<bool> IsFollowing(int followerId, int followeeId)
        => await _repo.IsFollowing(followerId, followeeId);

    public async Task<int> GetFollowerCount(int userId)
        => await _repo.CountFollowers(userId);

    public async Task<int> GetFollowingCount(int userId)
        => await _repo.CountFollowing(userId);

    public async Task<List<int>> GetFollowingIds(int userId)
    {
        var following = await _repo.FindFollowingByUserId(userId);
        return following.Select(f => f.FolloweeId).ToList();
    }

    public async Task<List<FollowDto>> GetMutualFollowers(int userId)
    {
        var followers  = await _repo.FindFollowersByUserId(userId);
        var following  = await _repo.FindFollowingByUserId(userId);

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
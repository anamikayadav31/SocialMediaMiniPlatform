public interface IFollowService
{
    /// <summary>Follow a user. Public = ACCEPTED immediately. Private = PENDING.</summary>
    Task<FollowDto>       FollowUser(int followerId, int followeeId);

    Task                  UnfollowUser(int followerId, int followeeId);

    Task<FollowDto?>      AcceptFollowRequest(int followId);
    Task<FollowDto?>      RejectFollowRequest(int followId);

    Task<List<FollowDto>> GetFollowers(int userId);
    Task<List<FollowDto>> GetFollowing(int userId);
    Task<List<FollowDto>> GetPendingRequests(int userId);

    Task<bool>            IsFollowing(int followerId, int followeeId);

    Task<int>             GetFollowerCount(int userId);
    Task<int>             GetFollowingCount(int userId);

    /// <summary>Returns accepted followee IDs — used by FeedService.</summary>
    Task<List<int>>       GetFollowingIds(int userId);

    /// <summary>Returns mutual followers (both follow each other).</summary>
    Task<List<FollowDto>> GetMutualFollowers(int userId);
}
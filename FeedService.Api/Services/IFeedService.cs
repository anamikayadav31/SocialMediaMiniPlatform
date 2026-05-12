public interface IFeedService
{
    /// <summary>Home feed: posts from followed users, paginated, Redis cached (5-min TTL).</summary>
    Task<List<FeedItemDto>>        GetFeedForUser(int userId, int page, int pageSize);

    /// <summary>Explore feed: recent public posts from non-followed users ranked by engagement.</summary>
    Task<List<FeedItemDto>>        GetExploreFeed(int userId, int page, int pageSize);

    /// <summary>Fan-out: insert FeedItem for each follower and invalidate their Redis cache.</summary>
    Task                           AddPostToFollowerFeeds(int postId, int authorId, List<int> followerIds);

    /// <summary>Trending hashtags in last 48 hours, grouped by frequency.</summary>
    Task<List<TrendingHashtagDto>> GetTrendingHashtags(int topN);

    /// <summary>Suggested users: mutual followers not yet followed, sorted by mutual count.</summary>
    Task<List<SuggestedUserDto>>   GetSuggestedUsers(int userId);
}
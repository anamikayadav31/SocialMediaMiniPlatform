using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class FeedServiceImpl : IFeedService
{
    private readonly IFeedRepository     _repo;
    private readonly FeedDbContext       _db;
    private readonly IDistributedCache   _cache;

    public FeedServiceImpl(
        IFeedRepository repo,
        FeedDbContext db,
        IDistributedCache cache)
    {
        _repo  = repo;
        _db    = db;
        _cache = cache;
    }

    // ── Home Feed (Redis cached, 5-min TTL) ───────────────────

    public async Task<List<FeedItemDto>> GetFeedForUser(int userId, int page, int pageSize)
    {
        string cacheKey = $"feed:{userId}:page:{page}:size:{pageSize}";

        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return JsonSerializer.Deserialize<List<FeedItemDto>>(cached) ?? new();

        var items = (await _repo.GetFeedForUser(userId, page, pageSize))
            .Select(ToDto).ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(items), options);

        return items;
    }

    // ── Explore Feed ──────────────────────────────────────────

    public async Task<List<FeedItemDto>> GetExploreFeed(int userId, int page, int pageSize)
    {
        var items = await _repo.GetExploreFeed(userId, page, pageSize);
        return items.Select(ToDto).ToList();
    }

    // ── Fan-out ───────────────────────────────────────────────

    public async Task AddPostToFollowerFeeds(int postId, int authorId, List<int> followerIds)
    {
        var feedItems = followerIds.Select(followerId => new FeedItem
        {
            UserId    = followerId,
            PostId    = postId,
            AuthorId  = authorId,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _repo.AddFeedItems(feedItems);
        await _repo.SaveChanges();

        // Invalidate Redis cache for each follower
        foreach (var followerId in followerIds)
        {
            // Invalidate page 1 (most common case)
            await _cache.RemoveAsync($"feed:{followerId}:page:1:size:10");
            await _cache.RemoveAsync($"feed:{followerId}:page:1:size:20");
        }
    }

    // ── Trending Hashtags (last 48 hours) ─────────────────────

    public async Task<List<TrendingHashtagDto>> GetTrendingHashtags(int topN)
    {
        var cutoff = DateTime.UtcNow.AddHours(-48);

        // Pull hashtag strings from DB, split by comma, group and count in memory
        var hashtagStrings = await _db.FeedItems
            .Where(f => f.CreatedAt >= cutoff)
            .Select(f => f.PostId)
            .Distinct()
            .ToListAsync();

        // NOTE: In production this would join PostService DB or use a shared read model.
        // For now returns empty list as FeedService doesn't own Post data.
        // Wire up via HTTP call to PostService or shared DB view.
        return new List<TrendingHashtagDto>();
    }

    // ── Suggested Users ───────────────────────────────────────

    public async Task<List<SuggestedUserDto>> GetSuggestedUsers(int userId)
    {
        // Returns mutual followers not yet followed by userId.
        // In production: call FollowService for followingIds and mutual data.
        // Stub returns empty list — wire up via HTTP call to FollowService.
        return new List<SuggestedUserDto>();
    }

    // ── Mapper ────────────────────────────────────────────────

    private static FeedItemDto ToDto(FeedItem f) => new()
    {
        FeedItemId = f.FeedItemId,
        UserId     = f.UserId,
        PostId     = f.PostId,
        AuthorId   = f.AuthorId,
        CreatedAt  = f.CreatedAt
    };
}
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

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            var json = System.Text.Encoding.UTF8.GetString(cached);
            return JsonSerializer.Deserialize<List<FeedItemDto>>(json) ?? new();
        }

        var items = (await _repo.GetFeedForUser(userId, page, pageSize))
            .Select(ToDto).ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        var bytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));
        await _cache.SetAsync(cacheKey, bytes, options);

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
            await _cache.RemoveAsync($"feed:{followerId}:page:1:size:10");
            await _cache.RemoveAsync($"feed:{followerId}:page:1:size:20");
        }
    }

    // ── Trending Hashtags ─────────────────────────────────────
    // FIX: delegate to _repo so unit tests can mock it
    public async Task<List<TrendingHashtagDto>> GetTrendingHashtags(int topN)
    {
        var since = DateTime.UtcNow.AddHours(-48);
        return await _repo.GetTrendingHashtags(topN, since);
    }

    // ── Suggested Users ───────────────────────────────────────
    // FIX: delegate to _repo so unit tests can mock it
    public async Task<List<SuggestedUserDto>> GetSuggestedUsers(int userId)
        => await _repo.GetSuggestedUsers(userId);

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
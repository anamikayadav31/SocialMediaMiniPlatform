using Microsoft.EntityFrameworkCore;

public class FeedRepository : IFeedRepository
{
    private readonly FeedDbContext _db;

    public FeedRepository(FeedDbContext db) { _db = db; }

    public async Task<List<FeedItem>> GetFeedForUser(int userId, int page, int pageSize)
        => await _db.FeedItems
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<FeedItem>> GetExploreFeed(int userId, int page, int pageSize)
        // Returns posts not already in this user's feed (from non-followed users)
        => await _db.FeedItems
            .Where(f => f.UserId != userId)
            .GroupBy(f => f.PostId)
            .Select(g => g.First())
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task AddFeedItem(FeedItem item)
        => await _db.FeedItems.AddAsync(item);

    public async Task AddFeedItems(List<FeedItem> items)
        => await _db.FeedItems.AddRangeAsync(items);

    public async Task SaveChanges()
        => await _db.SaveChangesAsync();
}
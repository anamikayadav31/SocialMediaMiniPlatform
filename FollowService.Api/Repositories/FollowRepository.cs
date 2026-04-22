using Microsoft.EntityFrameworkCore;

public class FollowRepository : IFollowRepository
{
    private readonly FollowDbContext _db;

    public FollowRepository(FollowDbContext db) { _db = db; }

    public async Task<Follow?> FindByFollowerAndFollowee(int followerId, int followeeId)
        => await _db.Follows.FirstOrDefaultAsync(f =>
            f.FollowerId == followerId && f.FolloweeId == followeeId);

    public async Task<List<Follow>> FindFollowersByUserId(int userId)
        => await _db.Follows
            .Where(f => f.FolloweeId == userId && f.Status == FollowStatus.ACCEPTED)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<List<Follow>> FindFollowingByUserId(int userId)
        => await _db.Follows
            .Where(f => f.FollowerId == userId && f.Status == FollowStatus.ACCEPTED)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<List<Follow>> FindPendingRequests(int userId)
        => await _db.Follows
            .Where(f => f.FolloweeId == userId && f.Status == FollowStatus.PENDING)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<bool> IsFollowing(int followerId, int followeeId)
        => await _db.Follows.AnyAsync(f =>
            f.FollowerId == followerId &&
            f.FolloweeId == followeeId &&
            f.Status == FollowStatus.ACCEPTED);

    public async Task<int> CountFollowers(int userId)
        => await _db.Follows.CountAsync(f =>
            f.FolloweeId == userId && f.Status == FollowStatus.ACCEPTED);

    public async Task<int> CountFollowing(int userId)
        => await _db.Follows.CountAsync(f =>
            f.FollowerId == userId && f.Status == FollowStatus.ACCEPTED);

    public async Task AddFollow(Follow follow)
        => await _db.Follows.AddAsync(follow);

    public async Task DeleteFollowById(int followId)
        => await _db.Follows
            .Where(f => f.FollowId == followId)
            .ExecuteDeleteAsync();

    public async Task SaveChanges()
        => await _db.SaveChangesAsync();
}
using Microsoft.EntityFrameworkCore;

public class LikeRepository : ILikeRepository
{
    private readonly LikeDbContext _db;
    public LikeRepository(LikeDbContext db) { _db = db; }

    public async Task<Like?> FindByLikeId(int likeId)
        => await _db.Likes.FirstOrDefaultAsync(l => l.LikeId == likeId);

    public async Task<Like?> FindByUserAndTarget(int userId, int targetId, TargetType targetType)
        => await _db.Likes.FirstOrDefaultAsync(l =>
            l.UserId     == userId     &&
            l.TargetId   == targetId   &&
            l.TargetType == targetType);

    // Alias for FindByUserAndTarget
    public Task<Like?> FindLike(int userId, int targetId, TargetType targetType)
        => FindByUserAndTarget(userId, targetId, targetType);

    public async Task<List<Like>> FindByTargetId(int targetId, TargetType targetType)
        => await _db.Likes
            .Where(l => l.TargetId == targetId && l.TargetType == targetType)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    // Alias for FindByTargetId
    public Task<List<Like>> FindByTarget(int targetId, TargetType targetType)
        => FindByTargetId(targetId, targetType);

    public async Task<List<Like>> FindByUserId(int userId)
        => await _db.Likes
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    // Alias for FindByUserId
    public Task<List<Like>> FindByUser(int userId)
        => FindByUserId(userId);

    public async Task<int> CountByTargetId(int targetId, TargetType targetType)
        => await _db.Likes.CountAsync(l => l.TargetId == targetId && l.TargetType == targetType);

    public async Task<bool> HasLiked(int userId, int targetId, TargetType targetType)
        => await _db.Likes.AnyAsync(l =>
            l.UserId     == userId     &&
            l.TargetId   == targetId   &&
            l.TargetType == targetType);

    public async Task AddLike(Like like)
        => await _db.Likes.AddAsync(like);

    public async Task DeleteByLikeId(int likeId)
        => await _db.Likes
            .Where(l => l.LikeId == likeId)
            .ExecuteDeleteAsync();

    public Task DeleteLike(Like like)
    {
        _db.Likes.Remove(like);
        return Task.CompletedTask;
    }

    public async Task<List<int>> GetLikerIdsByPost(int postId)
        => await _db.Likes
            .Where(l => l.TargetId == postId && l.TargetType == TargetType.POST)
            .Select(l => l.UserId)
            .ToListAsync();

    public async Task<List<int>> GetLikedPostIds(int userId)
        => await _db.Likes
            .Where(l => l.UserId == userId && l.TargetType == TargetType.POST)
            .Select(l => l.TargetId)
            .ToListAsync();

    public async Task SaveChanges()
        => await _db.SaveChangesAsync();
}
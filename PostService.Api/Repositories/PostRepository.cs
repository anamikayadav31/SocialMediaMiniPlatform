using Microsoft.EntityFrameworkCore;

public class PostRepository : IPostRepository
{
    private readonly PostDbContext _db;
    public PostRepository(PostDbContext db) { _db = db; }

    public async Task<Post?> FindById(int postId)
        => await _db.Posts.FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

    public async Task<List<Post>> FindByUserId(int userId)
        => await _db.Posts
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<List<Post>> FindPublic(int page, int pageSize)
        => await _db.Posts
            .Where(p => p.Visibility == Visibility.PUBLIC && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<Post>> FindByHashtag(string tag, int page, int pageSize)
    {
        string pattern = $"%{tag}%";
        return await _db.Posts
            .Where(p => !p.IsDeleted &&
                        p.Visibility == Visibility.PUBLIC &&
                        EF.Functions.Like(p.Hashtags, pattern))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Post>> SearchPosts(string query, int page, int pageSize)
    {
        string pattern = $"%{query}%";
        return await _db.Posts
            .Where(p => !p.IsDeleted &&
                        EF.Functions.Like(p.Content, pattern))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Post>> FindTrending(int hoursBack, int take)
    {
        var since = DateTime.UtcNow.AddHours(-hoursBack);
        return await _db.Posts
            .Where(p => !p.IsDeleted &&
                        p.Visibility == Visibility.PUBLIC &&
                        p.CreatedAt >= since)
            .OrderByDescending(p => p.LikeCount * 3 + p.CommentCount * 2 + p.ShareCount)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Post>> FindFeedForUser(List<int> followingIds, int page, int pageSize)
        => await _db.Posts
            .Where(p => followingIds.Contains(p.UserId) &&
                        !p.IsDeleted &&
                        p.Visibility != Visibility.PRIVATE)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<Post>> FindExploreFeed(List<int> excludeUserIds, int page, int pageSize)
        => await _db.Posts
            .Where(p => !excludeUserIds.Contains(p.UserId) &&
                        !p.IsDeleted &&
                        p.Visibility == Visibility.PUBLIC)
            .OrderByDescending(p => p.LikeCount * 3 + p.CommentCount * 2 + p.ShareCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<Post>> FindTimeline(int userId, int page, int pageSize)
        => await _db.Posts
            .Where(p => p.UserId == userId &&
                        !p.IsDeleted &&
                        p.Visibility == Visibility.PUBLIC)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task AddPost(Post post) => await _db.Posts.AddAsync(post);

    public void UpdatePost(Post post) => _db.Posts.Update(post);

    public async Task IncrementCount(int postId, string field, int delta)
    {
        if (field == "LikeCount")
            await _db.Posts.Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikeCount, p => p.LikeCount + delta));

        else if (field == "CommentCount")
            await _db.Posts.Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentCount, p => p.CommentCount + delta));

        else if (field == "ShareCount")
            await _db.Posts.Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ShareCount, p => p.ShareCount + delta));
    }

    public async Task SoftDelete(int postId)
        => await _db.Posts.Where(p => p.PostId == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true));

    public async Task SaveChanges() => await _db.SaveChangesAsync();
}
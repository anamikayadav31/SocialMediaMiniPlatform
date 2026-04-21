using Microsoft.EntityFrameworkCore;

public class CommentRepository : ICommentRepository
{
    private readonly CommentDbContext _db;

    public CommentRepository(CommentDbContext db) { _db = db; }

    public async Task<Comment?> FindByCommentId(int commentId)
        => await _db.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

    public async Task<List<Comment>> FindByPostId(int postId)
        => await _db.Comments
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<List<Comment>> FindReplies(int parentCommentId)
        => await _db.Comments
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public async Task<List<Comment>> FindByUserId(int userId)
        => await _db.Comments
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<List<Comment>> FindTopLevelByPostId(int postId)
        => await _db.Comments
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<int> CountByPostId(int postId)
        => await _db.Comments.CountAsync(c => c.PostId == postId && !c.IsDeleted);

    public async Task AddComment(Comment comment)
        => await _db.Comments.AddAsync(comment);

    public async Task DeleteCommentById(int commentId)
        => await _db.Comments
            .Where(c => c.CommentId == commentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IsDeleted, true)
                .SetProperty(c => c.Content, "This comment was deleted."));

    public async Task IncrementLikeCount(int commentId)
        => await _db.Comments
            .Where(c => c.CommentId == commentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.LikeCount, c => c.LikeCount + 1));

    public async Task IncrementCommentCount(int postId)
        // Stub — replace with HTTP call to PostService if needed
        => await Task.CompletedTask;

    public async Task SaveChanges()
        => await _db.SaveChangesAsync();
}
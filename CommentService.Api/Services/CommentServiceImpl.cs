public class CommentServiceImpl : ICommentService
{
    private readonly ICommentRepository _repo;
    private readonly INotifService      _notif;
    private readonly IPostService       _postSvc;

    public CommentServiceImpl(
        ICommentRepository repo,
        INotifService notif,
        IPostService postSvc)
    {
        _repo    = repo;
        _notif   = notif;
        _postSvc = postSvc;
    }

    // ── Add ───────────────────────────────────────────────────

    public async Task<CommentDto> AddComment(AddCommentDto dto)
    {
        var comment = new Comment
        {
            PostId          = dto.PostId,
            UserId          = dto.UserId,
            ParentCommentId = dto.ParentCommentId,
            Content         = dto.Content,
            CreatedAt       = DateTime.UtcNow
        };

        await _repo.AddComment(comment);
        await _repo.SaveChanges();

        // If reply, increment parent's ReplyCount
        if (dto.ParentCommentId.HasValue)
        {
            var parent = await _repo.FindByCommentId(dto.ParentCommentId.Value);
            if (parent != null)
            {
                parent.ReplyCount++;
                await _repo.SaveChanges();
            }
        }

        // Notify post author
        await _postSvc.IncrementCommentCount(dto.PostId);
        if (dto.PostOwnerId.HasValue)
            await _notif.SendCommentNotif(dto.UserId, dto.PostId, comment.CommentId, dto.PostOwnerId.Value);
        else
            await _notif.SendCommentNotif(dto.UserId, dto.PostId, comment.CommentId);

        return ToDto(comment);
    }

    // ── Queries ───────────────────────────────────────────────

    public async Task<CommentDto?> GetCommentById(int commentId)
    {
        var c = await _repo.FindByCommentId(commentId);
        return c == null ? null : ToDto(c);
    }

    public async Task<List<CommentDto>> GetCommentsByPost(int postId)
        => (await _repo.FindByPostId(postId)).Select(ToDto).ToList();

    public async Task<List<CommentDto>> GetTopLevelComments(int postId)
        => (await _repo.FindTopLevelByPostId(postId)).Select(ToDto).ToList();

    public async Task<List<CommentDto>> GetReplies(int commentId)
        => (await _repo.FindReplies(commentId)).Select(ToDto).ToList();

    public async Task<List<CommentDto>> GetCommentsByUser(int userId)
        => (await _repo.FindByUserId(userId)).Select(ToDto).ToList();

    public async Task<int> GetCommentCount(int postId)
        => await _repo.CountByPostId(postId);

    // ── Edit ──────────────────────────────────────────────────

    public async Task<CommentDto?> EditComment(int commentId, string content)
    {
        var comment = await _repo.FindByCommentId(commentId);
        if (comment == null || comment.IsDeleted) return null;

        comment.Content  = content;
        comment.IsEdited = true;
        comment.EditedAt = DateTime.UtcNow;

        await _repo.SaveChanges();
        return ToDto(comment);
    }

    // ── Delete (soft) ─────────────────────────────────────────

    public async Task DeleteComment(int commentId)
        => await _repo.DeleteCommentById(commentId);
        // Sets IsDeleted = true, Content = "This comment was deleted."

    // ── Like ──────────────────────────────────────────────────

    public async Task IncrementLikeCount(int commentId)
        => await _repo.IncrementLikeCount(commentId);

    // ── Mapper ────────────────────────────────────────────────

    private static CommentDto ToDto(Comment c) => new()
    {
        CommentId       = c.CommentId,
        PostId          = c.PostId,
        UserId          = c.UserId,
        ParentCommentId = c.ParentCommentId,
        Content         = c.Content,
        LikeCount       = c.LikeCount,
        ReplyCount      = c.ReplyCount,
        IsDeleted       = c.IsDeleted,
        IsEdited        = c.IsEdited,
        CreatedAt       = c.CreatedAt,
        EditedAt        = c.EditedAt
    };
}
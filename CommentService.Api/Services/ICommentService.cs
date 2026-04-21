public interface ICommentService
{
    Task<CommentDto>       AddComment(AddCommentDto dto);
    Task<CommentDto?>      GetCommentById(int commentId);
    Task<List<CommentDto>> GetCommentsByPost(int postId);
    Task<List<CommentDto>> GetTopLevelComments(int postId);   // ParentCommentId IS NULL
    Task<List<CommentDto>> GetReplies(int commentId);         // ParentCommentId = commentId
    Task<List<CommentDto>> GetCommentsByUser(int userId);
    Task<CommentDto?>      EditComment(int commentId, string content);
    Task                   DeleteComment(int commentId);      // soft delete
    Task<int>              GetCommentCount(int postId);
    Task                   IncrementLikeCount(int commentId);
}
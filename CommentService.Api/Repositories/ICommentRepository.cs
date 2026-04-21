public interface ICommentRepository
{
    Task<Comment?>       FindByCommentId(int commentId);
    Task<List<Comment>>  FindByPostId(int postId);
    Task<List<Comment>>  FindReplies(int parentCommentId);
    Task<List<Comment>>  FindByUserId(int userId);
    Task<List<Comment>>  FindTopLevelByPostId(int postId);   // WHERE ParentCommentId IS NULL
    Task<int>            CountByPostId(int postId);
    Task                 AddComment(Comment comment);
    Task                 DeleteCommentById(int commentId);
    Task                 IncrementLikeCount(int commentId);
    Task                 IncrementCommentCount(int postId);  // calls post service
    Task                 SaveChanges();
}
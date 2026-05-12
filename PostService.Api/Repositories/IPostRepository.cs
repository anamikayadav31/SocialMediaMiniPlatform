public interface IPostRepository
{
    Task<Post?>      FindById(int postId);
    Task<List<Post>> FindByUserId(int userId);
    Task<List<Post>> FindPublic(int page, int pageSize);
    Task<List<Post>> FindByHashtag(string tag, int page, int pageSize);
    Task<List<Post>> SearchPosts(string query, int page, int pageSize);
    Task<List<Post>> SearchByContent(string query, int page, int pageSize);            // alias for SearchPosts
    Task<List<Post>> FindTrending(int hoursBack, int take);
    Task<List<Post>> FindFeedForUser(List<int> followingIds, int page, int pageSize);
    Task<List<Post>> FindExploreFeed(List<int> excludeUserIds, int page, int pageSize);
    Task<List<Post>> FindTimeline(int userId, int page, int pageSize);
    Task             AddPost(Post post);
    void             UpdatePost(Post post);
    Task             IncrementCount(int postId, string field, int delta);
    Task             IncrementField(int postId, string field, int delta);              // alias for IncrementCount
    Task             SoftDelete(int postId);
    Task             SaveChanges();
}
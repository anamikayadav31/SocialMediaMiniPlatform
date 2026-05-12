public interface IPostService
{
    Task<PostDto?>      CreatePost(CreatePostDto dto);
    Task<PostDto?>      GetPostById(int postId);
    Task<List<PostDto>> GetPostsByUser(int userId);
    Task<List<PostDto>> GetPublicPosts(int page, int pageSize);
    Task<PostDto?>      UpdatePost(int postId, UpdatePostDto dto);
    Task<bool>          DeletePost(int postId);
    Task<List<PostDto>> GetByHashtag(string tag, int page, int pageSize);
    Task<List<PostDto>> SearchPosts(string query, int page, int pageSize);
    Task<List<PostDto>> GetTrendingPosts(int hoursBack = 24, int take = 20);
    Task<List<PostDto>> GetFeedForUser(List<int> followingIds, int page, int pageSize);
    Task<List<PostDto>> GetExploreFeed(List<int> excludeUserIds, int page, int pageSize);
    Task<List<PostDto>> GetTimeline(int userId, int page, int pageSize);
    Task                IncrementCount(int postId, string field, int delta);
}
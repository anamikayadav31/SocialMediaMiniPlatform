public class PostServiceImpl : IPostService
{
    private readonly IPostRepository _repo;
    public PostServiceImpl(IPostRepository repo) { _repo = repo; }

    public async Task<PostDto?> CreatePost(CreatePostDto dto)
    {
        var post = new Post
        {
            UserId         = dto.UserId,
            Content        = dto.Content.Trim(),
            MediaUrl       = dto.MediaUrl,
            MediaType      = dto.MediaType,
            Visibility     = dto.Visibility,
            Hashtags       = dto.Hashtags.Trim(),
            OriginalPostId = dto.OriginalPostId,
            CreatedAt      = DateTime.UtcNow
        };

        await _repo.AddPost(post);
        await _repo.SaveChanges();
        return ToDto(post);
    }

    public async Task<PostDto?> GetPostById(int postId)
    {
        var post = await _repo.FindById(postId);
        return post == null ? null : ToDto(post);
    }

    public async Task<List<PostDto>> GetPostsByUser(int userId)
        => (await _repo.FindByUserId(userId)).Select(ToDto).ToList();

    public async Task<List<PostDto>> GetPublicPosts(int page, int pageSize)
        => (await _repo.FindPublic(page, pageSize)).Select(ToDto).ToList();

    public async Task<PostDto?> UpdatePost(int postId, UpdatePostDto dto)
    {
        var post = await _repo.FindById(postId);
        if (post == null) return null;

        if (dto.Content    != null) post.Content    = dto.Content.Trim();
        if (dto.MediaUrl   != null) post.MediaUrl   = dto.MediaUrl;
        if (dto.Visibility != null) post.Visibility = dto.Visibility.Value;
        if (dto.Hashtags   != null) post.Hashtags   = dto.Hashtags.Trim();

        post.IsEdited  = true;
        post.UpdatedAt = DateTime.UtcNow;

        _repo.UpdatePost(post);
        await _repo.SaveChanges();
        return ToDto(post);
    }

    public async Task<bool> DeletePost(int postId)
    {
        var post = await _repo.FindById(postId);
        if (post == null) return false;

        await _repo.SoftDelete(postId);
        return true;
    }

    public async Task<List<PostDto>> GetByHashtag(string tag, int page, int pageSize)
        => (await _repo.FindByHashtag(tag, page, pageSize)).Select(ToDto).ToList();

    public async Task<List<PostDto>> SearchPosts(string query, int page, int pageSize)
        => (await _repo.SearchPosts(query, page, pageSize)).Select(ToDto).ToList();

    public async Task<List<PostDto>> GetTrendingPosts(int hoursBack = 24, int take = 20)
        => (await _repo.FindTrending(hoursBack, take)).Select(ToDto).ToList();

    public async Task<List<PostDto>> GetFeedForUser(List<int> followingIds, int page, int pageSize)
        => (await _repo.FindFeedForUser(followingIds, page, pageSize)).Select(ToDto).ToList();

    public async Task<List<PostDto>> GetExploreFeed(List<int> excludeUserIds, int page, int pageSize)
        => (await _repo.FindExploreFeed(excludeUserIds, page, pageSize)).Select(ToDto).ToList();

    public async Task<List<PostDto>> GetTimeline(int userId, int page, int pageSize)
        => (await _repo.FindTimeline(userId, page, pageSize)).Select(ToDto).ToList();

    public async Task IncrementCount(int postId, string field, int delta)
        => await _repo.IncrementCount(postId, field, delta);

    private static PostDto ToDto(Post p) => new()
    {
        PostId         = p.PostId,
        UserId         = p.UserId,
        Content        = p.Content,
        MediaUrl       = p.MediaUrl,
        MediaType      = p.MediaType,
        Visibility     = p.Visibility,
        Hashtags       = p.Hashtags,
        LikeCount      = p.LikeCount,
        CommentCount   = p.CommentCount,
        ShareCount     = p.ShareCount,
        IsDeleted      = p.IsDeleted,
        IsEdited       = p.IsEdited,
        OriginalPostId = p.OriginalPostId,
        CreatedAt      = p.CreatedAt,
        UpdatedAt      = p.UpdatedAt
    };
}
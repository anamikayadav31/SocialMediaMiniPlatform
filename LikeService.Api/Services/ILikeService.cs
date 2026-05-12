public interface ILikeService
{
    /// <summary>Toggle like on a post or comment. Returns liked=true/false + new count.</summary>
    Task<ToggleLikeResultDto> ToggleLike(int userId, int targetId, TargetType targetType, int? ownerId = null);

    Task               AddLike(int userId, int targetId, TargetType targetType);
    Task               RemoveLike(int userId, int targetId, TargetType targetType);

    Task<List<LikeDto>> GetLikesByTarget(int targetId, TargetType targetType);
    Task<List<LikeDto>> GetLikesByUser(int userId);

    Task<int>           GetLikeCount(int targetId, TargetType targetType);
    Task<bool>          HasUserLiked(int userId, int targetId, TargetType targetType);

    /// <summary>Returns list of userIds who liked a post — used for the likers modal.</summary>
    Task<List<int>>     GetLikersForPost(int postId);

    /// <summary>Returns list of postIds liked by a user.</summary>
    Task<List<int>>     GetLikedPostsByUser(int userId);
}
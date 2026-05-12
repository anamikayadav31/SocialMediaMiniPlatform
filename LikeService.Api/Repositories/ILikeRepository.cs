public interface ILikeRepository
{
    Task<Like?>        FindByLikeId(int likeId);
    Task<Like?>        FindByUserAndTarget(int userId, int targetId, TargetType targetType);
    Task<Like?>        FindLike(int userId, int targetId, TargetType targetType);       // alias for FindByUserAndTarget
    Task<List<Like>>   FindByTargetId(int targetId, TargetType targetType);
    Task<List<Like>>   FindByTarget(int targetId, TargetType targetType);               // alias for FindByTargetId
    Task<List<Like>>   FindByUserId(int userId);
    Task<List<Like>>   FindByUser(int userId);                                          // alias for FindByUserId
    Task<int>          CountByTargetId(int targetId, TargetType targetType);
    Task<bool>         HasLiked(int userId, int targetId, TargetType targetType);
    Task               AddLike(Like like);
    Task               DeleteByLikeId(int likeId);
    Task               DeleteLike(Like like);                                           // delete by entity
    Task<List<int>>    GetLikerIdsByPost(int postId);                                  // returns user ids who liked a post
    Task<List<int>>    GetLikedPostIds(int userId);                                    // returns post ids liked by user
    Task               SaveChanges();
}
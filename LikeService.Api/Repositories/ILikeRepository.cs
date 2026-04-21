public interface ILikeRepository
{
    Task<Like?>        FindByLikeId(int likeId);
    Task<Like?>        FindByUserAndTarget(int userId, int targetId, TargetType targetType);
    Task<List<Like>>   FindByTargetId(int targetId, TargetType targetType);
    Task<List<Like>>   FindByUserId(int userId);
    Task<int>          CountByTargetId(int targetId, TargetType targetType);
    Task<bool>         HasLiked(int userId, int targetId, TargetType targetType);
    Task               AddLike(Like like);
    Task               DeleteByLikeId(int likeId);
    Task               SaveChanges();
}
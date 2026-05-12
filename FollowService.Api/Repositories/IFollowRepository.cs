public interface IFollowRepository
{
    Task<Follow?>       FindByFollowerAndFollowee(int followerId, int followeeId);
    Task<List<Follow>>  FindFollowersByUserId(int userId);
    Task<List<Follow>>  FindFollowers(int userId);                                     // alias for FindFollowersByUserId
    Task<List<Follow>>  FindFollowingByUserId(int userId);
    Task<List<Follow>>  FindFollowing(int userId);                                     // alias for FindFollowingByUserId
    Task<List<Follow>>  FindPendingRequests(int userId);
    Task<bool>          IsFollowing(int followerId, int followeeId);
    Task<bool>          ExistsAccepted(int followerId, int followeeId);                // alias for IsFollowing
    Task<int>           CountFollowers(int userId);
    Task<int>           CountFollowing(int userId);
    Task<List<int>>     GetAcceptedFolloweeIds(int userId);                           // ids of users being followed
    Task                AddFollow(Follow follow);
    Task                DeleteFollowById(int followId);
    Task                SaveChanges();
}
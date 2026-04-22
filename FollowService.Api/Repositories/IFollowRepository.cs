public interface IFollowRepository
{
    Task<Follow?>       FindByFollowerAndFollowee(int followerId, int followeeId);
    Task<List<Follow>>  FindFollowersByUserId(int userId);      // people following userId
    Task<List<Follow>>  FindFollowingByUserId(int userId);      // people userId follows
    Task<List<Follow>>  FindPendingRequests(int userId);        // incoming pending requests
    Task<bool>          IsFollowing(int followerId, int followeeId);
    Task<int>           CountFollowers(int userId);
    Task<int>           CountFollowing(int userId);
    Task                AddFollow(Follow follow);
    Task                DeleteFollowById(int followId);
    Task                SaveChanges();
}
public interface IFeedRepository
{
    Task<List<FeedItem>> GetFeedForUser(int userId, int page, int pageSize);
    Task<List<FeedItem>> GetExploreFeed(int userId, int page, int pageSize);
    Task                 AddFeedItem(FeedItem item);
    Task                 AddFeedItems(List<FeedItem> items);
    Task                 SaveChanges();
}
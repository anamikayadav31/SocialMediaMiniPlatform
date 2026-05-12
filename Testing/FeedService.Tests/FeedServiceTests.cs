using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using System.Text;
using System.Text.Json;

// ============================================================
//  FeedService Tests — FeedServiceImpl ki methods test karo
// ============================================================

[TestFixture]
public class FeedServiceTests
{
    private Mock<IFeedRepository>    _repoMock;
    private Mock<IDistributedCache>  _cacheMock;
    private Mock<FeedDbContext>      _dbMock;
    private FeedServiceImpl          _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock  = new Mock<IFeedRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _dbMock    = new Mock<FeedDbContext>();

        _sut = new FeedServiceImpl(
            _repoMock.Object,
            _dbMock.Object,
            _cacheMock.Object
        );
    }

    // ─────────────────────────────────────────────────────────
    //  GET FEED FOR USER — CACHE MISS (DB se fetch)
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetFeedForUser_CacheMiss_FetchesFromDbAndCaches()
    {
        // Arrange — cache mein kuch nahi hai (null return)
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                  .ReturnsAsync((byte[]?)null);

        var feedItems = new List<FeedItem>
        {
            new FeedItem { FeedItemId = 1, UserId = 5, PostId = 100, AuthorId = 3, CreatedAt = DateTime.UtcNow },
            new FeedItem { FeedItemId = 2, UserId = 5, PostId = 101, AuthorId = 4, CreatedAt = DateTime.UtcNow }
        };
        _repoMock.Setup(r => r.GetFeedForUser(5, 1, 10)).ReturnsAsync(feedItems);
        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.GetFeedForUser(5, 1, 10);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        // Cache mein store hona chahiye tha
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
    }

    [Test]
    public async Task GetFeedForUser_CacheHit_ReturnsFromCacheWithoutDbCall()
    {
        // Arrange — cache mein data hai
        var cachedItems = new List<FeedItemDto>
        {
            new FeedItemDto { FeedItemId = 10, UserId = 5, PostId = 200, AuthorId = 6 }
        };
        var json  = JsonSerializer.Serialize(cachedItems);
        var bytes = Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                  .ReturnsAsync(bytes);

        // Act
        var result = await _sut.GetFeedForUser(5, 1, 10);

        // Assert — DB call nahi hona chahiye
        Assert.That(result.Count, Is.EqualTo(1));
        _repoMock.Verify(r => r.GetFeedForUser(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    //  GET EXPLORE FEED TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetExploreFeed_ReturnsNonFollowedUserPosts()
    {
        var items = new List<FeedItem>
        {
            new FeedItem { FeedItemId = 20, UserId = 7, PostId = 300, AuthorId = 99 },
            new FeedItem { FeedItemId = 21, UserId = 7, PostId = 301, AuthorId = 98 }
        };
        _repoMock.Setup(r => r.GetExploreFeed(7, 1, 10)).ReturnsAsync(items);

        var result = await _sut.GetExploreFeed(7, 1, 10);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    // ─────────────────────────────────────────────────────────
    //  ADD POST TO FOLLOWER FEEDS (FAN-OUT) TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task AddPostToFollowerFeeds_CreatesOneFeedItemPerFollower()
    {
        List<FeedItem>? savedItems = null;

        _repoMock.Setup(r => r.AddFeedItems(It.IsAny<List<FeedItem>>()))
            .Callback<List<FeedItem>>(items => savedItems = items)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        var followerIds = new List<int> { 10, 20, 30 };

        // Act
        await _sut.AddPostToFollowerFeeds(postId: 50, authorId: 1, followerIds: followerIds);

        // Assert — 3 followers ke liye 3 feed items banana chahiye
        Assert.That(savedItems, Is.Not.Null);
        Assert.That(savedItems!.Count, Is.EqualTo(3));
        Assert.That(savedItems.All(fi => fi.PostId == 50 && fi.AuthorId == 1), Is.True);
    }

    [Test]
    public async Task AddPostToFollowerFeeds_InvalidatesCacheForEachFollower()
    {
        _repoMock.Setup(r => r.AddFeedItems(It.IsAny<List<FeedItem>>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var removedKeys = new List<string>();
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), default))
            .Callback<string, CancellationToken>((key, _) => removedKeys.Add(key))
            .Returns(Task.CompletedTask);

        var followerIds = new List<int> { 1, 2 };

        await _sut.AddPostToFollowerFeeds(postId: 10, authorId: 5, followerIds: followerIds);

        // 2 followers × 2 cache keys (page 1 size 10 + page 1 size 20) = 4 remove calls
        Assert.That(removedKeys.Count, Is.EqualTo(4));
    }

    // ─────────────────────────────────────────────────────────
    //  GET TRENDING HASHTAGS TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetTrendingHashtags_ReturnsTopN()
    {
        var hashtags = new List<TrendingHashtagDto>
        {
            new TrendingHashtagDto { Hashtag = "#travel", Count = 50 },
            new TrendingHashtagDto { Hashtag = "#food",   Count = 30 },
            new TrendingHashtagDto { Hashtag = "#tech",   Count = 20 }
        };
        _repoMock.Setup(r => r.GetTrendingHashtags(3, It.IsAny<DateTime>())).ReturnsAsync(hashtags);

        var result = await _sut.GetTrendingHashtags(3);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.First().Hashtag, Is.EqualTo("#travel"));
    }

    // ─────────────────────────────────────────────────────────
    //  GET SUGGESTED USERS TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetSuggestedUsers_ReturnsMutualFollowerSuggestions()
    {
        var suggestions = new List<SuggestedUserDto>
        {
            new SuggestedUserDto { UserId = 100, MutualFollowers = 5 },
            new SuggestedUserDto { UserId = 101, MutualFollowers = 3 }
        };
        _repoMock.Setup(r => r.GetSuggestedUsers(5)).ReturnsAsync(suggestions);

        var result = await _sut.GetSuggestedUsers(5);

        Assert.That(result.Count, Is.EqualTo(2));
        // Zyada mutual followers wala pehle hona chahiye
        Assert.That(result.First().MutualFollowers, Is.GreaterThanOrEqualTo(result.Last().MutualFollowers));
    }
}

using Moq;
using NUnit.Framework;

// ============================================================
//  FollowService Tests — FollowServiceImpl ki methods test karo
// ============================================================

[TestFixture]
public class FollowServiceTests
{
    private Mock<IFollowRepository> _repoMock;
    private Mock<IUserService>      _userSvcMock;
    private Mock<INotifService>     _notifMock;
    private FollowServiceImpl       _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock    = new Mock<IFollowRepository>();
        _userSvcMock = new Mock<IUserService>();
        _notifMock   = new Mock<INotifService>();

        _sut = new FollowServiceImpl(
            _repoMock.Object,
            _userSvcMock.Object,
            _notifMock.Object
        );
    }

    // ─────────────────────────────────────────────────────────
    //  FOLLOW USER TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task FollowUser_PublicAccount_CreatesAcceptedFollow()
    {
        // Arrange — public account (IsPrivate = false)
        _repoMock.Setup(r => r.FindByFollowerAndFollowee(1, 2)).ReturnsAsync((Follow?)null);
        _userSvcMock.Setup(u => u.IsPrivate(2)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddFollow(It.IsAny<Follow>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);
        _userSvcMock.Setup(u => u.UpdateCounters(1, 2, true)).Returns(Task.CompletedTask);
        _notifMock.Setup(n => n.SendFollowNotif(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FollowStatus>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.FollowUser(1, 2);

        // Assert — public account toh immediately ACCEPTED hona chahiye
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(FollowStatus.ACCEPTED));
        Assert.That(result.FollowerId, Is.EqualTo(1));
        Assert.That(result.FolloweeId, Is.EqualTo(2));
    }

    [Test]
    public async Task FollowUser_PrivateAccount_CreatesPendingFollow()
    {
        // Arrange — private account
        _repoMock.Setup(r => r.FindByFollowerAndFollowee(3, 4)).ReturnsAsync((Follow?)null);
        _userSvcMock.Setup(u => u.IsPrivate(4)).ReturnsAsync(true);
        _repoMock.Setup(r => r.AddFollow(It.IsAny<Follow>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);
        _notifMock.Setup(n => n.SendFollowNotif(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FollowStatus>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.FollowUser(3, 4);

        // Assert — private account toh PENDING hona chahiye
        Assert.That(result.Status, Is.EqualTo(FollowStatus.PENDING));

        // Counter update nahi hona chahiye jab PENDING ho
        _userSvcMock.Verify(u => u.UpdateCounters(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task FollowUser_AlreadyFollowing_ReturnsExistingFollow()
    {
        // Pehle se follow hai — duplicate nahi banana
        var existing = new Follow { FollowId = 1, FollowerId = 5, FolloweeId = 6, Status = FollowStatus.ACCEPTED };
        _repoMock.Setup(r => r.FindByFollowerAndFollowee(5, 6)).ReturnsAsync(existing);

        var result = await _sut.FollowUser(5, 6);

        Assert.That(result.FollowId, Is.EqualTo(1));
        // AddFollow nahi hona chahiye duplicate ke case mein
        _repoMock.Verify(r => r.AddFollow(It.IsAny<Follow>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    //  UNFOLLOW TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task UnfollowUser_ExistingFollow_DeletesAndUpdatesCounters()
    {
        var follow = new Follow { FollowId = 10, FollowerId = 1, FolloweeId = 2, Status = FollowStatus.ACCEPTED };
        _repoMock.Setup(r => r.FindByFollowerAndFollowee(1, 2)).ReturnsAsync(follow);
        _repoMock.Setup(r => r.DeleteFollowById(10)).Returns(Task.CompletedTask);
        _userSvcMock.Setup(u => u.UpdateCounters(1, 2, false)).Returns(Task.CompletedTask);

        await _sut.UnfollowUser(1, 2);

        _repoMock.Verify(r => r.DeleteFollowById(10), Times.Once);
        _userSvcMock.Verify(u => u.UpdateCounters(1, 2, false), Times.Once);
    }

    [Test]
    public async Task UnfollowUser_NotFollowing_DoesNothing()
    {
        _repoMock.Setup(r => r.FindByFollowerAndFollowee(7, 8)).ReturnsAsync((Follow?)null);

        // Koi exception nahi aana chahiye
        Assert.DoesNotThrowAsync(() => _sut.UnfollowUser(7, 8));

        _repoMock.Verify(r => r.DeleteFollowById(It.IsAny<int>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    //  IS FOLLOWING TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task IsFollowing_WhenFollowExists_ReturnsTrue()
    {
        _repoMock.Setup(r => r.ExistsAccepted(1, 2)).ReturnsAsync(true);

        var result = await _sut.IsFollowing(1, 2);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFollowing_WhenNoFollow_ReturnsFalse()
    {
        _repoMock.Setup(r => r.ExistsAccepted(1, 3)).ReturnsAsync(false);

        var result = await _sut.IsFollowing(1, 3);

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────
    //  GET FOLLOWERS / FOLLOWING TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetFollowers_ReturnsAcceptedFollowers()
    {
        var followers = new List<Follow>
        {
            new Follow { FollowId = 1, FollowerId = 10, FolloweeId = 5, Status = FollowStatus.ACCEPTED },
            new Follow { FollowId = 2, FollowerId = 11, FolloweeId = 5, Status = FollowStatus.ACCEPTED }
        };
        _repoMock.Setup(r => r.FindFollowers(5)).ReturnsAsync(followers);

        var result = await _sut.GetFollowers(5);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetFollowing_ReturnsAcceptedFollowees()
    {
        var following = new List<Follow>
        {
            new Follow { FollowId = 3, FollowerId = 5, FolloweeId = 20, Status = FollowStatus.ACCEPTED }
        };
        _repoMock.Setup(r => r.FindFollowing(5)).ReturnsAsync(following);

        var result = await _sut.GetFollowing(5);

        Assert.That(result.Count, Is.EqualTo(1));
    }

    // ─────────────────────────────────────────────────────────
    //  GET FOLLOWER/FOLLOWING COUNT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetFollowerCount_ReturnsCorrectNumber()
    {
        _repoMock.Setup(r => r.CountFollowers(5)).ReturnsAsync(42);

        var count = await _sut.GetFollowerCount(5);

        Assert.That(count, Is.EqualTo(42));
    }

    [Test]
    public async Task GetFollowingCount_ReturnsCorrectNumber()
    {
        _repoMock.Setup(r => r.CountFollowing(5)).ReturnsAsync(18);

        var count = await _sut.GetFollowingCount(5);

        Assert.That(count, Is.EqualTo(18));
    }

    // ─────────────────────────────────────────────────────────
    //  GET FOLLOWING IDS TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetFollowingIds_ReturnsIdList()
    {
        var ids = new List<int> { 10, 20, 30 };
        _repoMock.Setup(r => r.GetAcceptedFolloweeIds(5)).ReturnsAsync(ids);

        var result = await _sut.GetFollowingIds(5);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result, Contains.Item(20));
    }

    // ─────────────────────────────────────────────────────────
    //  PENDING REQUESTS TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetPendingRequests_ReturnsPendingFollows()
    {
        var pending = new List<Follow>
        {
            new Follow { FollowId = 5, FollowerId = 50, FolloweeId = 5, Status = FollowStatus.PENDING },
            new Follow { FollowId = 6, FollowerId = 51, FolloweeId = 5, Status = FollowStatus.PENDING }
        };
        _repoMock.Setup(r => r.FindPendingRequests(5)).ReturnsAsync(pending);

        var result = await _sut.GetPendingRequests(5);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(f => f.Status == FollowStatus.PENDING), Is.True);
    }
}

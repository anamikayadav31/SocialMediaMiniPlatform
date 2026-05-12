using Moq;
using NUnit.Framework;

// ============================================================
//  LikeService Tests — SimpleLikeService (transaction-free wrapper)
// ============================================================

[TestFixture]
public class LikeServiceTests
{
    private Mock<ILikeRepository> _repoMock;
    private Mock<INotifService>   _notifMock;
    private SimpleLikeService     _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock  = new Mock<ILikeRepository>();
        _notifMock = new Mock<INotifService>();

        // SimpleLikeService — transaction-free (unit testing ke liye)
        _sut = new SimpleLikeService(_repoMock.Object, _notifMock.Object);
    }

    // ─────────────────────────────────────────────────────────
    //  HAS USER LIKED TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task HasUserLiked_WhenLikeExists_ReturnsTrue()
    {
        _repoMock.Setup(r => r.HasLiked(1, 10, TargetType.POST)).ReturnsAsync(true);

        var result = await _sut.HasUserLiked(1, 10, TargetType.POST);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasUserLiked_WhenNoLike_ReturnsFalse()
    {
        _repoMock.Setup(r => r.HasLiked(1, 10, TargetType.POST)).ReturnsAsync(false);

        var result = await _sut.HasUserLiked(1, 10, TargetType.POST);

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────
    //  GET LIKE COUNT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetLikeCount_ReturnsCorrectCount()
    {
        _repoMock.Setup(r => r.CountByTargetId(5, TargetType.POST)).ReturnsAsync(7);

        var count = await _sut.GetLikeCount(5, TargetType.POST);

        Assert.That(count, Is.EqualTo(7));
    }

    [Test]
    public async Task GetLikeCount_NoLikes_ReturnsZero()
    {
        _repoMock.Setup(r => r.CountByTargetId(99, TargetType.COMMENT)).ReturnsAsync(0);

        var count = await _sut.GetLikeCount(99, TargetType.COMMENT);

        Assert.That(count, Is.EqualTo(0));
    }

    // ─────────────────────────────────────────────────────────
    //  ADD LIKE TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task AddLike_NewLike_CallsRepoAdd()
    {
        _repoMock.Setup(r => r.AddLike(It.IsAny<Like>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.AddLike(1, 10, TargetType.POST);

        _repoMock.Verify(r => r.AddLike(It.Is<Like>(l =>
            l.UserId == 1 && l.TargetId == 10 && l.TargetType == TargetType.POST
        )), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  REMOVE LIKE TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task RemoveLike_ExistingLike_CallsRepoDelete()
    {
        var like = new Like { LikeId = 1, UserId = 1, TargetId = 10, TargetType = TargetType.POST };
        _repoMock.Setup(r => r.FindLike(1, 10, TargetType.POST)).ReturnsAsync(like);
        _repoMock.Setup(r => r.DeleteLike(like)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.RemoveLike(1, 10, TargetType.POST);

        _repoMock.Verify(r => r.DeleteLike(like), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  GET LIKES BY TARGET TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetLikesByTarget_PostLikes_ReturnsList()
    {
        var likes = new List<Like>
        {
            new Like { LikeId = 1, UserId = 1, TargetId = 5, TargetType = TargetType.POST },
            new Like { LikeId = 2, UserId = 2, TargetId = 5, TargetType = TargetType.POST }
        };
        _repoMock.Setup(r => r.FindByTarget(5, TargetType.POST)).ReturnsAsync(likes);

        var result = await _sut.GetLikesByTarget(5, TargetType.POST);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    // ─────────────────────────────────────────────────────────
    //  GET LIKES BY USER TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetLikesByUser_ReturnsUserLikes()
    {
        var likes = new List<Like>
        {
            new Like { LikeId = 3, UserId = 10, TargetId = 1, TargetType = TargetType.POST },
            new Like { LikeId = 4, UserId = 10, TargetId = 2, TargetType = TargetType.COMMENT }
        };
        _repoMock.Setup(r => r.FindByUser(10)).ReturnsAsync(likes);

        var result = await _sut.GetLikesByUser(10);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    // ─────────────────────────────────────────────────────────
    //  GET LIKERS FOR POST TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetLikersForPost_ReturnsUserIdList()
    {
        var userIds = new List<int> { 1, 2, 3 };
        _repoMock.Setup(r => r.GetLikerIdsByPost(7)).ReturnsAsync(userIds);

        var result = await _sut.GetLikersForPost(7);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result, Contains.Item(1));
    }

    // ─────────────────────────────────────────────────────────
    //  GET LIKED POSTS BY USER TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetLikedPostsByUser_ReturnsPostIdList()
    {
        var postIds = new List<int> { 10, 20, 30 };
        _repoMock.Setup(r => r.GetLikedPostIds(5)).ReturnsAsync(postIds);

        var result = await _sut.GetLikedPostsByUser(5);

        Assert.That(result.Count, Is.EqualTo(3));
    }
}

// ─────────────────────────────────────────────────────────────
//  SimpleLikeService — transaction-free wrapper (unit tests ke liye)
//  Real LikeServiceImpl mein DB transaction hai — integration test mein use karo
// ─────────────────────────────────────────────────────────────
public class SimpleLikeService : ILikeService
{
    private readonly ILikeRepository _repo;
    private readonly INotifService   _notif;

    public SimpleLikeService(ILikeRepository repo, INotifService notif)
    {
        _repo  = repo;
        _notif = notif;
    }

    public async Task<bool> HasUserLiked(int userId, int targetId, TargetType targetType)
        => await _repo.HasLiked(userId, targetId, targetType);

    public async Task<int> GetLikeCount(int targetId, TargetType targetType)
        => await _repo.CountByTargetId(targetId, targetType);

    public async Task AddLike(int userId, int targetId, TargetType targetType)
    {
        var like = new Like { UserId = userId, TargetId = targetId, TargetType = targetType };
        await _repo.AddLike(like);
        await _repo.SaveChanges();
    }

    public async Task RemoveLike(int userId, int targetId, TargetType targetType)
    {
        var like = await _repo.FindLike(userId, targetId, targetType);
        if (like != null)
        {
            await _repo.DeleteLike(like);
            await _repo.SaveChanges();
        }
    }

    public async Task<List<LikeDto>> GetLikesByTarget(int targetId, TargetType targetType)
    {
        var likes = await _repo.FindByTarget(targetId, targetType);
        return likes.Select(l => new LikeDto
        {
            LikeId     = l.LikeId,
            UserId     = l.UserId,
            TargetId   = l.TargetId,
            TargetType = l.TargetType
        }).ToList();
    }

    public async Task<List<LikeDto>> GetLikesByUser(int userId)
    {
        var likes = await _repo.FindByUser(userId);
        return likes.Select(l => new LikeDto
        {
            LikeId     = l.LikeId,
            UserId     = l.UserId,
            TargetId   = l.TargetId,
            TargetType = l.TargetType
        }).ToList();
    }

    public async Task<List<int>> GetLikersForPost(int postId)
        => await _repo.GetLikerIdsByPost(postId);

    public async Task<List<int>> GetLikedPostsByUser(int userId)
        => await _repo.GetLikedPostIds(userId);

    // ToggleLike ke liye real DB transaction chahiye — integration test mein karo
    public Task<ToggleLikeResultDto> ToggleLike(int userId, int targetId, TargetType targetType, int? ownerId = null)
        => throw new NotImplementedException("Use integration tests for ToggleLike");
}
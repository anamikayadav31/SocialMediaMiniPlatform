using Moq;
using NUnit.Framework;

// ============================================================
//  CommentService Tests — CommentServiceImpl ki methods test karo
// ============================================================

[TestFixture]
public class CommentServiceTests
{
    private Mock<ICommentRepository> _repoMock;
    private Mock<INotifService>      _notifMock;
    private Mock<IPostService>       _postSvcMock;
    private CommentServiceImpl       _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock    = new Mock<ICommentRepository>();
        _notifMock   = new Mock<INotifService>();
        _postSvcMock = new Mock<IPostService>();

        _sut = new CommentServiceImpl(
            _repoMock.Object,
            _notifMock.Object,
            _postSvcMock.Object
        );
    }

    // ─────────────────────────────────────────────────────────
    //  ADD COMMENT TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task AddComment_TopLevel_ReturnsDto()
    {
        // Arrange — top-level comment (ParentCommentId null)
        var dto = new AddCommentDto
        {
            PostId          = 1,
            UserId          = 2,
            Content         = "Nice post!",
            ParentCommentId = null,
            PostOwnerId     = 5
        };

        _repoMock.Setup(r => r.AddComment(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);
        _postSvcMock.Setup(p => p.IncrementCommentCount(1)).Returns(Task.CompletedTask);
        _notifMock.Setup(n => n.SendCommentNotif(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddComment(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.EqualTo("Nice post!"));
        Assert.That(result.PostId, Is.EqualTo(1));
        Assert.That(result.UserId, Is.EqualTo(2));
    }

    [Test]
    public async Task AddComment_Reply_IncrementsParentReplyCount()
    {
        // Arrange — reply to comment ID 10
        var parentComment = new Comment { CommentId = 10, PostId = 1, UserId = 1, Content = "Parent", ReplyCount = 0 };

        var dto = new AddCommentDto
        {
            PostId          = 1,
            UserId          = 3,
            Content         = "I agree!",
            ParentCommentId = 10
        };

        _repoMock.Setup(r => r.AddComment(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.FindByCommentId(10)).ReturnsAsync(parentComment);
        _postSvcMock.Setup(p => p.IncrementCommentCount(1)).Returns(Task.CompletedTask);
        _notifMock.Setup(n => n.SendCommentNotif(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddComment(dto);

        // Assert — parent ka ReplyCount badhna chahiye
        Assert.That(result, Is.Not.Null);
        Assert.That(parentComment.ReplyCount, Is.EqualTo(1));
    }

    // ─────────────────────────────────────────────────────────
    //  GET COMMENT BY ID TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetCommentById_ExistingComment_ReturnsDto()
    {
        var comment = new Comment { CommentId = 5, PostId = 1, UserId = 2, Content = "Hello" };
        _repoMock.Setup(r => r.FindByCommentId(5)).ReturnsAsync(comment);

        var result = await _sut.GetCommentById(5);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CommentId, Is.EqualTo(5));
    }

    [Test]
    public async Task GetCommentById_NotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.FindByCommentId(999)).ReturnsAsync((Comment?)null);

        var result = await _sut.GetCommentById(999);

        Assert.That(result, Is.Null);
    }

    // ─────────────────────────────────────────────────────────
    //  GET COMMENTS BY POST TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetCommentsByPost_ReturnsAllComments()
    {
        var comments = new List<Comment>
        {
            new Comment { CommentId = 1, PostId = 3, UserId = 1, Content = "C1" },
            new Comment { CommentId = 2, PostId = 3, UserId = 2, Content = "C2" },
            new Comment { CommentId = 3, PostId = 3, UserId = 3, Content = "C3" }
        };
        _repoMock.Setup(r => r.FindByPostId(3)).ReturnsAsync(comments);

        var result = await _sut.GetCommentsByPost(3);

        Assert.That(result.Count, Is.EqualTo(3));
    }

    // ─────────────────────────────────────────────────────────
    //  GET TOP LEVEL COMMENTS TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetTopLevelComments_OnlyReturnsNonReplies()
    {
        var topLevel = new List<Comment>
        {
            new Comment { CommentId = 1, PostId = 4, UserId = 1, Content = "Top1", ParentCommentId = null },
            new Comment { CommentId = 2, PostId = 4, UserId = 2, Content = "Top2", ParentCommentId = null }
        };
        _repoMock.Setup(r => r.FindTopLevelByPostId(4)).ReturnsAsync(topLevel);

        var result = await _sut.GetTopLevelComments(4);

        Assert.That(result.Count, Is.EqualTo(2));
        // Koi bhi ParentCommentId null nahi honi chahiye
        Assert.That(result.All(c => c.ParentCommentId == null), Is.True);
    }

    // ─────────────────────────────────────────────────────────
    //  GET REPLIES TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetReplies_ReturnsRepliesToComment()
    {
        var replies = new List<Comment>
        {
            new Comment { CommentId = 10, ParentCommentId = 5, PostId = 1, UserId = 2, Content = "Reply 1" },
            new Comment { CommentId = 11, ParentCommentId = 5, PostId = 1, UserId = 3, Content = "Reply 2" }
        };
        _repoMock.Setup(r => r.FindReplies(5)).ReturnsAsync(replies);

        var result = await _sut.GetReplies(5);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(r => r.ParentCommentId == 5), Is.True);
    }

    // ─────────────────────────────────────────────────────────
    //  EDIT COMMENT TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task EditComment_ActiveComment_UpdatesContent()
    {
        var comment = new Comment { CommentId = 7, PostId = 1, UserId = 2, Content = "Old text", IsDeleted = false };
        _repoMock.Setup(r => r.FindByCommentId(7)).ReturnsAsync(comment);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var result = await _sut.EditComment(7, "New text");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Content, Is.EqualTo("New text"));
        Assert.That(comment.IsEdited, Is.True);
    }

    [Test]
    public async Task EditComment_DeletedComment_ReturnsNull()
    {
        // Deleted comment edit nahi ho sakta
        var comment = new Comment { CommentId = 8, IsDeleted = true, Content = "This comment was deleted." };
        _repoMock.Setup(r => r.FindByCommentId(8)).ReturnsAsync(comment);

        var result = await _sut.EditComment(8, "Trying to edit");

        Assert.That(result, Is.Null);
    }

    // ─────────────────────────────────────────────────────────
    //  DELETE COMMENT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteComment_CallsSoftDeleteOnRepo()
    {
        _repoMock.Setup(r => r.DeleteCommentById(15)).Returns(Task.CompletedTask);

        await _sut.DeleteComment(15);

        _repoMock.Verify(r => r.DeleteCommentById(15), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  GET COMMENT COUNT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetCommentCount_ReturnsCorrectNumber()
    {
        _repoMock.Setup(r => r.CountByPostId(20)).ReturnsAsync(12);

        var count = await _sut.GetCommentCount(20);

        Assert.That(count, Is.EqualTo(12));
    }

    // ─────────────────────────────────────────────────────────
    //  INCREMENT LIKE COUNT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task IncrementLikeCount_CallsRepo()
    {
        _repoMock.Setup(r => r.IncrementLikeCount(5)).Returns(Task.CompletedTask);

        await _sut.IncrementLikeCount(5);

        _repoMock.Verify(r => r.IncrementLikeCount(5), Times.Once);
    }
}

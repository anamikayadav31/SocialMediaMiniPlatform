using Moq;
using NUnit.Framework;

// ============================================================
//  NotificationService Tests — NotifServiceImpl ki methods test karo
// ============================================================

[TestFixture]
public class NotifServiceTests
{
    private Mock<INotifRepository> _repoMock;
    private NotifServiceImpl       _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<INotifRepository>();
        _sut      = new NotifServiceImpl(_repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────
    //  SEND (Core) TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task Send_ValidNotification_SavesToRepo()
    {
        var notif = new Notification
        {
            RecipientId = 1,
            ActorId     = 2,
            Type        = NotifType.NEW_FOLLOWER,
            Message     = "Test message",
            CreatedAt   = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.AddNotification(It.IsAny<Notification>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.Send(notif);

        _repoMock.Verify(r => r.AddNotification(It.IsAny<Notification>()), Times.Once);
        _repoMock.Verify(r => r.SaveChanges(), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  SEND LIKE NOTIF TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task SendLikeNotif_PostLike_SavesCorrectNotifType()
    {
        Notification? saved = null;

        _repoMock.Setup(r => r.AddNotification(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.SendLikeNotif(recipientId: 5, actorId: 10, targetId: 20, targetType: TargetType.POST);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Type, Is.EqualTo(NotifType.LIKE_POST));
        Assert.That(saved.RecipientId, Is.EqualTo(5));
        Assert.That(saved.ActorId, Is.EqualTo(10));
        Assert.That(saved.TargetId, Is.EqualTo(20));
    }

    [Test]
    public async Task SendLikeNotif_CommentLike_SavesLikeCommentType()
    {
        Notification? saved = null;

        _repoMock.Setup(r => r.AddNotification(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.SendLikeNotif(recipientId: 1, actorId: 2, targetId: 3, targetType: TargetType.COMMENT);

        Assert.That(saved!.Type, Is.EqualTo(NotifType.LIKE_COMMENT));
    }

    // ─────────────────────────────────────────────────────────
    //  SEND COMMENT NOTIF TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task SendCommentNotif_SetsNewCommentType()
    {
        Notification? saved = null;

        _repoMock.Setup(r => r.AddNotification(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.SendCommentNotif(postAuthorId: 1, actorId: 2, postId: 10);

        Assert.That(saved!.Type, Is.EqualTo(NotifType.NEW_COMMENT));
        Assert.That(saved.RecipientId, Is.EqualTo(1));
        Assert.That(saved.TargetType, Is.EqualTo(TargetType.POST));
    }

    // ─────────────────────────────────────────────────────────
    //  SEND FOLLOW NOTIF TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task SendFollowNotif_SetsNewFollowerType()
    {
        Notification? saved = null;

        _repoMock.Setup(r => r.AddNotification(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.SendFollowNotif(targetId: 5, followerId: 10);

        Assert.That(saved!.Type, Is.EqualTo(NotifType.NEW_FOLLOWER));
        Assert.That(saved.RecipientId, Is.EqualTo(5));
        Assert.That(saved.ActorId, Is.EqualTo(10));
    }

    // ─────────────────────────────────────────────────────────
    //  SEND MENTION NOTIF TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task SendMentionNotif_SetsMentionType()
    {
        Notification? saved = null;

        _repoMock.Setup(r => r.AddNotification(It.IsAny<Notification>()))
            .Callback<Notification>(n => saved = n)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        await _sut.SendMentionNotif(mentionedId: 7, actorId: 8, postId: 15);

        Assert.That(saved!.Type, Is.EqualTo(NotifType.MENTION));
        Assert.That(saved.RecipientId, Is.EqualTo(7));
        Assert.That(saved.TargetId, Is.EqualTo(15));
    }

    // ─────────────────────────────────────────────────────────
    //  GET BY RECIPIENT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetByRecipient_ReturnsAllNotifications()
    {
        var notifs = new List<Notification>
        {
            new Notification { NotificationId = 1, RecipientId = 5, Type = NotifType.LIKE_POST },
            new Notification { NotificationId = 2, RecipientId = 5, Type = NotifType.NEW_FOLLOWER }
        };
        _repoMock.Setup(r => r.FindByRecipient(5)).ReturnsAsync(notifs);

        var result = await _sut.GetByRecipient(5);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    // ─────────────────────────────────────────────────────────
    //  GET UNREAD TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetUnread_ReturnsOnlyUnreadNotifications()
    {
        var unread = new List<Notification>
        {
            new Notification { NotificationId = 3, RecipientId = 5, IsRead = false }
        };
        _repoMock.Setup(r => r.FindUnread(5)).ReturnsAsync(unread);

        var result = await _sut.GetUnread(5);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.All(n => !n.IsRead), Is.True);
    }

    // ─────────────────────────────────────────────────────────
    //  GET UNREAD COUNT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetUnreadCount_ReturnsCorrectNumber()
    {
        _repoMock.Setup(r => r.CountUnread(5)).ReturnsAsync(3);

        var count = await _sut.GetUnreadCount(5);

        Assert.That(count, Is.EqualTo(3));
    }

    // ─────────────────────────────────────────────────────────
    //  MARK AS READ TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task MarkAsRead_CallsRepoMarkRead()
    {
        _repoMock.Setup(r => r.MarkRead(10)).Returns(Task.CompletedTask);

        await _sut.MarkAsRead(10);

        _repoMock.Verify(r => r.MarkRead(10), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  MARK ALL READ TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task MarkAllRead_CallsRepoMarkAllRead()
    {
        _repoMock.Setup(r => r.MarkAllRead(5)).Returns(Task.CompletedTask);

        await _sut.MarkAllRead(5);

        _repoMock.Verify(r => r.MarkAllRead(5), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  DELETE NOTIF TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task DeleteNotif_CallsRepoDelete()
    {
        _repoMock.Setup(r => r.DeleteById(7)).Returns(Task.CompletedTask);

        await _sut.DeleteNotif(7);

        _repoMock.Verify(r => r.DeleteById(7), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  SEND BULK TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task SendBulk_MultipleRecipients_SavesOneNotifPerRecipient()
    {
        int saveCount = 0;
        _repoMock.Setup(r => r.AddNotification(It.IsAny<Notification>()))
            .Callback(() => saveCount++)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var dto = new SendBulkNotifDto
        {
            RecipientIds = new List<int> { 1, 2, 3 },
            ActorId      = 10,
            Type         = NotifType.PLATFORM,
            Message      = "Platform announcement"
        };

        await _sut.SendBulk(dto);

        // Har recipient ke liye ek notification save hona chahiye
        Assert.That(saveCount, Is.EqualTo(3));
    }
}

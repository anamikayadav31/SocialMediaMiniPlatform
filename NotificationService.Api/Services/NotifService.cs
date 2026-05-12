public class NotifServiceImpl : INotifService
{
    private readonly INotifRepository _repo;

    public NotifServiceImpl(INotifRepository repo)
    {
        _repo = repo;
    }

    // ── Core Send ─────────────────────────────────────────────
    public async Task Send(Notification notif)
    {
        await _repo.AddNotification(notif);
        await _repo.SaveChanges();
    }

    // ── Typed helpers ─────────────────────────────────────────
    public async Task SendLikeNotif(int recipientId, int actorId, int targetId, TargetType targetType)
    {
        var type    = targetType == TargetType.POST ? NotifType.LIKE_POST : NotifType.LIKE_COMMENT;
        var message = targetType == TargetType.POST
            ? $"User {actorId} liked your post."
            : $"User {actorId} liked your comment.";

        await Send(new Notification
        {
            RecipientId = recipientId,
            ActorId     = actorId,
            Type        = type,
            Message     = message,
            TargetId    = targetId,
            TargetType  = targetType,
            CreatedAt   = DateTime.UtcNow
        });
    }

    public async Task SendCommentNotif(int postAuthorId, int actorId, int postId)
    {
        await Send(new Notification
        {
            RecipientId = postAuthorId,
            ActorId     = actorId,
            Type        = NotifType.NEW_COMMENT,
            Message     = $"User {actorId} commented on your post.",
            TargetId    = postId,
            TargetType  = TargetType.POST,
            CreatedAt   = DateTime.UtcNow
        });
    }

    public async Task SendFollowNotif(int targetId, int followerId)
    {
        await Send(new Notification
        {
            RecipientId = targetId,
            ActorId     = followerId,
            Type        = NotifType.NEW_FOLLOWER,
            Message     = $"User {followerId} started following you.",
            TargetId    = followerId,
            TargetType  = TargetType.USER,
            CreatedAt   = DateTime.UtcNow
        });
    }



    public async Task SendMentionNotif(int mentionedId, int actorId, int postId)
    {
        await Send(new Notification
        {
            RecipientId = mentionedId,
            ActorId     = actorId,
            Type        = NotifType.MENTION,
            Message     = $"User {actorId} mentioned you in a post.",
            TargetId    = postId,
            TargetType  = TargetType.POST,
            CreatedAt   = DateTime.UtcNow
        });
    }

    // ── Bulk send ─────────────────────────────────────────────
    public async Task SendBulk(SendBulkNotifDto dto)
    {
        var notifications = dto.RecipientIds.Select(recipientId => new Notification
        {
            RecipientId = recipientId,
            ActorId     = dto.ActorId,
            Type        = dto.Type,
            Message     = dto.Message,
            TargetId    = dto.TargetId,
            TargetType  = dto.TargetType,
            CreatedAt   = DateTime.UtcNow
        }).ToList();

        foreach (var notif in notifications)
            await _repo.AddNotification(notif);

        await _repo.SaveChanges();
    }

    // ── Queries ───────────────────────────────────────────────
    // FIX: use FindByRecipient alias so Moq setup on FindByRecipient works
    public async Task<List<NotificationDto>> GetByRecipient(int recipientId)
        => (await _repo.FindByRecipient(recipientId)).Select(ToDto).ToList();

    // FIX: use FindUnread alias
    public async Task<List<NotificationDto>> GetUnread(int recipientId)
        => (await _repo.FindUnread(recipientId)).Select(ToDto).ToList();

    // FIX: use CountUnread alias
    public async Task<int> GetUnreadCount(int recipientId)
        => await _repo.CountUnread(recipientId);

    // ── Actions ───────────────────────────────────────────────
    // FIX: use MarkRead alias so Moq verify on MarkRead works
    public async Task MarkAsRead(int notifId)
        => await _repo.MarkRead(notifId);

    public async Task MarkAllRead(int recipientId)
        => await _repo.MarkAllRead(recipientId);

    // FIX: use DeleteById alias so Moq verify on DeleteById works
    public async Task DeleteNotif(int notifId)
        => await _repo.DeleteById(notifId);

    // ── Mapper ────────────────────────────────────────────────
    private static NotificationDto ToDto(Notification n) => new()
    {
        NotificationId = n.NotificationId,
        RecipientId    = n.RecipientId,
        ActorId        = n.ActorId,
        Type           = n.Type,
        Message        = n.Message,
        TargetId       = n.TargetId,
        TargetType     = n.TargetType,
        IsRead         = n.IsRead,
        CreatedAt      = n.CreatedAt
    };
}
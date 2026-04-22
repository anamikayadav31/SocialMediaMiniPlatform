public interface INotifService
{
    Task                      Send(Notification notif);
    Task                      SendLikeNotif(int recipientId, int actorId, int targetId, TargetType targetType);
    Task                      SendCommentNotif(int postAuthorId, int actorId, int postId);
    Task                      SendFollowNotif(int targetId, int followerId);
    Task                      SendMentionNotif(int mentionedId, int actorId, int postId);
    Task                      SendBulk(SendBulkNotifDto dto);

    Task<List<NotificationDto>> GetByRecipient(int recipientId);
    Task<List<NotificationDto>> GetUnread(int recipientId);
    Task<int>                   GetUnreadCount(int recipientId);

    Task                        MarkAsRead(int notifId);
    Task                        MarkAllRead(int recipientId);
    Task                        DeleteNotif(int notifId);
}
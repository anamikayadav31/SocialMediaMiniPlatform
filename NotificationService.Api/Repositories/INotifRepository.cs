public interface INotifRepository
{
    Task<Notification?>       FindByNotifId(int notifId);
    Task<List<Notification>>  FindByRecipientId(int recipientId);
    Task<List<Notification>>  FindByRecipient(int recipientId);                        // alias for FindByRecipientId
    Task<List<Notification>>  FindUnreadByRecipientId(int recipientId);
    Task<List<Notification>>  FindUnread(int recipientId);                             // alias for FindUnreadByRecipientId
    Task<int>                 CountUnreadByRecipientId(int recipientId);
    Task<int>                 CountUnread(int recipientId);                            // alias for CountUnreadByRecipientId
    Task                      MarkAsRead(int notifId);
    Task                      MarkRead(int notifId);                                   // alias for MarkAsRead
    Task                      MarkAllRead(int recipientId);
    Task                      AddNotification(Notification notif);
    Task                      DeleteByNotifId(int notifId);
    Task                      DeleteById(int notifId);                                 // alias for DeleteByNotifId
    Task                      SaveChanges();
}
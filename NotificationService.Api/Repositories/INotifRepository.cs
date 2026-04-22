public interface INotifRepository
{
    Task<Notification?>       FindByNotifId(int notifId);
    Task<List<Notification>>  FindByRecipientId(int recipientId);
    Task<List<Notification>>  FindUnreadByRecipientId(int recipientId);
    Task<int>                 CountUnreadByRecipientId(int recipientId);
    Task                      MarkAsRead(int notifId);
    Task                      MarkAllRead(int recipientId);   // ExecuteUpdateAsync batch
    Task                      AddNotification(Notification notif);
    Task                      DeleteByNotifId(int notifId);
    Task                      SaveChanges();
}
using Microsoft.EntityFrameworkCore;

public class NotifRepository : INotifRepository
{
    private readonly NotifDbContext _db;

    public NotifRepository(NotifDbContext db) { _db = db; }

    public async Task<Notification?> FindByNotifId(int notifId)
        => await _db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notifId);

    public async Task<List<Notification>> FindByRecipientId(int recipientId)
        => await _db.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<List<Notification>> FindUnreadByRecipientId(int recipientId)
        => await _db.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<int> CountUnreadByRecipientId(int recipientId)
        => await _db.Notifications
            .CountAsync(n => n.RecipientId == recipientId && !n.IsRead);

    public async Task MarkAsRead(int notifId)
        => await _db.Notifications
            .Where(n => n.NotificationId == notifId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

    public async Task MarkAllRead(int recipientId)
        => await _db.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

    public async Task AddNotification(Notification notif)
        => await _db.Notifications.AddAsync(notif);

    public async Task DeleteByNotifId(int notifId)
        => await _db.Notifications
            .Where(n => n.NotificationId == notifId)
            .ExecuteDeleteAsync();

    public async Task SaveChanges()
        => await _db.SaveChangesAsync();
}
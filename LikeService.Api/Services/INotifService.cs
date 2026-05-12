/// <summary>
/// Notification dispatch contract.
/// Implement this to call your Notification microservice / message bus.
/// </summary>
public interface INotifService
{
    Task SendLikeNotif(int actorUserId, int targetId, TargetType targetType);
    Task SendLikeNotifToRecipient(int recipientId, int actorUserId, int targetId, TargetType targetType);
}
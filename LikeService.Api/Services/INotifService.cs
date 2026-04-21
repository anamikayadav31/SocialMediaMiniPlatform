/// <summary>
/// Notification dispatch contract.
/// Implement this to call your Notification microservice / message bus.
/// </summary>
public interface INotifService
{
    /// <summary>
    /// Fires a LIKE notification to the author of the liked target.
    /// Called only on toggle-to-liked, never on unlike.
    /// </summary>
    /// <param name="actorUserId">The user who performed the like.</param>
    /// <param name="targetId">Post or comment that was liked.</param>
    /// <param name="targetType">POST or COMMENT.</param>
    Task SendLikeNotif(int actorUserId, int targetId, TargetType targetType);
}
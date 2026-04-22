/// <summary>
/// Notification dispatch contract for follow events.
/// Replace stub with real HTTP/gRPC/message-bus call.
/// </summary>
public interface INotifService
{
    Task SendFollowNotif(int actorUserId, int targetUserId, FollowStatus status);
}

public class NotifServiceStub : INotifService
{
    private readonly ILogger<NotifServiceStub> _logger;

    public NotifServiceStub(ILogger<NotifServiceStub> logger)
    {
        _logger = logger;
    }

    public Task SendFollowNotif(int actorUserId, int targetUserId, FollowStatus status)
    {
        _logger.LogInformation(
            "[NotifStub] FOLLOW notification — actor={ActorUserId}, target={TargetUserId}, status={Status}",
            actorUserId, targetUserId, status);

        // TODO: Replace with real notification call

        return Task.CompletedTask;
    }
}
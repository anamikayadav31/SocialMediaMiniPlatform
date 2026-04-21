/// <summary>
/// Stub implementation of INotifService.
/// Replace this with your real HTTP call / message bus publish to the Notification service.
/// </summary>
public class NotifServiceStub : INotifService
{
    private readonly ILogger<NotifServiceStub> _logger;

    public NotifServiceStub(ILogger<NotifServiceStub> logger)
    {
        _logger = logger;
    }

    public Task SendLikeNotif(int actorUserId, int targetId, TargetType targetType)
    {
        _logger.LogInformation(
            "[NotifStub] LIKE notification — actor={ActorUserId}, target={TargetId}, type={TargetType}",
            actorUserId, targetId, targetType);

        // TODO: Replace with e.g.:
        // await _httpClient.PostAsJsonAsync("http://notif-service/api/notif/like", new { actorUserId, targetId, targetType });

        return Task.CompletedTask;
    }
}
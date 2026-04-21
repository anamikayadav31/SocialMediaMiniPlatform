/// <summary>
/// Notification dispatch contract.
/// Replace stub with real HTTP/gRPC/message-bus call to Notification service.
/// </summary>
public interface INotifService
{
    Task SendCommentNotif(int actorUserId, int postId, int commentId);
}

/// <summary>
/// Stub — logs only. Replace with real implementation.
/// </summary>
public class NotifServiceStub : INotifService
{
    private readonly ILogger<NotifServiceStub> _logger;

    public NotifServiceStub(ILogger<NotifServiceStub> logger)
    {
        _logger = logger;
    }

    public Task SendCommentNotif(int actorUserId, int postId, int commentId)
    {
        _logger.LogInformation(
            "[NotifStub] COMMENT notification — actor={ActorUserId}, post={PostId}, comment={CommentId}",
            actorUserId, postId, commentId);

        // TODO: Replace with e.g.:
        // await _httpClient.PostAsJsonAsync("http://notif-service/api/notif/comment",
        //     new { actorUserId, postId, commentId });

        return Task.CompletedTask;
    }
}
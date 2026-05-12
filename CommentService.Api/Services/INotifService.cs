using System.Text;
using System.Text.Json;

public interface INotifService
{
    Task SendCommentNotif(int actorUserId, int postId, int commentId, int postOwnerId);
    Task SendCommentNotif(int actorUserId, int postId, int commentId);
}

public class NotifServiceStub : INotifService
{
    private readonly ILogger<NotifServiceStub> _logger;
    private readonly HttpClient _http;
    private const string NOTIF_BASE = "http://localhost:5500";

    public NotifServiceStub(ILogger<NotifServiceStub> logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _http   = factory.CreateClient();
    }

    public Task SendCommentNotif(int actorUserId, int postId, int commentId)
    {
        _logger.LogInformation("[Notif] Comment by {actor} on post {post} — no ownerId", actorUserId, postId);
        return Task.CompletedTask;
    }

    public async Task SendCommentNotif(int actorUserId, int postId, int commentId, int postOwnerId)
    {
        if (actorUserId == postOwnerId) return;
        try
        {
            // NotifType: NEW_COMMENT=2, TargetType: POST=0
            var payload = new
            {
                recipientIds = new[] { postOwnerId },
                actorId      = actorUserId,
                type         = 2,   // NEW_COMMENT
                message      = $"User {actorUserId} commented on your post.",
                targetId     = postId,
                targetType   = 0    // POST
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp    = await _http.PostAsync($"{NOTIF_BASE}/api/notifications/sendBulk", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("[Notif] Comment sendBulk failed {status}: {body}", resp.StatusCode, body);
            }
            else
            {
                _logger.LogInformation("[Notif] Comment notif sent to postOwner={o}", postOwnerId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Notif] Comment notif failed: {msg}", ex.Message);
        }
    }
}

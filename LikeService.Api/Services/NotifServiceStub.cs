using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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

    public Task SendLikeNotif(int actorUserId, int targetId, TargetType targetType)
    {
        _logger.LogInformation("[Notif] Like by {actor} on {type} {target} — no ownerId provided", actorUserId, targetType, targetId);
        return Task.CompletedTask;
    }

    public async Task SendLikeNotifToRecipient(int recipientId, int actorUserId, int targetId, TargetType targetType)
    {
        if (recipientId == actorUserId) return;
        try
        {
            // NotifType aur TargetType dono numeric value se bhejo — enum string mismatch avoid
            // NotifType: LIKE_POST=0, LIKE_COMMENT=1
            // TargetType: POST=0, COMMENT=1
            int notifTypeInt  = targetType == TargetType.POST ? 0 : 1; // LIKE_POST or LIKE_COMMENT
            int targetTypeInt = targetType == TargetType.POST ? 0 : 1; // POST or COMMENT

            var message = targetType == TargetType.POST
                ? $"User {actorUserId} liked your post."
                : $"User {actorUserId} liked your comment.";

            var payload = new
            {
                recipientIds = new[] { recipientId },
                actorId      = actorUserId,
                type         = notifTypeInt,
                message,
                targetId,
                targetType   = targetTypeInt
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp    = await _http.PostAsync($"{NOTIF_BASE}/api/notifications/sendBulk", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("[Notif] sendBulk failed {status}: {body}", resp.StatusCode, body);
            }
            else
            {
                _logger.LogInformation("[Notif] Like notif sent to recipientId={r}", recipientId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Notif] Like notif send failed: {msg}", ex.Message);
        }
    }
}
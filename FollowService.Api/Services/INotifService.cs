using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public interface INotifService
{
    Task SendFollowNotif(int actorUserId, int targetUserId, FollowStatus status);
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

    public async Task SendFollowNotif(int actorUserId, int targetUserId, FollowStatus status)
    {
        if (actorUserId == targetUserId) return;
        // REJECTED — koi notif mat bhejo
        if (status == FollowStatus.REJECTED) return;
        try
        {
            // NotifType: NEW_FOLLOWER=4, FOLLOW_REQUEST=5
            int notifTypeInt = status == FollowStatus.PENDING ? 5 : 4;
            var message = status == FollowStatus.PENDING
                ? $"User {actorUserId} sent you a follow request."
                : $"User {actorUserId} started following you.";

            var payload = new
            {
                recipientIds = new[] { targetUserId },
                actorId      = actorUserId,
                type         = notifTypeInt,
                message,
                targetId     = actorUserId,
                targetType   = 2  // USER = 2
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp    = await _http.PostAsync($"{NOTIF_BASE}/api/notifications/sendBulk", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("[Notif] Follow sendBulk failed {status}: {body}", resp.StatusCode, body);
            }
            else
            {
                _logger.LogInformation("[Notif] Follow notif sent actor={a} target={t} type={s}", actorUserId, targetUserId, status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Notif] Follow notif failed: {msg}", ex.Message);
        }
    }
}
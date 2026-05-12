using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/notifications")]
public class NotifController : ControllerBase
{
    private readonly INotifService _svc;

    public NotifController(INotifService svc) { _svc = svc; }

    [HttpGet("byRecipient/{recipientId:int}")]
    [Authorize]
    public async Task<IActionResult> GetByRecipient(int recipientId)
        => Ok(await _svc.GetByRecipient(recipientId));

    [HttpGet("unread/{recipientId:int}")]
    [Authorize]
    public async Task<IActionResult> GetUnread(int recipientId)
        => Ok(await _svc.GetUnread(recipientId));

    [HttpGet("unreadCount/{recipientId:int}")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount(int recipientId)
    {
        int count = await _svc.GetUnreadCount(recipientId);
        return Ok(new { recipientId, unreadCount = count });
    }

    [HttpPut("markAsRead/{notifId:int}")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int notifId)
    {
        await _svc.MarkAsRead(notifId);
        return NoContent();
    }

    [HttpPut("markAllRead/{recipientId:int}")]
    [Authorize]
    public async Task<IActionResult> MarkAllRead(int recipientId)
    {
        await _svc.MarkAllRead(recipientId);
        return NoContent();
    }

    [HttpDelete("{notifId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int notifId)
    {
        await _svc.DeleteNotif(notifId);
        return NoContent();
    }

    [HttpPost("sendBulk")]
    // Internal service-to-service endpoint — no auth required
    public async Task<IActionResult> SendBulk([FromBody] SendBulkNotifDto dto)
    {
        await _svc.SendBulk(dto);
        return Ok(new { message = $"Sent to {dto.RecipientIds.Count} recipients." });
    }
}
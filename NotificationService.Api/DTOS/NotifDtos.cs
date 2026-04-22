using System.ComponentModel.DataAnnotations;

// ── Outgoing DTOs ─────────────────────────────────────────────

public class NotificationDto
{
    public int          NotificationId { get; set; }
    public int          RecipientId    { get; set; }
    public int          ActorId        { get; set; }
    public NotifType    Type           { get; set; }
    public string       Message        { get; set; } = string.Empty;
    public int?         TargetId       { get; set; }
    public TargetType?  TargetType     { get; set; }
    public bool         IsRead         { get; set; }
    public DateTime     CreatedAt      { get; set; }
}

// ── Incoming DTOs ─────────────────────────────────────────────

public class SendBulkNotifDto
{
    [Required]
    public List<int> RecipientIds { get; set; } = new();

    [Required]
    public int ActorId { get; set; }

    [Required]
    public NotifType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public int?        TargetId   { get; set; }
    public TargetType? TargetType { get; set; }
}
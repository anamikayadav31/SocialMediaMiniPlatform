using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public enum NotifType
{
    LIKE_POST,
    LIKE_COMMENT,
    NEW_COMMENT,
    NEW_REPLY,
    NEW_FOLLOWER,
    FOLLOW_REQUEST,
    FOLLOW_ACCEPTED,
    MENTION,
    PLATFORM
}

public enum TargetType { POST, COMMENT, USER }

[Index(nameof(RecipientId), nameof(IsRead))]
public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    public int RecipientId { get; set; }    // user receiving the notification

    [Required]
    public int ActorId { get; set; }        // user who triggered it

    [Required]
    public NotifType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public int?      TargetId   { get; set; }   // postId, commentId, userId
    public TargetType? TargetType { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;

// ── Incoming DTOs ─────────────────────────────────────────────

public class AddCommentDto
{
    [Required]
    public int PostId { get; set; }

    [Required]
    public int UserId { get; set; }

    public int? ParentCommentId { get; set; }

    // Post owner ka userId — notification ke liye
    public int? PostOwnerId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class EditCommentDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

// ── Outgoing DTOs ─────────────────────────────────────────────

public class CommentDto
{
    public int       CommentId       { get; set; }
    public int       PostId          { get; set; }
    public int       UserId          { get; set; }
    public int?      ParentCommentId { get; set; }
    public string    Content         { get; set; } = string.Empty;
    public int       LikeCount       { get; set; }
    public int       ReplyCount      { get; set; }
    public bool      IsDeleted       { get; set; }
    public bool      IsEdited        { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public DateTime? EditedAt        { get; set; }
}
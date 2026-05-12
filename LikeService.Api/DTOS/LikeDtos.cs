using System.ComponentModel.DataAnnotations;

// ── Incoming DTOs ─────────────────────────────────────────────

public class ToggleLikeDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int TargetId { get; set; }

    [Required]
    public TargetType TargetType { get; set; }

    // Post/Comment ke owner ka userId — notification ke liye
    public int? OwnerId { get; set; }
}

// ── Outgoing DTOs ─────────────────────────────────────────────

public class LikeDto
{
    public int        LikeId     { get; set; }
    public int        UserId     { get; set; }
    public int        TargetId   { get; set; }
    public TargetType TargetType { get; set; }
    public DateTime   CreatedAt  { get; set; }
}

public class ToggleLikeResultDto
{
    public bool   Liked      { get; set; }  // true = liked, false = unliked
    public int    LikeCount  { get; set; }
}
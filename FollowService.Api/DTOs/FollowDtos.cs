using System.ComponentModel.DataAnnotations;

// ── Incoming DTOs ─────────────────────────────────────────────

public class FollowRequestDto
{
    [Required]
    public int FollowerId { get; set; }

    [Required]
    public int FolloweeId { get; set; }
}

// ── Outgoing DTOs ─────────────────────────────────────────────

public class FollowDto
{
    public int          FollowId   { get; set; }
    public int          FollowerId { get; set; }
    public int          FolloweeId { get; set; }
    public FollowStatus Status     { get; set; }
    public DateTime     CreatedAt  { get; set; }
}

public class FollowCountDto
{
    public int FollowerCount  { get; set; }
    public int FollowingCount { get; set; }
}
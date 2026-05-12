// ── Outgoing DTOs ─────────────────────────────────────────────

public class FeedItemDto
{
    public int      FeedItemId { get; set; }
    public int      UserId     { get; set; }
    public int      PostId     { get; set; }
    public int      AuthorId   { get; set; }
    public DateTime CreatedAt  { get; set; }
}

public class TrendingHashtagDto
{
    public string Hashtag { get; set; } = string.Empty;
    public int    Count   { get; set; }
}

public class SuggestedUserDto
{
    public int UserId            { get; set; }
    public int MutualFollowers   { get; set; }
}

// ── Incoming DTOs ─────────────────────────────────────────────

public class FanoutDto
{
    public int PostId   { get; set; }
    public int AuthorId { get; set; }
    public List<int> FollowerIds { get; set; } = new();
}
using System.ComponentModel.DataAnnotations;

// ── Incoming DTOs ────────────────────────────────────────────

public class CreatePostDto
{
    [Required]
    public int UserId { get; set; }

    [Required][MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public string?    MediaUrl   { get; set; }
    public MediaType  MediaType  { get; set; } = MediaType.NONE;
    public Visibility Visibility { get; set; } = Visibility.PUBLIC;

    [MaxLength(500)]
    public string Hashtags { get; set; } = string.Empty;

    // If sharing another post
    public int? OriginalPostId { get; set; }
}

public class UpdatePostDto
{
    [MaxLength(2000)]
    public string? Content { get; set; }

    public string?    MediaUrl   { get; set; }
    public Visibility? Visibility { get; set; }

    [MaxLength(500)]
    public string? Hashtags { get; set; }
}

// ── Outgoing DTOs ────────────────────────────────────────────

public class PostDto
{
    public int       PostId         { get; set; }
    public int       UserId         { get; set; }
    public string    Content        { get; set; } = string.Empty;
    public string?   MediaUrl       { get; set; }
    public MediaType  MediaType     { get; set; }
    public Visibility Visibility    { get; set; }
    public string    Hashtags       { get; set; } = string.Empty;
    public int       LikeCount      { get; set; }
    public int       CommentCount   { get; set; }
    public int       ShareCount     { get; set; }
    public bool      IsDeleted      { get; set; }
    public bool      IsEdited       { get; set; }
    public int?      OriginalPostId { get; set; }
    public DateTime  CreatedAt      { get; set; }
    public DateTime? UpdatedAt      { get; set; }
}

public class IncrementCountDto
{
    [Required]
    public string Field { get; set; } = string.Empty; // LikeCount / CommentCount / ShareCount

    [Required]
    public int Delta { get; set; } // +1 or -1
}
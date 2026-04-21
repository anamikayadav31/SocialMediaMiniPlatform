using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public enum MediaType  { NONE, IMAGE, VIDEO, GIF }
public enum Visibility { PUBLIC, FOLLOWERS, PRIVATE }

[Index(nameof(UserId))]
[Index(nameof(CreatedAt))]
public class Post
{
    [Key]
    public int PostId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required][MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public string? MediaUrl  { get; set; }
    public MediaType  MediaType  { get; set; } = MediaType.NONE;
    public Visibility Visibility { get; set; } = Visibility.PUBLIC;

    // Comma-separated e.g. "#travel,#food,#london"
    [MaxLength(500)]
    public string Hashtags { get; set; } = string.Empty;

    // Denormalised counters — updated via ExecuteUpdateAsync
    public int LikeCount    { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ShareCount   { get; set; } = 0;

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    public bool IsEdited { get; set; } = false;

    // Share/repost reference
    public int? OriginalPostId { get; set; }

    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
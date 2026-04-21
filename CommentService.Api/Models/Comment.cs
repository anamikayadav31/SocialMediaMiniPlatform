using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(PostId), nameof(ParentCommentId))]
public class Comment
{
    [Key]
    public int CommentId { get; set; }

    [Required]
    public int PostId { get; set; }

    [Required]
    public int UserId { get; set; }

    public int? ParentCommentId { get; set; }  // null = top-level, int = reply

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public int LikeCount { get; set; } = 0;

    public int ReplyCount { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;

    public bool IsEdited { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EditedAt { get; set; }

    // Self-referential navigation
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
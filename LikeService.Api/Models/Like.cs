using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public enum TargetType { POST, COMMENT }

[Index(nameof(UserId), nameof(TargetId), nameof(TargetType), IsUnique = true)]
public class Like
{
    [Key]
    public int LikeId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int TargetId { get; set; }          // postId or commentId

    [Required]
    public TargetType TargetType { get; set; } // POST or COMMENT

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
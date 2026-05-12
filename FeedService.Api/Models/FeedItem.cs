using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(UserId), nameof(CreatedAt))]
public class FeedItem
{
    [Key]
    public int FeedItemId { get; set; }

    [Required]
    public int UserId  { get; set; }   // feed owner

    [Required]
    public int PostId  { get; set; }   // post in the feed

    [Required]
    public int AuthorId { get; set; }  // post author

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
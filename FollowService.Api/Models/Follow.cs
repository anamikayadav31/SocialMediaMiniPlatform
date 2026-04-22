using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public enum FollowStatus { PENDING, ACCEPTED, REJECTED }

[Index(nameof(FollowerId), nameof(FolloweeId), IsUnique = true)]
[Index(nameof(FolloweeId))]
public class Follow
{
    [Key]
    public int FollowId { get; set; }

    [Required]
    public int FollowerId { get; set; }   // user who sent the request

    [Required]
    public int FolloweeId { get; set; }   // user being followed

    [Required]
    public FollowStatus Status { get; set; } = FollowStatus.PENDING;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
using Microsoft.EntityFrameworkCore;

public class FollowDbContext : DbContext
{
    public FollowDbContext(DbContextOptions<FollowDbContext> options) : base(options) { }

    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Follow>(entity =>
        {
            // Composite unique index: one follow record per pair
            entity.HasIndex(f => new { f.FollowerId, f.FolloweeId })
                  .IsUnique();

            // Index on FolloweeId for follower list queries
            entity.HasIndex(f => f.FolloweeId);

            // Store enum as string
            entity.Property(f => f.Status)
                  .HasConversion<string>();
        });
    }
}

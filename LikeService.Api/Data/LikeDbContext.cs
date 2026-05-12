using Microsoft.EntityFrameworkCore;

public class LikeDbContext : DbContext
{
    public LikeDbContext(DbContextOptions<LikeDbContext> options) : base(options) { }

    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite unique index: one like per user per target
        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.TargetId, l.TargetType })
            .IsUnique();
    }
}
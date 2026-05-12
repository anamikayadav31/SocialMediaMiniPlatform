using Microsoft.EntityFrameworkCore;

public class FeedDbContext : DbContext
{
    // Parameterless constructor required for Moq mocking in unit tests
    public FeedDbContext() : base() { }

    public FeedDbContext(DbContextOptions<FeedDbContext> options) : base(options) { }

    public DbSet<FeedItem> FeedItems => Set<FeedItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeedItem>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.CreatedAt });
            entity.HasIndex(f => new { f.UserId, f.PostId }).IsUnique();
        });
    }
}
using Microsoft.EntityFrameworkCore;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

    public DbSet<Post> Posts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>()
            .Property(p => p.MediaType)
            .HasConversion<string>();

        modelBuilder.Entity<Post>()
            .Property(p => p.Visibility)
            .HasConversion<string>();
    }
}
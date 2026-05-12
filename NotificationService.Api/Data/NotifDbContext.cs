using Microsoft.EntityFrameworkCore;

public class NotifDbContext : DbContext
{
    public NotifDbContext(DbContextOptions<NotifDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(n => new { n.RecipientId, n.IsRead });

            entity.Property(n => n.Type)
                  .HasConversion<string>();

            entity.Property(n => n.TargetType)
                  .HasConversion<string>();
        });
    }
}
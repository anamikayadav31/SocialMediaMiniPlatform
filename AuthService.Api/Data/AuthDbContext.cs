using Microsoft.EntityFrameworkCore;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(new User
        {
            UserId       = 1,
            UserName     = "admin",
            FullName     = "Platform Admin",
            Email        = "admin@connectsphere.com",
            PasswordHash = "AQAAAAIAAYagAAAAEK+hWpmMBkJwuBRMhBwDVNKi8GQqgQvzmL9UoHr4HWtP8xEHpZ6pNpKiXQDLpGPW9w==",
            Bio          = string.Empty,
            Role         = "Admin",
            IsPrivate    = false,
            IsActive     = true,
            CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
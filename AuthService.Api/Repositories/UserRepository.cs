using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;

    public UserRepository(AuthDbContext db) { _db = db; }

    public async Task<User?> FindByUserId(int userId)
        => await _db.Users.FindAsync(userId);

    // Alias for FindByUserId
    public Task<User?> FindById(int userId)
        => FindByUserId(userId);

    public async Task<User?> FindByUserName(string userName)
        => await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName.ToLower());

    public async Task<User?> FindByEmail(string email)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<bool> ExistsByUserName(string userName)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName.ToLower());
        return user != null;
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
        return user != null;
    }

    public async Task<List<User>> SearchUsers(string query)
    {
        string pattern = $"%{query}%";
        return await _db.Users
            .Where(u => u.IsActive &&
                (EF.Functions.Like(u.UserName, pattern) ||
                 EF.Functions.Like(u.FullName,  pattern)))
            .Take(20)
            .ToListAsync();
    }

    // Alias for SearchUsers
    public Task<List<User>> SearchByQuery(string query)
        => SearchUsers(query);

    public async Task<List<User>> FindAllActive()
        => await _db.Users
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

    public async Task AddUser(User user) => await _db.Users.AddAsync(user);

    public void UpdateUser(User user) => _db.Users.Update(user);

    public void RemoveUser(User user) => _db.Users.Remove(user);

    public async Task UpdateCounters(int userId, string field, int delta)
    {
        if (field == "FollowerCount")
            await _db.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(u => u.FollowerCount, u => u.FollowerCount + delta));

        else if (field == "FollowingCount")
            await _db.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(u => u.FollowingCount, u => u.FollowingCount + delta));

        else if (field == "PostCount")
            await _db.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(u => u.PostCount, u => u.PostCount + delta));
    }

    public async Task SaveChanges() => await _db.SaveChangesAsync();
}
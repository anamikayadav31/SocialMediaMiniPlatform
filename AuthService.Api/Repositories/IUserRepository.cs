public interface IUserRepository
{
    Task<User?>      FindByUserId(int userId);
    Task<User?>      FindByUserName(string userName);
    Task<User?>      FindByEmail(string email);
    Task<bool>       ExistsByUserName(string userName);
    Task<bool>       ExistsByEmail(string email);
    Task<List<User>> SearchUsers(string query);
    Task<List<User>> FindAllActive();
    Task             AddUser(User user);
    void             UpdateUser(User user);
    void             RemoveUser(User user);
    Task             UpdateCounters(int userId, string field, int delta);
    Task             SaveChanges();
}
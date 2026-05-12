public interface IUserService
{
    Task<AuthResponseDto?>     Register(RegisterDto dto);
    Task<AuthResponseDto?>     Login(LoginDto dto);
    Task                       Logout(int userId);
    Task<bool>                 ValidateToken(string token);
    Task<string?>              RefreshToken(string token);
    Task<UserProfileDto?>      GetUserById(int userId);
    Task<UserProfileDto?>      GetUserByUserName(string userName);
    Task<bool>                 UpdateProfile(int userId, UpdateProfileDto dto);
    Task<bool>                 ChangePassword(int userId, ChangePasswordDto dto);
    Task<List<UserProfileDto>> SearchUsers(string query);
    Task<bool>                 TogglePrivacy(int userId);
    Task<bool>                 DeactivateAccount(int userId);
    Task<List<UserProfileDto>> GetSuggestedUsers(int userId);
    Task<List<UserProfileDto>> GetAllUsers();
    Task<bool>                 SuspendUser(int userId);
    Task<bool>                 ActivateUser(int userId);
    Task<bool>                 AdminDeleteUser(int userId);
    Task                       UpdateCounters(int userId, string field, int delta);
    Task<AuthResponseDto?>     GoogleLogin(string email, string? name, string? picture);
}
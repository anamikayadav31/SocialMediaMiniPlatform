using NUnit.Framework;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

// ============================================================
//  AuthService Tests — UserService ki saari methods test karo
// ============================================================

[TestFixture]
public class UserServiceTests
{
    // ── Mocks & system under test ─────────────────────────────
    private Mock<IUserRepository>  _repoMock;
    private IConfiguration         _config;
    private UserService            _sut;            // System Under Test
    private PasswordHasher<User>   _hasher;

    // ── Setup: har test se pehle chalta hai ───────────────────
    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IUserRepository>();
        _hasher   = new PasswordHasher<User>();

        // JWT config in-memory — real config ki zaroorat nahi
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"]      = "SuperSecretTestKey_AtLeast32Chars!!",
            ["Jwt:Issuer"]   = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _sut = new UserService(_repoMock.Object, _config);
    }

    // ─────────────────────────────────────────────────────────
    //  REGISTER TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task Register_ValidDto_ReturnsAuthResponse()
    {
        // Arrange — username aur email dono available hain
        var dto = new RegisterDto
        {
            UserName = "testuser",
            FullName = "Test User",
            Email    = "test@example.com",
            Password = "Password@123"
        };

        _repoMock.Setup(r => r.ExistsByUserName(dto.UserName)).ReturnsAsync(false);
        _repoMock.Setup(r => r.ExistsByEmail(dto.Email)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddUser(It.IsAny<User>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Register(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserName, Is.EqualTo("testuser"));
        Assert.That(result.Token, Is.Not.Empty);
    }

    [Test]
    public async Task Register_DuplicateUserName_ReturnsNull()
    {
        // Arrange — username already exists
        var dto = new RegisterDto
        {
            UserName = "existing",
            FullName = "Existing User",
            Email    = "new@example.com",
            Password = "Password@123"
        };

        _repoMock.Setup(r => r.ExistsByUserName(dto.UserName)).ReturnsAsync(true);

        // Act
        var result = await _sut.Register(dto);

        // Assert — duplicate hone par null return hona chahiye
        Assert.That(result, Is.Null);
        _repoMock.Verify(r => r.AddUser(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task Register_DuplicateEmail_ReturnsNull()
    {
        var dto = new RegisterDto
        {
            UserName = "newuser",
            FullName = "New User",
            Email    = "duplicate@example.com",
            Password = "Password@123"
        };

        _repoMock.Setup(r => r.ExistsByUserName(dto.UserName)).ReturnsAsync(false);
        _repoMock.Setup(r => r.ExistsByEmail(dto.Email)).ReturnsAsync(true);

        var result = await _sut.Register(dto);

        Assert.That(result, Is.Null);
    }

    // ─────────────────────────────────────────────────────────
    //  LOGIN TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange — ek real hashed password wala user banao
        var user = new User { UserId = 1, UserName = "alice", Email = "alice@example.com", IsActive = true, Role = "User" };
        user.PasswordHash = _hasher.HashPassword(user, "Password@123");

        var dto = new LoginDto { Email = "alice@example.com", Password = "Password@123" };

        _repoMock.Setup(r => r.FindByEmail("alice@example.com")).ReturnsAsync(user);

        var result = await _sut.Login(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserId, Is.EqualTo(1));
        Assert.That(result.Token, Is.Not.Empty);
    }

    [Test]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var user = new User { UserId = 2, Email = "bob@example.com", IsActive = true };
        user.PasswordHash = _hasher.HashPassword(user, "CorrectPassword");

        var dto = new LoginDto { Email = "bob@example.com", Password = "WrongPassword" };

        _repoMock.Setup(r => r.FindByEmail("bob@example.com")).ReturnsAsync(user);

        var result = await _sut.Login(dto);

        // Galat password par null aana chahiye
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Login_InactiveUser_ReturnsNull()
    {
        // Deactivated account — login nahi hona chahiye
        var user = new User { UserId = 3, Email = "inactive@example.com", IsActive = false };
        user.PasswordHash = _hasher.HashPassword(user, "Password@123");

        var dto = new LoginDto { Email = "inactive@example.com", Password = "Password@123" };

        _repoMock.Setup(r => r.FindByEmail("inactive@example.com")).ReturnsAsync(user);

        var result = await _sut.Login(dto);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Login_UserNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.FindByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);

        var dto = new LoginDto { Email = "ghost@example.com", Password = "anything" };

        var result = await _sut.Login(dto);

        Assert.That(result, Is.Null);
    }

    // ─────────────────────────────────────────────────────────
    //  GET USER TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetUserById_ExistingUser_ReturnsProfile()
    {
        var user = new User { UserId = 5, UserName = "charlie", FullName = "Charlie Brown", Email = "c@example.com" };
        _repoMock.Setup(r => r.FindById(5)).ReturnsAsync(user);

        var result = await _sut.GetUserById(5);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserName, Is.EqualTo("charlie"));
    }

    [Test]
    public async Task GetUserById_NonExistingUser_ReturnsNull()
    {
        _repoMock.Setup(r => r.FindById(999)).ReturnsAsync((User?)null);

        var result = await _sut.GetUserById(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserByUserName_Found_ReturnsProfile()
    {
        var user = new User { UserId = 6, UserName = "dave", FullName = "Dave", Email = "dave@example.com" };
        _repoMock.Setup(r => r.FindByUserName("dave")).ReturnsAsync(user);

        var result = await _sut.GetUserByUserName("dave");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserId, Is.EqualTo(6));
    }

    // ─────────────────────────────────────────────────────────
    //  UPDATE PROFILE TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateProfile_ValidUser_ReturnsTrue()
    {
        var user = new User { UserId = 7, UserName = "eve", FullName = "Eve", Email = "eve@example.com" };
        _repoMock.Setup(r => r.FindById(7)).ReturnsAsync(user);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var dto = new UpdateProfileDto { FullName = "Eve Updated", Bio = "Hello World" };

        var result = await _sut.UpdateProfile(7, dto);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UpdateProfile_UserNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.FindById(999)).ReturnsAsync((User?)null);

        var result = await _sut.UpdateProfile(999, new UpdateProfileDto { FullName = "X" });

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────
    //  TOGGLE PRIVACY TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task TogglePrivacy_PublicUser_BecomesPrivate()
    {
        var user = new User { UserId = 8, IsPrivate = false };
        _repoMock.Setup(r => r.FindById(8)).ReturnsAsync(user);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var result = await _sut.TogglePrivacy(8);

        Assert.That(result, Is.True);
        Assert.That(user.IsPrivate, Is.True);
    }

    // ─────────────────────────────────────────────────────────
    //  DEACTIVATE ACCOUNT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task DeactivateAccount_ActiveUser_DeactivatesSuccessfully()
    {
        var user = new User { UserId = 9, IsActive = true };
        _repoMock.Setup(r => r.FindById(9)).ReturnsAsync(user);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var result = await _sut.DeactivateAccount(9);

        Assert.That(result, Is.True);
        Assert.That(user.IsActive, Is.False);
    }

    // ─────────────────────────────────────────────────────────
    //  SEARCH USERS TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task SearchUsers_Query_ReturnsMatchingUsers()
    {
        var users = new List<User>
        {
            new User { UserId = 10, UserName = "frank", FullName = "Frank Sinatra", Email = "frank@example.com" },
            new User { UserId = 11, UserName = "france", FullName = "France Gall",  Email = "france@example.com" }
        };
        _repoMock.Setup(r => r.SearchByQuery("frank")).ReturnsAsync(users);

        var result = await _sut.SearchUsers("frank");

        Assert.That(result.Count, Is.EqualTo(2));
    }

    // ─────────────────────────────────────────────────────────
    //  LOGOUT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task Logout_AnyUserId_CompletesWithoutError()
    {
        // Logout sirf token discard karta hai — koi DB call nahi
        Assert.DoesNotThrowAsync(() => _sut.Logout(1));
        await Task.CompletedTask;
    }
}

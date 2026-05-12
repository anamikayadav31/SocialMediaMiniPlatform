using Moq;
using NUnit.Framework;

// ============================================================
//  PostService Tests — PostServiceImpl ki methods test karo
// ============================================================

[TestFixture]
public class PostServiceTests
{
    private Mock<IPostRepository> _repoMock;
    private PostServiceImpl       _sut;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IPostRepository>();
        _sut      = new PostServiceImpl(_repoMock.Object);
    }

    // ─────────────────────────────────────────────────────────
    //  CREATE POST TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreatePost_ValidDto_ReturnsPostDto()
    {
        // Arrange
        var dto = new CreatePostDto
        {
            UserId     = 1,
            Content    = "Hello World! #test",
            Hashtags   = "#test",
            Visibility = Visibility.PUBLIC,
            MediaType  = MediaType.NONE
        };

        _repoMock.Setup(r => r.AddPost(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreatePost(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Content, Is.EqualTo("Hello World! #test"));
        Assert.That(result.UserId, Is.EqualTo(1));
        _repoMock.Verify(r => r.AddPost(It.IsAny<Post>()), Times.Once);
    }

    [Test]
    public async Task CreatePost_ContentGetsTrimed_WhitespaceRemoved()
    {
        var dto = new CreatePostDto
        {
            UserId   = 2,
            Content  = "  spaces around  ",
            Hashtags = "",
            Visibility = Visibility.PUBLIC,
            MediaType  = MediaType.NONE
        };

        _repoMock.Setup(r => r.AddPost(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var result = await _sut.CreatePost(dto);

        // Content trim hona chahiye
        Assert.That(result!.Content, Is.EqualTo("spaces around"));
    }

    // ─────────────────────────────────────────────────────────
    //  GET POST BY ID
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetPostById_ExistingPost_ReturnsDto()
    {
        var post = new Post { PostId = 10, UserId = 1, Content = "Test post", Visibility = Visibility.PUBLIC };
        _repoMock.Setup(r => r.FindById(10)).ReturnsAsync(post);

        var result = await _sut.GetPostById(10);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.PostId, Is.EqualTo(10));
    }

    [Test]
    public async Task GetPostById_NotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.FindById(999)).ReturnsAsync((Post?)null);

        var result = await _sut.GetPostById(999);

        Assert.That(result, Is.Null);
    }

    // ─────────────────────────────────────────────────────────
    //  GET POSTS BY USER
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetPostsByUser_ReturnsAllUserPosts()
    {
        var posts = new List<Post>
        {
            new Post { PostId = 1, UserId = 5, Content = "Post 1", Visibility = Visibility.PUBLIC },
            new Post { PostId = 2, UserId = 5, Content = "Post 2", Visibility = Visibility.PUBLIC }
        };
        _repoMock.Setup(r => r.FindByUserId(5)).ReturnsAsync(posts);

        var result = await _sut.GetPostsByUser(5);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetPostsByUser_NoPostsFound_ReturnsEmptyList()
    {
        _repoMock.Setup(r => r.FindByUserId(99)).ReturnsAsync(new List<Post>());

        var result = await _sut.GetPostsByUser(99);

        Assert.That(result, Is.Empty);
    }

    // ─────────────────────────────────────────────────────────
    //  UPDATE POST TESTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task UpdatePost_ExistingPost_UpdatesAndReturnsDto()
    {
        var post = new Post { PostId = 3, UserId = 1, Content = "Old content", Visibility = Visibility.PUBLIC };
        _repoMock.Setup(r => r.FindById(3)).ReturnsAsync(post);
        _repoMock.Setup(r => r.UpdatePost(It.IsAny<Post>()));
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var dto = new UpdatePostDto { Content = "Updated content" };

        var result = await _sut.UpdatePost(3, dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Content, Is.EqualTo("Updated content"));
        Assert.That(post.IsEdited, Is.True);
    }

    [Test]
    public async Task UpdatePost_PostNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.FindById(999)).ReturnsAsync((Post?)null);

        var result = await _sut.UpdatePost(999, new UpdatePostDto { Content = "X" });

        Assert.That(result, Is.Null);
    }

    // ─────────────────────────────────────────────────────────
    //  DELETE POST TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task DeletePost_ExistingPost_ReturnsTrue()
    {
        var post = new Post { PostId = 4, UserId = 1, Content = "To be deleted" };
        _repoMock.Setup(r => r.FindById(4)).ReturnsAsync(post);
        _repoMock.Setup(r => r.SoftDelete(4)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

        var result = await _sut.DeletePost(4);

        Assert.That(result, Is.True);
        _repoMock.Verify(r => r.SoftDelete(4), Times.Once);
    }

    [Test]
    public async Task DeletePost_NotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.FindById(999)).ReturnsAsync((Post?)null);

        var result = await _sut.DeletePost(999);

        Assert.That(result, Is.False);
    }

    // ─────────────────────────────────────────────────────────
    //  GET PUBLIC POSTS
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetPublicPosts_ReturnsPaginatedList()
    {
        var posts = Enumerable.Range(1, 5)
            .Select(i => new Post { PostId = i, UserId = 1, Content = $"Post {i}", Visibility = Visibility.PUBLIC })
            .ToList();

        _repoMock.Setup(r => r.FindPublic(1, 5)).ReturnsAsync(posts);

        var result = await _sut.GetPublicPosts(1, 5);

        Assert.That(result.Count, Is.EqualTo(5));
    }

    // ─────────────────────────────────────────────────────────
    //  INCREMENT COUNT TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task IncrementCount_CallsRepository()
    {
        _repoMock.Setup(r => r.IncrementField(5, "LikeCount", 1)).Returns(Task.CompletedTask);

        await _sut.IncrementCount(5, "LikeCount", 1);

        _repoMock.Verify(r => r.IncrementField(5, "LikeCount", 1), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    //  SEARCH POSTS TEST
    // ─────────────────────────────────────────────────────────

    [Test]
    public async Task SearchPosts_ReturnsMatchingPosts()
    {
        var posts = new List<Post>
        {
            new Post { PostId = 20, UserId = 1, Content = "travel photography", Visibility = Visibility.PUBLIC }
        };
        _repoMock.Setup(r => r.SearchByContent("travel", 1, 10)).ReturnsAsync(posts);

        var result = await _sut.SearchPosts("travel", 1, 10);

        Assert.That(result.Count, Is.EqualTo(1));
    }
}
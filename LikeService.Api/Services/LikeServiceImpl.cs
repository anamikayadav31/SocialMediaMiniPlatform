public class LikeServiceImpl : ILikeService
{
    private readonly ILikeRepository _repo;
    private readonly LikeDbContext   _db;       // needed for BeginTransactionAsync
    private readonly INotifService   _notif;

    public LikeServiceImpl(ILikeRepository repo, LikeDbContext db, INotifService notif)
    {
        _repo  = repo;
        _db    = db;
        _notif = notif;
    }

    // ── Toggle ────────────────────────────────────────────────────────────────

    public async Task<ToggleLikeResultDto> ToggleLike(int userId, int targetId, TargetType targetType, int? ownerId = null)
    {
        bool alreadyLiked = await _repo.HasLiked(userId, targetId, targetType);

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            if (alreadyLiked)
            {
                await RemoveLike(userId, targetId, targetType);
                // No notification on unlike
            }
            else
            {
                await AddLike(userId, targetId, targetType);
                // Send notification only if we know the owner
                if (ownerId.HasValue)
                    await _notif.SendLikeNotifToRecipient(ownerId.Value, userId, targetId, targetType);
                else
                    await _notif.SendLikeNotif(userId, targetId, targetType);
            }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        int newCount = await _repo.CountByTargetId(targetId, targetType);
        return new ToggleLikeResultDto
        {
            Liked     = !alreadyLiked,
            LikeCount = newCount
        };
    }

    // ── Add / Remove ──────────────────────────────────────────────────────────

    public async Task AddLike(int userId, int targetId, TargetType targetType)
    {
        var like = new Like
        {
            UserId     = userId,
            TargetId   = targetId,
            TargetType = targetType,
            CreatedAt  = DateTime.UtcNow
        };
        await _repo.AddLike(like);
        await _repo.SaveChanges();
    }

    public async Task RemoveLike(int userId, int targetId, TargetType targetType)
    {
        var like = await _repo.FindByUserAndTarget(userId, targetId, targetType);
        if (like == null) return;

        await _repo.DeleteByLikeId(like.LikeId);
        // SaveChanges not needed — DeleteByLikeId uses ExecuteDeleteAsync (auto-saves)
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<LikeDto>> GetLikesByTarget(int targetId, TargetType targetType)
        => (await _repo.FindByTargetId(targetId, targetType)).Select(ToDto).ToList();

    public async Task<List<LikeDto>> GetLikesByUser(int userId)
        => (await _repo.FindByUserId(userId)).Select(ToDto).ToList();

    public async Task<int> GetLikeCount(int targetId, TargetType targetType)
        => await _repo.CountByTargetId(targetId, targetType);

    public async Task<bool> HasUserLiked(int userId, int targetId, TargetType targetType)
        => await _repo.HasLiked(userId, targetId, targetType);

    public async Task<List<int>> GetLikersForPost(int postId)
    {
        var likes = await _repo.FindByTargetId(postId, TargetType.POST);
        return likes.Select(l => l.UserId).ToList();
    }

    public async Task<List<int>> GetLikedPostsByUser(int userId)
    {
        var likes = await _repo.FindByUserId(userId);
        return likes
            .Where(l => l.TargetType == TargetType.POST)
            .Select(l => l.TargetId)
            .ToList();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────

    private static LikeDto ToDto(Like l) => new()
    {
        LikeId     = l.LikeId,
        UserId     = l.UserId,
        TargetId   = l.TargetId,
        TargetType = l.TargetType,
        CreatedAt  = l.CreatedAt
    };
}
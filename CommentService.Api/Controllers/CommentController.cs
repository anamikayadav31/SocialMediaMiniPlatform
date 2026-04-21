using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _svc;

    public CommentController(ICommentService svc) { _svc = svc; }

    // POST /api/comments
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
    {
        var result = await _svc.AddComment(dto);
        return Ok(result);
    }

    // GET /api/comments/{commentId}
    [HttpGet("{commentId:int}")]
    public async Task<IActionResult> GetById(int commentId)
    {
        var result = await _svc.GetCommentById(commentId);
        return result == null ? NotFound() : Ok(result);
    }

    // GET /api/comments/byPost/{postId}
    [HttpGet("byPost/{postId:int}")]
    public async Task<IActionResult> GetByPost(int postId)
        => Ok(await _svc.GetCommentsByPost(postId));

    // GET /api/comments/topLevel/{postId}
    [HttpGet("topLevel/{postId:int}")]
    public async Task<IActionResult> GetTopLevel(int postId)
        => Ok(await _svc.GetTopLevelComments(postId));

    // GET /api/comments/replies/{commentId}
    [HttpGet("replies/{commentId:int}")]
    public async Task<IActionResult> GetReplies(int commentId)
        => Ok(await _svc.GetReplies(commentId));

    // GET /api/comments/byUser/{userId}
    [HttpGet("byUser/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(await _svc.GetCommentsByUser(userId));

    // GET /api/comments/count/{postId}
    [HttpGet("count/{postId:int}")]
    public async Task<IActionResult> GetCount(int postId)
    {
        int count = await _svc.GetCommentCount(postId);
        return Ok(new { postId, count });
    }

    // PUT /api/comments/{commentId}
    [HttpPut("{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> EditComment(int commentId, [FromBody] EditCommentDto dto)
    {
        var result = await _svc.EditComment(commentId, dto.Content);
        return result == null ? NotFound() : Ok(result);
    }

    // DELETE /api/comments/{commentId}  (soft delete)
    [HttpDelete("{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        await _svc.DeleteComment(commentId);
        return NoContent();
    }

    // PUT /api/comments/{commentId}/like
    [HttpPut("{commentId:int}/like")]
    public async Task<IActionResult> IncrementLike(int commentId)
    {
        await _svc.IncrementLikeCount(commentId);
        return NoContent();
    }
}
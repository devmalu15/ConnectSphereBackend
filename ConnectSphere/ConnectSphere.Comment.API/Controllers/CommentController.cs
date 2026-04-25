using ConnectSphere.Comment.API.DTOs;
using ConnectSphere.Comment.API.Services;
using ConnectSphere.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConnectSphere.Comment.API.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _service;
    public CommentController(ICommentService service) => _service = service;
    private int CurrentUserId => int.Parse(
    User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ??
    User.FindFirstValue("sub") ??
    "0"
);

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] CreateCommentDto dto)
    {
        var comment = await _service.AddAsync(CurrentUserId, dto);
        return Ok(ApiResponse<CommentDto>.Ok(comment));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var comment = await _service.GetByIdAsync(id);
        return comment == null ? NotFound() :
Ok(ApiResponse<CommentDto>.Ok(comment));
    }

    [HttpGet("post/{postId:int}")]
    public async Task<IActionResult> GetByPost(int postId)
    {
        var comments = await _service.GetTopLevelByPostAsync(postId);
        return Ok(ApiResponse<IList<CommentDto>>.Ok(comments));
    }

    [HttpGet("{id:int}/replies")]
    public async Task<IActionResult> GetReplies(int id)
    {
        var replies = await _service.GetRepliesAsync(id);
        return Ok(ApiResponse<IList<CommentDto>>.Ok(replies));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [FromBody] EditCommentDto dto)
    {
        var comment = await _service.EditAsync(id, CurrentUserId, dto.Content);
        return Ok(ApiResponse<CommentDto>.Ok(comment));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id, CurrentUserId);
        return Ok(ApiResponse<string>.Ok("Comment deleted."));
    }

    // CommentController — add this
[HttpDelete("{id:int}/internal")]
public async Task<IActionResult> DeleteInternal(int id)
{
    var comment = await _service.GetByIdAsync(id);
    if (comment == null) return NotFound();
    await _service.SoftDeleteAsync(id, comment.UserId); // bypass ownership check
    return Ok(ApiResponse<string>.Ok("Comment deleted by admin."));
}
}
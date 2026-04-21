using ConnectSphere.Contracts.DTOs;
using ConnectSphere.Post.API.DTOs;
using ConnectSphere.Post.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConnectSphere.Post.API.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly IPostService _service;
    public PostController(IPostService service) => _service = service;

    // Use ClaimTypes.NameIdentifier as the primary, and "sub" as the fallback
    private int CurrentUserId => int.Parse(
        User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        "0"
    );

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromForm] CreatePostDto dto)
    {
        var post = await _service.CreateAsync(CurrentUserId, dto);
        return Ok(ApiResponse<PostDto>.Ok(post));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        int? userId = User.Identity?.IsAuthenticated == true ? CurrentUserId :
null;
        var post = await _service.GetByIdAsync(id, userId);
        return post == null ? NotFound() : Ok(ApiResponse<PostDto>.Ok(post));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int page =
1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetByUserIdAsync(userId, page, pageSize);
        return Ok(ApiResponse<PagedResult<PostDto>>.Ok(result));
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublic([FromQuery] int page = 1,
[FromQuery] int pageSize = 20)
    {
        var result = await _service.GetPublicAsync(page, pageSize);
        return Ok(ApiResponse<PagedResult<PostDto>>.Ok(result));
    }

    [HttpGet("hashtag/{tag}")]
    public async Task<IActionResult> GetByHashtag(string tag)
    {
        var posts = await _service.GetByHashtagAsync(tag);
        return Ok(ApiResponse<IList<PostDto>>.Ok(posts));
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var posts = await _service.SearchAsync(q);
        return Ok(ApiResponse<IList<PostDto>>.Ok(posts));
    }

    [HttpGet("trending")]
    public async Task<IActionResult> Trending([FromQuery] int topN = 20)
    {
        var posts = await _service.GetTrendingAsync(topN);
        return Ok(ApiResponse<IList<PostDto>>.Ok(posts));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePostDto dto)
    {
        var post = await _service.UpdateAsync(id, CurrentUserId, dto);
        return Ok(ApiResponse<PostDto>.Ok(post));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id, CurrentUserId);
        return Ok(ApiResponse<string>.Ok("Post deleted."));
    }

    [HttpPost("{id:int}/repost")]
    [Authorize]
    public async Task<IActionResult> Repost(int id)
    {
        var post = await _service.RepostAsync(id, CurrentUserId);
        return Ok(ApiResponse<PostDto>.Ok(post));
    }
}
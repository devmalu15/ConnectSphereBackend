using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Feed.API.Services; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using System.Security.Claims; 
  
namespace ConnectSphere.Feed.API.Controllers; 
  
[ApiController] 
[Route("api/feed")] 
public class FeedController : ControllerBase 
{ 
    private readonly IFeedService _service; 
    public FeedController(IFeedService service) => _service = service; 
    private int CurrentUserId => int.Parse(
    User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? 
    User.FindFirstValue("sub") ?? 
    "0"
);
        [HttpGet("{userId:int}")] 
    [Authorize] 
    public async Task<IActionResult> GetFeed(int userId, 
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        var result = await _service.GetFeedForUserAsync(userId, page, pageSize); 
        return Ok(ApiResponse<PagedResult<FeedItemDto>>.Ok(result)); 
    } 
  
        [HttpGet("suggested/{userId:int}")] 
    [Authorize] 
    public async Task<IActionResult> GetSuggested(int userId, 
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        var result = await _service.GetSuggestedFeedAsync(userId, page, pageSize); 
        return Ok(ApiResponse<IList<FeedItemDto>>.Ok(result)); 
    } 
  
        [HttpGet("explore/{userId:int}")] 
    [Authorize] 
    public async Task<IActionResult> GetExplore(int userId) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        var result = await _service.GetExploreAsync(userId); 
        return Ok(ApiResponse<IList<FeedItemDto>>.Ok(result)); 
    } 
  
        [HttpGet("trending-hashtags")] 
    public async Task<IActionResult> GetTrendingHashtags([FromQuery] int topN = 10) 
    { 
        var hashtags = await _service.GetTrendingHashtagsAsync(topN); 
        return Ok(ApiResponse<IList<string>>.Ok(hashtags)); 
    } 
  
        [HttpDelete("{userId:int}/cache")] 
    [Authorize] 
    public async Task<IActionResult> InvalidateCache(int userId) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        await _service.InvalidateFeedCacheAsync(userId); 
        return Ok(ApiResponse<string>.Ok("Cache cleared.")); 
    } 
} 
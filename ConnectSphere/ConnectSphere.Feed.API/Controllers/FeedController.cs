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
    /// <summary>Home feed — Redis-cached, from followed users</summary> 
    [HttpGet("{userId:int}")] 
    [Authorize] 
    public async Task<IActionResult> GetFeed(int userId, 
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        var result = await _service.GetFeedForUserAsync(userId, page, pageSize); 
        return Ok(ApiResponse<PagedResult<FeedItemDto>>.Ok(result)); 
    } 
  
    /// <summary> 
    /// Suggested feed — personalised by the user's liked post tag history. 
    /// Uses the tag-based recommendation algorithm. 
    /// </summary> 
    [HttpGet("suggested/{userId:int}")] 
    [Authorize] 
    public async Task<IActionResult> GetSuggested(int userId, 
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        var result = await _service.GetSuggestedFeedAsync(userId, page, pageSize); 
        return Ok(ApiResponse<IList<FeedItemDto>>.Ok(result)); 
    } 
  
    /// <summary>Explore — public posts from non-followed users</summary> 
    [HttpGet("explore/{userId:int}")] 
    [Authorize] 
    public async Task<IActionResult> GetExplore(int userId) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        var result = await _service.GetExploreAsync(userId); 
        return Ok(ApiResponse<IList<FeedItemDto>>.Ok(result)); 
    } 
  
    /// <summary>Trending hashtags (cached 30 min)</summary> 
    [HttpGet("trending-hashtags")] 
    public async Task<IActionResult> GetTrendingHashtags([FromQuery] int topN = 10) 
    { 
        var hashtags = await _service.GetTrendingHashtagsAsync(topN); 
        return Ok(ApiResponse<IList<string>>.Ok(hashtags)); 
    } 
  
    /// <summary>Manually invalidate a user's feed cache</summary> 
    [HttpDelete("{userId:int}/cache")] 
    [Authorize] 
    public async Task<IActionResult> InvalidateCache(int userId) 
    { 
        if (CurrentUserId != userId) return Forbid(); 
        await _service.InvalidateFeedCacheAsync(userId); 
        return Ok(ApiResponse<string>.Ok("Cache cleared.")); 
    } 
} 
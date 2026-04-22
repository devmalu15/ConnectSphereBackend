using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Like.API.Services; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using System.Security.Claims; 
using ConnectSphere.Like.API.DTOs;
  
namespace ConnectSphere.Like.API.Controllers; 
  
[ApiController] 
[Route("api/likes")] 
public class LikeController : ControllerBase 
{ 
    private readonly ILikeService _service; 
    public LikeController(ILikeService service) => _service = service; 
    private int CurrentUserId => int.Parse(
    User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? 
    User.FindFirstValue("sub") ?? 
    "0"
);
  
    [HttpPost("toggle")] 
    [Authorize] 
    public async Task<IActionResult> Toggle([FromBody] ToggleLikeDto dto) 
    { 
        var isLiked = await _service.ToggleLikeAsync(CurrentUserId, dto.TargetId, 
dto.TargetType); 
        return Ok(ApiResponse<bool>.Ok(isLiked)); 
    } 
  
    [HttpGet("count/{targetId:int}/{type}")] 
    public async Task<IActionResult> GetCount(int targetId, TargetType type) 
    { 
        var count = await _service.GetLikeCountAsync(targetId, type); 
        return Ok(ApiResponse<int>.Ok(count)); 
    } 
  
    [HttpGet("has/{targetId:int}/{type}")] 
    [Authorize] 
    public async Task<IActionResult> HasLiked(int targetId, TargetType type) 
    { 
        var has = await _service.HasUserLikedAsync(CurrentUserId, targetId, type); 
        return Ok(ApiResponse<bool>.Ok(has)); 
    } 
  
    [HttpGet("post/{postId:int}/likers")] 
    [Authorize] 
    public async Task<IActionResult> GetLikers(int postId) 
    { 
        var ids = await _service.GetLikerIdsAsync(postId, TargetType.POST); 
        return Ok(ApiResponse<IList<int>>.Ok(ids)); 
    } 
  
    [HttpGet("user/{userId:int}/posts")] 
    [Authorize] 
    public async Task<IActionResult> GetLikedPostsByUser(int userId) 
    { 
        var ids = await _service.GetLikedPostIdsByUserAsync(userId); 
        return Ok(ApiResponse<IList<int>>.Ok(ids)); 
    } 
} 
  

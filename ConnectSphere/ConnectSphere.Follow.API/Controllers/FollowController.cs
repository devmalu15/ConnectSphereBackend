using ConnectSphere.Contracts.DTOs;
using ConnectSphere.Follow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConnectSphere.Follow.API.DTOs;

namespace ConnectSphere.Follow.API.Controllers;

[ApiController]
[Route("api/follows")]
public class FollowController : ControllerBase
{
    private readonly IFollowService _service;
    public FollowController(IFollowService service) => _service = service;

    private int CurrentUserId => int.Parse(
   User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ??
   User.FindFirstValue("sub") ??
   "0"
);


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Follow([FromBody] FollowRequestDto dto)
    {
        var result = await _service.FollowUserAsync(CurrentUserId, dto.FolloweeId);
        return Ok(ApiResponse<object>.Ok(new
        {
            result.FollowId,
            status =
result.Status.ToString()
        }));
    }

    [HttpDelete("{followeeId:int}")]
    [Authorize]
    public async Task<IActionResult> Unfollow(int followeeId)
    {
        await _service.UnfollowAsync(CurrentUserId, followeeId);
        return Ok(ApiResponse<string>.Ok("Unfollowed."));
    }

    [HttpPut("{id:int}/accept")]
    [Authorize]
    public async Task<IActionResult> Accept(int id)
    {
        await _service.AcceptFollowRequestAsync(id, CurrentUserId);
        return Ok(ApiResponse<string>.Ok("Follow request accepted."));
    }

    [HttpPut("{id:int}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(int id)
    {
        await _service.RejectFollowRequestAsync(id, CurrentUserId);
        return Ok(ApiResponse<string>.Ok("Follow request rejected."));
    }

    [HttpDelete("{userId:int}/followers/{followerId:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveFollower(int userId, int followerId)
    {
        
        if (CurrentUserId != userId) return Forbid();

        await _service.RemoveFollowerAsync(userId, followerId);
        return Ok(ApiResponse<string>.Ok("Follower removed."));
    }

    [HttpGet("{userId:int}/followers")]
    [Authorize]
    public async Task<IActionResult> GetFollowers(int userId)
    {
        var ids = await _service.GetFollowerIdsAsync(userId);
        return Ok(ApiResponse<IList<int>>.Ok(ids));
    }

    [HttpGet("{userId:int}/following")]
    [Authorize]
    public async Task<IActionResult> GetFollowing(int userId)
    {
        var ids = await _service.GetFollowingIdsAsync(userId);
        return Ok(ApiResponse<IList<int>>.Ok(ids));
    }

    [HttpGet("{userId:int}/pending")]
    [Authorize]
    public async Task<IActionResult> GetPending(int userId)
    {
        if (CurrentUserId != userId) return Forbid();
        var requests = await _service.GetPendingRequestsAsync(userId);
        return Ok(ApiResponse<object>.Ok(requests.Select(f => new
        {
            f.FollowId,
            f.FollowerId,
            f.CreatedAt
        })));
    }

    [HttpGet("is-following/{followeeId:int}")]
    [Authorize]
    public async Task<IActionResult> IsFollowing(int followeeId)
    {
        var result = await _service.IsFollowingAsync(CurrentUserId, followeeId);
        return Ok(ApiResponse<bool>.Ok(result));
    }

    [HttpGet("mutual/{userAId:int}/{userBId:int}")]
    [Authorize]
    public async Task<IActionResult> GetMutual(int userAId, int userBId)
    {
        var ids = await _service.GetMutualFollowersAsync(userAId, userBId);
        return Ok(ApiResponse<IList<int>>.Ok(ids));
    }

    
    [HttpGet("{userId:int}/following-ids")]
    [Authorize]
    public async Task<IActionResult> GetFollowingIds(int userId)
    {
        var ids = await _service.GetFollowerIdsAsync(userId); 
        return Ok(ApiResponse<IList<int>>.Ok(ids));
    }

   
    [HttpGet("internal/{userId:int}/following-ids")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetFollowingIdsInternal(int userId)
    {
        var ids = await _service.GetFollowerIdsAsync(userId); 
        return Ok(ApiResponse<IList<int>>.Ok(ids));
    }
}


using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Implementation; 
using ConnectSphere.Follow.API.Data; 
using FollowEntity = ConnectSphere.Follow.API.Entities.Follow; 
using MassTransit; 
using Microsoft.EntityFrameworkCore; 
using ConnectSphere.Follow.API.DTOs;
  
namespace ConnectSphere.Follow.API.Services; 
  
public class FollowService : IFollowService 
{ 
    private readonly FollowDbContext _ctx; 
    private readonly IPublishEndpoint _bus; 
    private readonly IHttpClientFactory _httpFactory; 
  
    public FollowService(FollowDbContext ctx, IPublishEndpoint bus, 
IHttpClientFactory httpFactory) 
    { 
        _ctx = ctx; _bus = bus; _httpFactory = httpFactory; 
    } 
  
    public async Task<FollowResult> FollowUserAsync(int followerId, int followeeId) 
    { 
        if (followerId == followeeId) throw new InvalidOperationException("Cannot follow yourself."); 
  
        var existing = await _ctx.Follows.FirstOrDefaultAsync( 
            f => f.FollowerId == followerId && f.FolloweeId == followeeId); 
        if (existing != null) throw new InvalidOperationException("Already following."); 
  
        // Check if followee is private via Auth.API 
        var isPrivate = await IsUserPrivateAsync(followeeId); 
  
        var follow = new FollowEntity 
        { 
            FollowerId = followerId, 
            FolloweeId = followeeId, 
            Status = isPrivate ? FollowStatus.PENDING : FollowStatus.ACCEPTED, 
            AcceptedAt = isPrivate ? null : DateTime.UtcNow 
        }; 
  
        _ctx.Follows.Add(follow); 
        await _ctx.SaveChangesAsync(); 
  
        if (isPrivate) 
            await _bus.Publish(new FollowRequestedEvent(follow.FollowId, 
followerId, followeeId, true)); 
        else 
        { 
            await _bus.Publish(new FollowAcceptedEvent(follow.FollowId, followerId, 
followeeId)); 
            await _bus.Publish(new CountersUpdatedEvent(followerId, 0, 1, 0)); 
            await _bus.Publish(new CountersUpdatedEvent(followeeId, 1, 0, 0)); 
        } 
  
        return new FollowResult(follow.FollowId, follow.Status); 
    } 
  
    public async Task UnfollowAsync(int followerId, int followeeId) 
    { 
        var follow = await _ctx.Follows.FirstOrDefaultAsync( 
            f => f.FollowerId == followerId && f.FolloweeId == followeeId) 
            ?? throw new KeyNotFoundException("Follow not found."); 
  
        _ctx.Follows.Remove(follow); 
        await _ctx.SaveChangesAsync(); 
  
        if (follow.Status == FollowStatus.ACCEPTED) 
        { 
            await _bus.Publish(new UnfollowedEvent(followerId, followeeId)); 
            await _bus.Publish(new CountersUpdatedEvent(followerId, 0, -1, 0)); 
            await _bus.Publish(new CountersUpdatedEvent(followeeId, -1, 0, 0)); 
        } 
    } 
  
    public async Task AcceptFollowRequestAsync(int followId, int followeeId) 
    { 
        var follow = await _ctx.Follows.FirstOrDefaultAsync( 
            f => f.FollowId == followId && f.FolloweeId == followeeId) 
            ?? throw new KeyNotFoundException(); 
  
        await _ctx.Follows.Where(f => f.FollowId == followId) 
            .ExecuteUpdateAsync(s => s 
                .SetProperty(f => f.Status, FollowStatus.ACCEPTED) 
                .SetProperty(f => f.AcceptedAt, DateTime.UtcNow)); 
  
        await _bus.Publish(new FollowAcceptedEvent(followId, follow.FollowerId, 
followeeId)); 
        await _bus.Publish(new CountersUpdatedEvent(follow.FollowerId, 0, 1, 0)); 
        await _bus.Publish(new CountersUpdatedEvent(followeeId, 1, 0, 0)); 
    } 
  
    public async Task RejectFollowRequestAsync(int followId, int followeeId) 
    { 
        var follow = await _ctx.Follows.FirstOrDefaultAsync( 
            f => f.FollowId == followId && f.FolloweeId == followeeId) 
            ?? throw new KeyNotFoundException(); 
  
        _ctx.Follows.Remove(follow); 
        await _ctx.SaveChangesAsync(); 
        await _bus.Publish(new FollowRejectedEvent(followId, follow.FollowerId, 
followeeId)); 
    } 


    public async Task RemoveFollowerAsync(int userId, int followerId)
{
    // 1. Find the follow relationship where 'followerId' follows 'userId'
    var follow = await _ctx.Follows.FirstOrDefaultAsync(f => 
        f.FollowerId == followerId && f.FolloweeId == userId) 
        ?? throw new KeyNotFoundException("Follower relationship not found.");

    // 2. Remove the record
    _ctx.Follows.Remove(follow);
    await _ctx.SaveChangesAsync();

    // 3. Only update counters and feeds if the follow was already ACTIVE/ACCEPTED
    if (follow.Status == FollowStatus.ACCEPTED)
    {
        // Notify Feed service to break the link
        await _bus.Publish(new UnfollowedEvent(followerId, userId));

        // Decrement followers for the owner (userId)
        await _bus.Publish(new CountersUpdatedEvent(userId, -1, 0, 0));

        // Decrement following for the removed person (followerId)
        await _bus.Publish(new CountersUpdatedEvent(followerId, 0, -1, 0));
    }
}
  
    public async Task<IList<int>> GetFollowerIdsAsync(int userId) => 
        await _ctx.Follows 
            .Where(f => f.FolloweeId == userId && f.Status == 
FollowStatus.ACCEPTED) 
            .Select(f => f.FollowerId).ToListAsync(); 
  
    public async Task<IList<int>> GetFollowingIdsAsync(int userId) => 
        await _ctx.Follows 
            .Where(f => f.FollowerId == userId && f.Status == 
FollowStatus.ACCEPTED) 
            .Select(f => f.FolloweeId).ToListAsync(); 
  
    public async Task<IList<int>> GetMutualFollowersAsync(int userAId, int userBId) 
    { 
        var aFollowers = await GetFollowerIdsAsync(userAId); 
        var bFollowers = await GetFollowerIdsAsync(userBId); 
        return aFollowers.Intersect(bFollowers).ToList(); 
    } 
  
    public async Task<bool> IsFollowingAsync(int followerId, int followeeId) => 
        await _ctx.Follows.AnyAsync(f => 
            f.FollowerId == followerId && f.FolloweeId == followeeId && 
            f.Status == FollowStatus.ACCEPTED); 
  
    public async Task<IList<FollowEntity>> GetPendingRequestsAsync(int userId) => 
        await _ctx.Follows.Where(f => f.FolloweeId == userId && f.Status == 
FollowStatus.PENDING).ToListAsync(); 
  
   private async Task<bool> IsUserPrivateAsync(int userId)
{
    try
    {
        var client = _httpFactory.CreateClient("AuthService");
        var response = await client.GetAsync($"api/users/{userId}/internal");

        if (!response.IsSuccessStatusCode)
        {
            // Log this — if Auth.API is unreachable, fail SAFE by treating as private
            // This prevents auto-accepting follows when the check can't be performed
            return true;
        }

        var content = await response.Content.ReadFromJsonAsync<ApiWrapper>(
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return content?.Data?.IsPrivate ?? false;
    }
    catch
    {
        // If the HTTP call throws entirely, fail safe — treat as private
        return true;
    }
}
private record ApiWrapper(bool Success, UserData? Data, string? Message);
private record UserData(
    int UserId, string? UserName, string? FullName,
    string? AvatarUrl, string? Bio, bool IsPrivate,
    int FollowerCount, int FollowingCount, int PostCount); 
} 
  
public record FollowResult(int FollowId, ConnectSphere.Contracts.Enums.FollowStatus 
Status); 
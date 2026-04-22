using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Implementation; 
using ConnectSphere.Like.API.Data; 
using LikeEntity = ConnectSphere.Like.API.Entities.Like; 
using MassTransit; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Like.API.Services; 
  
public class LikeService : ILikeService 
{ 
    private readonly LikeDbContext _ctx; 
    private readonly IPublishEndpoint _bus; 
    private readonly IHttpClientFactory _httpFactory; 
  
    public LikeService(LikeDbContext ctx, IPublishEndpoint bus, IHttpClientFactory 
httpFactory) 
    { 
        _ctx = ctx; _bus = bus; _httpFactory = httpFactory; 
    } 
  
    public async Task<bool> ToggleLikeAsync(int userId, int targetId, TargetType 
targetType) 
    { 
        await using var tx = await _ctx.Database.BeginTransactionAsync(); 
        try 
        { 
            var existing = await _ctx.Likes.FirstOrDefaultAsync(l => 
                l.UserId == userId && l.TargetId == targetId && l.TargetType == 
targetType); 
  
            bool isLiked; 
            if (existing == null) 
            { 
                _ctx.Likes.Add(new LikeEntity { UserId = userId, TargetId = targetId, 
TargetType = targetType }); 
                await UpdateCountOnRemoteService(targetId, targetType, +1); 
                isLiked = true; 
            } 
            else 
            { 
                _ctx.Likes.Remove(existing); 
                await UpdateCountOnRemoteService(targetId, targetType, -1); 
                isLiked = false; 
            } 
  
            await _ctx.SaveChangesAsync(); 
            await tx.CommitAsync(); 
  
            if (isLiked) 
                await _bus.Publish(new LikeToggledEvent(userId, targetId, 
targetType, true)); 
  
            return isLiked; 
        } 
        catch 
        { 
            await tx.RollbackAsync(); 
            throw; 
        } 
    } 
  
    private async Task UpdateCountOnRemoteService(int targetId, TargetType 
targetType, int delta) 
    { 
        var serviceName = targetType == TargetType.POST ? "PostService" : 
"CommentService"; 
        var client = _httpFactory.CreateClient(serviceName); 
        var endpoint = targetType == TargetType.POST 
            ? $"api/posts/{targetId}/like-count?delta={delta}" 
            : $"api/comments/{targetId}/like-count?delta={delta}"; 
        await client.PatchAsync(endpoint, null); 
    } 
  
    public async Task<int> GetLikeCountAsync(int targetId, TargetType targetType) 
=> 
        await _ctx.Likes.CountAsync(l => l.TargetId == targetId && l.TargetType == 
targetType); 
  
    public async Task<bool> HasUserLikedAsync(int userId, int targetId, TargetType 
targetType) => 
        await _ctx.Likes.AnyAsync(l => l.UserId == userId && l.TargetId == targetId 
&& l.TargetType == targetType); 
  
    public async Task<IList<int>> GetLikerIdsAsync(int targetId, TargetType 
targetType) => 
        await _ctx.Likes.Where(l => l.TargetId == targetId && l.TargetType == 
targetType) 
            .Select(l => l.UserId).ToListAsync(); 
  
    public async Task<IList<int>> GetLikedPostIdsByUserAsync(int userId) => 
        await _ctx.Likes.Where(l => l.UserId == userId && l.TargetType == 
TargetType.POST) 
            .Select(l => l.TargetId).ToListAsync(); 
}
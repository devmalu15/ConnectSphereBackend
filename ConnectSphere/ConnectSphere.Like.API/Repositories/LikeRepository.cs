using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Like.API.Data; 
using LikeEntity = ConnectSphere.Like.API.Entities.Like; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Like.API.Repositories; 
  
public class LikeRepository : ILikeRepository 
{ 
    private readonly LikeDbContext _ctx; 
    public LikeRepository(LikeDbContext ctx) => _ctx = ctx; 
  
    public Task<LikeEntity?> GetAsync(int userId, int targetId, TargetType targetType) => 
        _ctx.Likes.FirstOrDefaultAsync(l => 
            l.UserId == userId && l.TargetId == targetId && l.TargetType == 
targetType); 
  
    public Task<int> CountAsync(int targetId, TargetType targetType) => 
        _ctx.Likes.CountAsync(l => l.TargetId == targetId && l.TargetType == 
targetType); 
  
    public Task<bool> ExistsAsync(int userId, int targetId, TargetType targetType) 
=> 
        _ctx.Likes.AnyAsync(l => 
            l.UserId == userId && l.TargetId == targetId && l.TargetType == 
targetType); 
  
    public Task<IList<int>> GetLikerIdsAsync(int targetId, TargetType targetType) 
=> 
        _ctx.Likes 
            .Where(l => l.TargetId == targetId && l.TargetType == targetType) 
            .Select(l => l.UserId) 
            .ToListAsync() 
            .ContinueWith<IList<int>>(t => t.Result); 
  
    public Task<IList<int>> GetLikedPostIdsByUserAsync(int userId) => 
        _ctx.Likes 
            .Where(l => l.UserId == userId && l.TargetType == TargetType.POST) 
            .Select(l => l.TargetId) 
            .ToListAsync() 
            .ContinueWith<IList<int>>(t => t.Result); 
  
    public async Task AddAsync(LikeEntity like) => await _ctx.Likes.AddAsync(like); 
  
    public Task RemoveAsync(LikeEntity like) 
    { 
        _ctx.Likes.Remove(like); 
        return Task.CompletedTask; 
    } 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
} 
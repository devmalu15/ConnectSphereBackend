using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Follow.API.Data; 
using FollowEntity = ConnectSphere.Follow.API.Entities.Follow; 
using Microsoft.EntityFrameworkCore;
using ConnectSphere.Follow.API.DTOs; 
  
namespace ConnectSphere.Follow.API.Repositories; 
  
public class FollowRepository : IFollowRepository 
{ 
    private readonly FollowDbContext _ctx; 
    public FollowRepository(FollowDbContext ctx) => _ctx = ctx; 
  
    public Task<FollowEntity?> GetAsync(int followerId, int followeeId) => 
        _ctx.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && 
f.FolloweeId == followeeId); 
  
    public Task<FollowEntity?> GetByIdAsync(int followId) => 
        _ctx.Follows.FirstOrDefaultAsync(f => f.FollowId == followId); 
  
    public Task<IList<int>> GetFollowerIdsAsync(int userId) => 
        _ctx.Follows 
            .Where(f => f.FolloweeId == userId && f.Status == 
FollowStatus.ACCEPTED) 
            .Select(f => f.FollowerId) 
            .ToListAsync() 
            .ContinueWith<IList<int>>(t => t.Result); 
  
    public Task<IList<int>> GetFollowingIdsAsync(int userId) => 
        _ctx.Follows 
            .Where(f => f.FollowerId == userId && f.Status == 
FollowStatus.ACCEPTED) 
            .Select(f => f.FolloweeId) 
            .ToListAsync() 
            .ContinueWith<IList<int>>(t => t.Result); 
  
    public Task<IList<FollowEntity>> GetPendingForUserAsync(int userId) => 
        _ctx.Follows 
            .Where(f => f.FolloweeId == userId && f.Status == FollowStatus.PENDING) 
            .ToListAsync() 
            .ContinueWith<IList<FollowEntity>>(t => t.Result); 
  
    public Task<bool> ExistsAsync(int followerId, int followeeId) => 
        _ctx.Follows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == 
followeeId); 
  
    public async Task AddAsync(FollowEntity follow) => await 
_ctx.Follows.AddAsync(follow); 
  
    public Task RemoveAsync(FollowEntity follow) 
    { 
        _ctx.Follows.Remove(follow); 
        return Task.CompletedTask; 
    } 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
} 
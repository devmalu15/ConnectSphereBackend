using ConnectSphere.Feed.API.Data; 
using ConnectSphere.Feed.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Feed.API.Repositories; 
  
public class FeedRepository : IFeedRepository 
{ 
    private readonly FeedDbContext _ctx; 
    public FeedRepository(FeedDbContext ctx) => _ctx = ctx; 
  
    public async Task<(IList<FeedItem> Items, int Total)> GetByUserAsync( 
        int userId, int page, int pageSize) 
    { 
        var query = _ctx.FeedItems 
            .Where(f => f.UserId == userId) 
            .OrderByDescending(f => f.CreatedAt); 
  
        var total = await query.CountAsync(); 
        var items = await query 
            .Skip((page - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync(); 
  
        return (items, total); 
    } 
  
    public Task<IList<FeedItem>> GetCandidatesAsync(int userId, int withinDays) 
    { 
        var since = DateTime.UtcNow.AddDays(-withinDays); 
        return _ctx.FeedItems 
            .Where(f => f.UserId == userId && f.CreatedAt >= since) 
            .ToListAsync() 
            .ContinueWith<IList<FeedItem>>(t => t.Result); 
    } 
  
    public Task<IList<int>> GetPostIdsInFeedAsync(int userId) => 
        _ctx.FeedItems 
            .Where(f => f.UserId == userId) 
            .Select(f => f.PostId) 
            .Distinct() 
            .ToListAsync() 
            .ContinueWith<IList<int>>(t => t.Result); 
  
    public Task<IList<FeedItem>> GetExploreCandidatesAsync( 
        int userId, IList<int> excludePostIds, int take = 50) => 
        _ctx.FeedItems 
            .Where(f => f.UserId != userId && !excludePostIds.Contains(f.PostId)) 
            .OrderByDescending(f => f.Score) 
            .Take(take) 
            .ToListAsync() 
            .ContinueWith<IList<FeedItem>>(t => t.Result); 
  
    public Task<bool> ExistsAsync(int userId, int postId) => 
        _ctx.FeedItems.AnyAsync(f => f.UserId == userId && f.PostId == postId); 
  
    public async Task AddAsync(FeedItem item) => 
        await _ctx.FeedItems.AddAsync(item); 
  
    public async Task<IList<int>> RemoveByPostIdAsync(int postId) 
    { 
        
        
        var affectedUserIds = await _ctx.FeedItems 
            .Where(f => f.PostId == postId) 
            .Select(f => f.UserId) 
            .Distinct() 
            .ToListAsync(); 
  
        await _ctx.FeedItems 
            .Where(f => f.PostId == postId) 
            .ExecuteDeleteAsync(); 
  
        return affectedUserIds; 
    } 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
} 
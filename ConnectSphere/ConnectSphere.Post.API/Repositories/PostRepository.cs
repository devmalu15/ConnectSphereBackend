using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Post.API.Data; 
using PostEntity = ConnectSphere.Post.API.Entities.Post; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Post.API.Repositories; 
  
public class PostRepository : IPostRepository 
{ 
    private readonly PostDbContext _ctx; 
    public PostRepository(PostDbContext ctx) => _ctx = ctx; 
  
    public Task<PostEntity?> GetByIdAsync(int postId) => 
        _ctx.Posts.FirstOrDefaultAsync(p => p.PostId == postId); 
  
    public async Task<PagedResult<PostEntity>> GetByUserIdAsync(int userId, int page, int 
pageSize) 
    { 
        var query = _ctx.Posts.Where(p => p.UserId == userId).OrderByDescending(p 
=> p.CreatedAt); 
        var total = await query.CountAsync(); 
        var items = await query.Skip((page - 1) * 
pageSize).Take(pageSize).ToListAsync(); 
        return new PagedResult<PostEntity>(items, page, pageSize, total); 
    } 
  
    public async Task<PagedResult<PostEntity>> GetPublicAsync(int page, int pageSize) 
    { 
        var query = _ctx.Posts.Where(p => p.Visibility == 
Visibility.PUBLIC).OrderByDescending(p => p.CreatedAt); 
        var total = await query.CountAsync(); 
        var items = await query.Skip((page - 1) * 
pageSize).Take(pageSize).ToListAsync(); 
        return new PagedResult<PostEntity>(items, page, pageSize, total); 
    } 
  
    public Task<IList<PostEntity>> GetByHashtagAsync(string tag) => 
        _ctx.Posts 
            .Where(p => EF.Functions.Like(p.Hashtags, $"%{tag}%") && p.Visibility 
== Visibility.PUBLIC) 
            .OrderByDescending(p => p.CreatedAt) 
            .ToListAsync() 
            .ContinueWith<IList<PostEntity>>(t => t.Result); 
  
    public Task<IList<PostEntity>> SearchAsync(string query) => 
        _ctx.Posts 
            .Where(p => EF.Functions.Like(p.Content, $"%{query}%")) 
            .OrderByDescending(p => p.CreatedAt) 
            .Take(20) 
            .ToListAsync() 
            .ContinueWith<IList<PostEntity>>(t => t.Result); 
  
    public Task<IList<PostEntity>> GetTrendingAsync(int topN) 
    { 
        var since = DateTime.UtcNow.AddHours(-24); 
        return _ctx.Posts 
            .Where(p => p.CreatedAt >= since && p.Visibility == Visibility.PUBLIC) 
            .OrderByDescending(p => (p.LikeCount * 3) + (p.CommentCount * 2) + 
(p.ShareCount * 1)) 
            .Take(topN) 
            .ToListAsync() 
            .ContinueWith<IList<PostEntity>>(t => t.Result); 
    } 
  
    public async Task AddAsync(PostEntity post) => await _ctx.Posts.AddAsync(post); 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
}
using ConnectSphere.Auth.API.Data; 
using ConnectSphere.Auth.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Auth.API.Repositories; 
  
public class UserRepository : IUserRepository 
{ 
    private readonly AuthDbContext _ctx; 
    public UserRepository(AuthDbContext ctx) => _ctx = ctx; 
  
    public Task<User?> GetByIdAsync(int userId) => 
        _ctx.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive); 
  
    public Task<User?> GetByEmailAsync(string email) => 
        _ctx.Users.FirstOrDefaultAsync(u => u.Email == email); 
  
    public Task<User?> GetByUserNameAsync(string userName) => 
        _ctx.Users.FirstOrDefaultAsync(u => u.UserName == userName); 
  
    public Task<User?> GetByOAuthAsync(string provider, string providerId) => 
        _ctx.Users.FirstOrDefaultAsync(u => u.OAuthProvider == provider && 
u.OAuthProviderId == providerId); 
  
    public Task<IList<User>> SearchAsync(string query) => 
        _ctx.Users 
            .Where(u => u.IsActive && (u.UserName.Contains(query) || 
u.FullName.Contains(query))) 
            .Take(20) 
            .ToListAsync() 
            .ContinueWith<IList<User>>(t => t.Result); 
  
    public async Task<IList<User>> GetSuggestedUsersAsync(int userId, IList<int> 
followingIds) 
    { 
        
        return await _ctx.Users 
            .Where(u => u.UserId != userId && u.IsActive && 
!followingIds.Contains(u.UserId)) 
            .OrderByDescending(u => u.FollowerCount) 
            .Take(10) 
            .ToListAsync(); 
    } 
  
    public async Task AddAsync(User user) 
    { 
        await _ctx.Users.AddAsync(user); 
    } 
  
    public Task UpdateAsync(User user) 
    { 
        _ctx.Users.Update(user); 
        return Task.CompletedTask; 
    } 
  
    public Task<bool> ExistsByEmailAsync(string email) => 
        _ctx.Users.AnyAsync(u => u.Email == email); 
  
    public Task<bool> ExistsByUserNameAsync(string userName) => 
        _ctx.Users.AnyAsync(u => u.UserName == userName); 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
}
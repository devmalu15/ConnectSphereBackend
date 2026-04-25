using ConnectSphere.Feed.API.Data; 
using ConnectSphere.Feed.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Feed.API.Repositories; 
  
public class UserTagPreferenceRepository : IUserTagPreferenceRepository 
{ 
    private readonly FeedDbContext _ctx; 
    public UserTagPreferenceRepository(FeedDbContext ctx) => _ctx = ctx; 
  
    public Task<Dictionary<string, double>> GetAffinityMapAsync(int userId) => 
        _ctx.UserTagPreferences 
            .Where(p => p.UserId == userId) 
            .ToDictionaryAsync(p => p.Tag, p => p.AffinityScore); 
  
    public Task<UserTagPreference?> GetAsync(int userId, string tag) => 
        _ctx.UserTagPreferences 
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Tag == tag); 
  
    public async Task AddAsync(UserTagPreference preference) => 
        await _ctx.UserTagPreferences.AddAsync(preference); 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
} 
using ConnectSphere.Feed.API.Entities; 
  
namespace ConnectSphere.Feed.API.Repositories; 
  
public interface IUserTagPreferenceRepository 
{ 
    /// <summary>Returns all tag affinities for a user as a dictionary: tag -> score.</summary> 
    Task<Dictionary<string, double>> GetAffinityMapAsync(int userId); 
  
    Task<UserTagPreference?> GetAsync(int userId, string tag); 
  
    Task AddAsync(UserTagPreference preference); 
  
    Task SaveChangesAsync(); 
}
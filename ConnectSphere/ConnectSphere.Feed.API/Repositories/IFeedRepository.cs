using ConnectSphere.Feed.API.Entities; 
  
namespace ConnectSphere.Feed.API.Repositories; 
  
public interface IFeedRepository 
{ 
  
    Task<(IList<FeedItem> Items, int Total)> GetByUserAsync(int userId, int page, 
int pageSize); 
  
    /// <summary> 
    
    /// Used by the tag-based suggestion algorithm as the candidate pool. 
    /// </summary> 
    Task<IList<FeedItem>> GetCandidatesAsync(int userId, int withinDays); 
  
    /// <summary>Returns all post IDs currently in a user's feed.</summary> 
    Task<IList<int>> GetPostIdsInFeedAsync(int userId); 
  
   
    Task<IList<FeedItem>> GetExploreCandidatesAsync(int userId, IList<int> 
excludePostIds, int take = 50); 
  
    Task<bool> ExistsAsync(int userId, int postId); 
  
    Task AddAsync(FeedItem item); 
  
    /// <summary>Removes all feed items for a specific post (used when a post is deleted).</summary> 
    Task<IList<int>> RemoveByPostIdAsync(int postId); 
  
    Task SaveChangesAsync(); 
} 
using ConnectSphere.Feed.API.Entities; 
  
namespace ConnectSphere.Feed.API.Repositories; 
  
public interface IFeedRepository 
{ 
  
    Task<(IList<FeedItem> Items, int Total)> GetByUserAsync(int userId, int page, 
int pageSize); 
  
        
        Task<IList<FeedItem>> GetCandidatesAsync(int userId, int withinDays); 
  
        Task<IList<int>> GetPostIdsInFeedAsync(int userId); 
  
   
    Task<IList<FeedItem>> GetExploreCandidatesAsync(int userId, IList<int> 
excludePostIds, int take = 50); 
  
    Task<bool> ExistsAsync(int userId, int postId); 
  
    Task AddAsync(FeedItem item); 
  
        Task<IList<int>> RemoveByPostIdAsync(int postId); 
  
    Task SaveChangesAsync(); 
} 
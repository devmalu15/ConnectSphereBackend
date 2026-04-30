using ConnectSphere.Contracts.DTOs; 
  
namespace ConnectSphere.Feed.API.Services; 
  
public interface IFeedService 
{ 
    Task<PagedResult<FeedItemDto>> GetFeedForUserAsync(int userId, int page, int 
pageSize); 
    Task<IList<FeedItemDto>> GetSuggestedFeedAsync(int userId, int page, int 
pageSize); 
    Task<IList<FeedItemDto>> GetExploreAsync(int userId); 
    Task<IList<string>> GetTrendingHashtagsAsync(int topN = 10); 
    Task AddToFeedAsync(int userId, int postId, int actorId, decimal score); 
    Task InvalidateFeedCacheAsync(int userId); 
    Task RemoveFromFeedsAsync(int postId); 
    Task BackfillFeedAsync(int recipientUserId, int followeeId); 
  
        Task UpdateTagPreferencesOnLikeAsync(int userId, int postId); 
} 
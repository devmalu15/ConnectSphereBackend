using FollowEntity = ConnectSphere.Follow.API.Entities.Follow; 
  
namespace ConnectSphere.Follow.API.Services; 
  
public interface IFollowService 
{ 
    Task<FollowResult> FollowUserAsync(int followerId, int followeeId); 
    Task UnfollowAsync(int followerId, int followeeId); 
    Task AcceptFollowRequestAsync(int followId, int followeeId); 
    Task RejectFollowRequestAsync(int followId, int followeeId); 
    Task RemoveFollowerAsync(int userId, int followerId);
    Task<IList<int>> GetFollowerIdsAsync(int userId); 
    Task<IList<int>> GetFollowingIdsAsync(int userId); 
    Task<IList<int>> GetMutualFollowersAsync(int userAId, int userBId); 
    Task<bool> IsFollowingAsync(int followerId, int followeeId); 
    Task<IList<FollowEntity>> GetPendingRequestsAsync(int userId); 
} 
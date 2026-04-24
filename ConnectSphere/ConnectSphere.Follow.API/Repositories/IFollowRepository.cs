using ConnectSphere.Contracts.Enums; 
using FollowEntity = ConnectSphere.Follow.API.Entities.Follow; 
  
namespace ConnectSphere.Follow.API.Repositories; 
  
public interface IFollowRepository 
{ 
    Task<FollowEntity?> GetAsync(int followerId, int followeeId); 
    Task<FollowEntity?> GetByIdAsync(int followId); 
    Task<IList<int>> GetFollowerIdsAsync(int userId); 
    Task<IList<int>> GetFollowingIdsAsync(int userId); 
    Task<IList<FollowEntity>> GetPendingForUserAsync(int userId); 
    Task<bool> ExistsAsync(int followerId, int followeeId); 
    Task AddAsync(FollowEntity follow); 
    Task RemoveAsync(FollowEntity follow); 
    Task SaveChangesAsync(); 
} 
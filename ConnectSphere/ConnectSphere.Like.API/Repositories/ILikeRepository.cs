using ConnectSphere.Contracts.Enums; 
using LikeEntity = ConnectSphere.Like.API.Entities.Like; 
  
namespace ConnectSphere.Like.API.Repositories; 
  
public interface ILikeRepository 
{ 
    Task<LikeEntity?> GetAsync(int userId, int targetId, TargetType targetType); 
    Task<int> CountAsync(int targetId, TargetType targetType); 
    Task<bool> ExistsAsync(int userId, int targetId, TargetType targetType); 
    Task<IList<int>> GetLikerIdsAsync(int targetId, TargetType targetType); 
    Task<IList<int>> GetLikedPostIdsByUserAsync(int userId); 
    Task AddAsync(LikeEntity like); 
    Task RemoveAsync(LikeEntity like); 
    Task SaveChangesAsync(); 
} 
using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Like.API.Services; 
  
public interface ILikeService 
{ 
    Task<bool> ToggleLikeAsync(int userId, int targetId, TargetType targetType); 
    Task<int> GetLikeCountAsync(int targetId, TargetType targetType); 
    Task<bool> HasUserLikedAsync(int userId, int targetId, TargetType targetType); 
    Task<IList<int>> GetLikerIdsAsync(int targetId, TargetType targetType); 
    Task<IList<int>> GetLikedPostIdsByUserAsync(int userId); 
}
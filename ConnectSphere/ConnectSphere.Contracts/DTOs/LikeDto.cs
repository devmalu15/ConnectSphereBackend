using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.DTOs; 
  
public record LikeDto(int LikeId, int UserId, int TargetId, TargetType TargetType, 
DateTime CreatedAt);
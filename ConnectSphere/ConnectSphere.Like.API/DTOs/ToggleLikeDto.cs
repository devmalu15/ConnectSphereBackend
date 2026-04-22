using ConnectSphere.Contracts.Enums; 
 
  
namespace ConnectSphere.Like.API.DTOs; 

public record ToggleLikeDto(int TargetId, TargetType TargetType);
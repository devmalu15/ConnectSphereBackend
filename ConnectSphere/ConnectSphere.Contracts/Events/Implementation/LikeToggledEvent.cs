using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record LikeToggledEvent(int UserId, int TargetId, TargetType TargetType, 
bool IsLiked) : ILikeToggledEvent; 
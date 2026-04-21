using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record LikeToggledEvent(int UserId, int TargetId, TargetType TargetType, 
bool IsLiked) : ILikeToggledEvent; 
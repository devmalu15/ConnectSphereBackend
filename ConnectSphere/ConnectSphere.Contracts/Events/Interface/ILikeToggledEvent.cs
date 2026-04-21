using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface ILikeToggledEvent 
{ 
    int UserId { get; } 
    int TargetId { get; } 
    TargetType TargetType { get; } 
    bool IsLiked { get; } 
} 
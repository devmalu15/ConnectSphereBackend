using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.DTOs; 
  
public record NotificationDto( 
    int NotificationId, int RecipientId, int ActorId, 
    NotifType Type, string Message, int? TargetId, 
    TargetType? TargetType, bool IsRead, DateTime CreatedAt); 
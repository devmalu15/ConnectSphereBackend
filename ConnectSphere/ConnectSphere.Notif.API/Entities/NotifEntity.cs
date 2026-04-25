using ConnectSphere.Contracts.Enums; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Notif.API.Entities; 
  
[Index(nameof(RecipientId), nameof(IsRead))] 
public class Notification 
{ 
    public int NotificationId { get; set; } 
    public int RecipientId { get; set; } 
    public int ActorId { get; set; } 
    public NotifType Type { get; set; } 
    public string Message { get; set; } = string.Empty; 
    public int? TargetId { get; set; } 
    public TargetType? TargetType { get; set; } 
    public bool IsRead { get; set; } = false; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
} 
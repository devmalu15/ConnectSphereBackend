using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Notif.API.Services; 
  
public interface INotifService 
{ 
    Task<PagedResult<NotificationDto>> GetByRecipientAsync(int recipientId, int 
page, int pageSize); 
    Task<int> GetUnreadCountAsync(int recipientId); 
    Task MarkReadAsync(int notificationId, int recipientId); 
    Task MarkAllReadAsync(int recipientId); 
    Task DeleteAsync(int notificationId, int recipientId); 
    Task SendAsync(int recipientId, int actorId, NotifType type, string message, 
int? targetId, TargetType? targetType); 
    Task SendBulkAsync(IList<int> recipientIds, int actorId, NotifType type, string 
message); 
} 
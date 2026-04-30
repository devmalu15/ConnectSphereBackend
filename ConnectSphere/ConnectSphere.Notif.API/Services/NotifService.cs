using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Notif.API.Data; 
using ConnectSphere.Notif.API.Entities; 
using ConnectSphere.Notif.API.Repositories; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Notif.API.Services; 
  
public class NotifService : INotifService 
{ 
    private readonly INotifRepository _repo; 
    private readonly NotifDbContext _ctx; 
  
    public NotifService(INotifRepository repo, NotifDbContext ctx) 
    { 
        _repo = repo; _ctx = ctx; 
    } 
  
    public async Task<PagedResult<NotificationDto>> GetByRecipientAsync(int 
recipientId, int page, int pageSize) 
    { 
        var total = await _ctx.Notifications.CountAsync(n => n.RecipientId == 
recipientId); 
        var items = await _repo.GetByRecipientAsync(recipientId, page, pageSize); 
        var dtos = items.Select(ToDto).ToList(); 
        return new PagedResult<NotificationDto>(dtos, page, pageSize, total); 
    } 
  
    public Task<int> GetUnreadCountAsync(int recipientId) => 
        _repo.CountUnreadAsync(recipientId); 
  
    public async Task MarkReadAsync(int notificationId, int recipientId) 
    { 
        await _ctx.Notifications 
            .Where(n => n.NotificationId == notificationId && n.RecipientId == 
recipientId) 
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true)); 
    } 
  
    public async Task MarkAllReadAsync(int recipientId) 
    { 
        await _ctx.Notifications 
            .Where(n => n.RecipientId == recipientId && !n.IsRead) 
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true)); 
    } 
  
    public async Task DeleteAsync(int notificationId, int recipientId) 
    { 
        await _ctx.Notifications 
            .Where(n => n.NotificationId == notificationId && n.RecipientId == 
recipientId) 
            .ExecuteDeleteAsync(); 
    } 
  
    public async Task SendAsync(int recipientId, int actorId, NotifType type, 
        string message, int? targetId, TargetType? targetType) 
    { 
        if (recipientId == actorId) return; 
  
        await _repo.AddAsync(new Notification 
        { 
            RecipientId = recipientId, 
            ActorId = actorId, 
            Type = type, 
            Message = message, 
            TargetId = targetId, 
            TargetType = targetType 
        }); 
        await _repo.SaveChangesAsync(); 
    } 
  
    public async Task SendBulkAsync(IList<int> recipientIds, int actorId, NotifType 
type, string message) 
    { 
        var notifications = recipientIds 
            .Where(id => id != actorId) 
            .Select(id => new Notification 
            { 
                RecipientId = id, 
                ActorId = actorId, 
                Type = type, 
                Message = message 
            }).ToList(); 
  
        await _repo.AddRangeAsync(notifications); 
        await _repo.SaveChangesAsync(); 
    } 
  
    private static NotificationDto ToDto(Notification n) => new( 
        n.NotificationId, n.RecipientId, n.ActorId, n.Type, 
        n.Message, n.TargetId, n.TargetType, n.IsRead, n.CreatedAt); 
}
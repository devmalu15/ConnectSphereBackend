using ConnectSphere.Notif.API.Data; 
using ConnectSphere.Notif.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Notif.API.Repositories; 
  
public class NotifRepository : INotifRepository 
{ 
    private readonly NotifDbContext _ctx; 
    public NotifRepository(NotifDbContext ctx) => _ctx = ctx; 
  
    public Task<IList<Notification>> GetByRecipientAsync(int recipientId, int page, 
int pageSize) => 
        _ctx.Notifications 
            .Where(n => n.RecipientId == recipientId) 
            .OrderByDescending(n => n.CreatedAt) 
            .Skip((page - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync() 
            .ContinueWith<IList<Notification>>(t => t.Result); 
  
    public Task<IList<Notification>> GetUnreadByRecipientAsync(int recipientId) => 
        _ctx.Notifications 
            .Where(n => n.RecipientId == recipientId && !n.IsRead) 
            .OrderByDescending(n => n.CreatedAt) 
            .ToListAsync() 
            .ContinueWith<IList<Notification>>(t => t.Result); 
  
    public Task<int> CountUnreadAsync(int recipientId) => 
        _ctx.Notifications.CountAsync(n => n.RecipientId == recipientId && 
!n.IsRead); 
  
    public async Task AddAsync(Notification notification) => 
        await _ctx.Notifications.AddAsync(notification); 
  
    public async Task AddRangeAsync(IList<Notification> notifications) => 
        await _ctx.Notifications.AddRangeAsync(notifications); 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
} 
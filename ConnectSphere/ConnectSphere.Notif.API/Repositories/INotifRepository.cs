using ConnectSphere.Notif.API.Entities; 
  
namespace ConnectSphere.Notif.API.Repositories; 
  
public interface INotifRepository 
{ 
    Task<IList<Notification>> GetByRecipientAsync(int recipientId, int page, int 
pageSize); 
    Task<IList<Notification>> GetUnreadByRecipientAsync(int recipientId); 
    Task<int> CountUnreadAsync(int recipientId); 
    Task AddAsync(Notification notification); 
    Task AddRangeAsync(IList<Notification> notifications); 
    Task SaveChangesAsync(); 
} 
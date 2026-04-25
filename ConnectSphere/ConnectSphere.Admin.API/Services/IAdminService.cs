using ConnectSphere.Admin.API.Entities; 
  
namespace ConnectSphere.Admin.API.Services; 
  
public interface IAdminService 
{ 
    Task UnsuspendUserAsync(int adminId, string adminName, int userId, string ip); 
    Task DeletePostAsync(int adminId, string adminName, int postId, string ip); 
    Task DeleteCommentAsync(int adminId, string adminName, int commentId, string 
ip); 
    Task<IList<AuditLog>> GetAuditLogsAsync(DateTime? from, DateTime? to, int page, 
int pageSize); 

    Task<object> GetAnalyticsAsync(string token);

    Task SuspendUserAsync(int adminId, string adminName, int userId, string ip, string token);
} 
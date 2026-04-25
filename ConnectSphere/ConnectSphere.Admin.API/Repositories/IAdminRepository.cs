using ConnectSphere.Admin.API.Entities; 
  
namespace ConnectSphere.Admin.API.Repositories; 
  
public interface IAdminRepository 
{ 
    Task<IList<AuditLog>> GetLogsAsync( 
        DateTime? from, DateTime? to, int page, int pageSize); 
  
    Task<IList<AuditLog>> GetLogsByActorAsync(int actorId, int page, int pageSize); 
  
    Task<IList<AuditLog>> GetLogsByEntityAsync( 
        string entityType, string entityId, int page, int pageSize); 
  
    Task<int> CountLogsAsync(DateTime? from, DateTime? to); 
  
    Task AddAsync(AuditLog log); 
  
    Task SaveChangesAsync(); 
} 
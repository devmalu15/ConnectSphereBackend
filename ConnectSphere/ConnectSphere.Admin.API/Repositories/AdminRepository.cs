using ConnectSphere.Admin.API.Data; 
using ConnectSphere.Admin.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Admin.API.Repositories; 
  
public class AdminRepository : IAdminRepository 
{ 
    private readonly AdminDbContext _ctx; 
    public AdminRepository(AdminDbContext ctx) => _ctx = ctx; 
  
    public async Task<IList<AuditLog>> GetLogsAsync( 
        DateTime? from, DateTime? to, int page, int pageSize) 
    { 
        var query = _ctx.AuditLogs.AsQueryable(); 
  
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value); 
        if (to.HasValue)   query = query.Where(a => a.CreatedAt <= to.Value); 
  
        return await query 
            .OrderByDescending(a => a.CreatedAt) 
            .Skip((page - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync(); 
    } 
  
    public Task<IList<AuditLog>> GetLogsByActorAsync(int actorId, int page, int 
pageSize) => 
        _ctx.AuditLogs 
            .Where(a => a.ActorId == actorId) 
            .OrderByDescending(a => a.CreatedAt) 
            .Skip((page - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync() 
            .ContinueWith<IList<AuditLog>>(t => t.Result); 
  
    public Task<IList<AuditLog>> GetLogsByEntityAsync( 
        string entityType, string entityId, int page, int pageSize) => 
        _ctx.AuditLogs 
            .Where(a => a.EntityType == entityType && a.EntityId == entityId) 
            .OrderByDescending(a => a.CreatedAt) 
            .Skip((page - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync() 
            .ContinueWith<IList<AuditLog>>(t => t.Result); 
  
    public Task<int> CountLogsAsync(DateTime? from, DateTime? to) 
    { 
        var query = _ctx.AuditLogs.AsQueryable(); 
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value); 
        if (to.HasValue)   query = query.Where(a => a.CreatedAt <= to.Value); 
        return query.CountAsync(); 
    } 
  
    public async Task AddAsync(AuditLog log) => 
        await _ctx.AuditLogs.AddAsync(log); 
  
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync(); 
}
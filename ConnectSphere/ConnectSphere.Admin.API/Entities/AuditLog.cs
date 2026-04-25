using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Admin.API.Entities; 
  
[Index(nameof(CreatedAt))] 
[Index(nameof(ActorId))] 
public class AuditLog 
{ 
    public int AuditLogId { get; set; } 
    public int ActorId { get; set; } 
    public string ActorUserName { get; set; } = string.Empty; 
    public string Action { get; set; } = string.Empty; 
    public string EntityType { get; set; } = string.Empty; 
    public string EntityId { get; set; } = string.Empty; 
    public string? BeforeValue { get; set; } 
    public string? AfterValue { get; set; } 
    public string? IpAddress { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
} 
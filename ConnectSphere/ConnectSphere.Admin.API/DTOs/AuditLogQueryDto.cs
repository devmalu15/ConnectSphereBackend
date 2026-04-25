using ConnectSphere.Contracts.Enums; 
using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Admin.API.DTOs; 
  

public record AuditLogQueryDto( 
    DateTime? From, 
    DateTime? To, 
    int Page = 1, 
    int PageSize = 20 
); 
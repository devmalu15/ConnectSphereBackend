using ConnectSphere.Contracts.Enums; 
using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Admin.API.DTOs; 
  
public record BroadcastRequestDto( 
    [Required] string Title, 
    [Required] string Message, 
    [Required] IList<int> UserIds, 
    NotifType Type 
);
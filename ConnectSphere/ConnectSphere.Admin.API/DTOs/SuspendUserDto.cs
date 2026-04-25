using ConnectSphere.Contracts.Enums; 
using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Admin.API.DTOs; 
  



public record SuspendUserDto( 
    [Required] string Reason 
); 
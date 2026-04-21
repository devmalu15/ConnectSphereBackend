using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.DTOs; 

public record ChangePasswordDto( 
    
    [Required] string CurrentPassword,

    [Required][MinLength(8)] string NewPassword 
); 
  
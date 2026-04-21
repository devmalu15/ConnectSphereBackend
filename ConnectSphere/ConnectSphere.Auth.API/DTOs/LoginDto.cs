using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.DTOs; 

public record LoginDto( 
    [Required] string Email, 
    [Required] string Password 
); 
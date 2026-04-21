using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.DTOs; 
  
public record RegisterDto( 
    [Required][MaxLength(50)] string UserName, 
    [Required][MaxLength(100)] string FullName, 
    [Required][EmailAddress] string Email, 
    [Required][MinLength(8)] string Password 
); 
  
